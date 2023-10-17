using System.Collections;
using System.Collections.Generic;
using Common;
using Unity.Assertions;
using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Analytics;
using Assert = UnityEngine.Assertions.Assert;
using Random = Unity.Mathematics.Random;

namespace DOTS
{
    
[UpdateAfter(typeof(BoidSpawnSystem))]
[UpdateAfter(typeof(PlayerSpawnerSystem))]
public partial struct BoidBehaviourSystem : ISystem
{
    Random random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoidConfig>();
        random = new Random(123456);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var boidConfig = SystemAPI.GetSingleton<BoidConfig>();
        if (!boidConfig.runSystem)
            return;
        
        var deltaTime = Time.deltaTime;

        #region Boid Data Region
        
        // movement data
        float moveSpeed = boidConfig.moveSpeed;
        float chaseSpeedModifier = boidConfig.chaseSpeedModifier;
            
        // neighbour data
        float maxNeighbourDistanceSquared = boidConfig.neighbourDistanceSquared;
        float halfFOVInRadians = boidConfig.halfFovInRadians;
            
        // target data
        float targetVisionDistanceSquared = boidConfig.targetVisionDistanceSquared;
        LinearSteering targetLinearSteering = boidConfig.targetLinearSteering;
        AngularSteering targetAngularSteering = boidConfig.TargetAngularSteering;
        
        // wander data
        var wanderParameters = boidConfig.wanderParameters;
        LinearSteering wanderLinearSteering = boidConfig.wanderLinearSteering;
        AngularSteering wanderAngularSteering = boidConfig.WanderAngularSteering;

        // alignment data
        AngularSteering alignmentAngularSteering = boidConfig.AlignAngularSteering;
                
        // cohesion data
        LinearSteering cohesionLinearSteering = boidConfig.cohesionLinearSteering;
                
        // separation data
        LinearSteering separationLinearSteering = boidConfig.separationLinearSteering;
        float minSeparationDistanceSquared = boidConfig.separationDistanceSquared;
        float separationDecayCoefficient = boidConfig.separationDecayCoefficient;
                
        // obstacle data
        LinearSteering obstacleLinearSteering = boidConfig.obstacleLinearSteering;
        float obstacleAvoidanceDistanceSquared = boidConfig.obstacleAvoidanceDistanceSquared;
        
        #endregion
        #region Query Region

        // queries (filters to select entities based on a specific set of components)
        EntityQuery boidQuery = SystemAPI.QueryBuilder().
            WithAll<Boid>().
            WithAllRW<VelocityComponent, RotationSpeedComponent>().
            WithAllRW<LocalTransform>().
            Build();

        EntityQuery targetQuery = SystemAPI.QueryBuilder().WithAll<BoidTarget, LocalTransform>().Build();
        EntityQuery obstacleQuery = SystemAPI.QueryBuilder().WithAll<Obstacle, LocalTransform>().Build();
        
        // number of different objects
        int boidsCount = boidQuery.CalculateEntityCount();
        int targetCount = targetQuery.CalculateEntityCount();
        int obstacleCount = obstacleQuery.CalculateEntityCount();

        #endregion
        #region Native Array Region

        // empty native array with float4x4 for new boid positions, which are later assigned to the boids
        NativeArray<float4x4> newBoidLTWMatrices = new NativeArray<float4x4>(boidsCount, Allocator.TempJob);
        
        // to store boid data
        NativeArray<float2> initialBoidPositions = new NativeArray<float2>(boidsCount, Allocator.TempJob);
        NativeArray<float2> initialBoidVelocities = new NativeArray<float2>(boidsCount, Allocator.TempJob);
        NativeArray<float> initialBoidOrientations = new NativeArray<float>(boidsCount, Allocator.TempJob);
        NativeArray<float2> initialBoidOrientationVectors = new NativeArray<float2>(boidsCount, Allocator.TempJob);
        NativeArray<float> initialBoidRotationSpeeds = new NativeArray<float>(boidsCount, Allocator.TempJob);
        
        // native arrays for new values for velocities and rotations
        NativeArray<float2> newVelocities = new NativeArray<float2>(boidsCount, Allocator.TempJob);
        NativeArray<float> newRotationSpeeds = new NativeArray<float>(boidsCount, Allocator.TempJob);
        
        // to store neighbour data
        NativeArray<float2> averageNeighbourPositions = new NativeArray<float2>(boidsCount, Allocator.TempJob);
        NativeArray<float> averageNeighbourOrientations = new NativeArray<float>(boidsCount, Allocator.TempJob);
        
        // to store forces from other boids
        // TODO: Make empty if separation weight is 0 (?)
        NativeArray<float2> separationForces = new NativeArray<float2>(boidsCount, Allocator.TempJob);

        // to store forces from different boid rules
        NativeArray<float2> fromOtherBoidsForces = new NativeArray<float2>(boidsCount, Allocator.TempJob);
        NativeArray<float2> targetForces = new NativeArray<float2>(boidsCount, Allocator.TempJob);
        NativeArray<float2> obstacleForces = new NativeArray<float2>(boidsCount, Allocator.TempJob);
        
        // to store target and obstacle positions
        NativeArray<float2> targetPositions = new NativeArray<float2>(targetCount, Allocator.TempJob);
        NativeArray<float2> obstaclePositions = new NativeArray<float2>(obstacleCount, Allocator.TempJob);

        #endregion
        
        // set all initial boid data
        // TODO: change query to read only components, TODO: convert to job?
        int index = 0;
        foreach (var (velocity, rotation, localToWorld) in SystemAPI.Query<RefRO<VelocityComponent>, RefRO<RotationSpeedComponent>, 
            RefRW<LocalTransform>>().WithAll<Boid>())
        {
            float3 forward = localToWorld.ValueRO.Forward();
            float2 orientationVector = new float2(forward.x, forward.z);
            float orientation = MathUtility.DirectionToFloat(orientationVector);
            orientation = MathUtility.MapToRange0To2Pie(orientation);
            
            initialBoidPositions[index] = localToWorld.ValueRO.Position.xz;
            initialBoidOrientations[index] = orientation;
            initialBoidOrientationVectors[index] = orientationVector;
            initialBoidVelocities[index] =  velocity.ValueRO.Value;
            initialBoidRotationSpeeds[index] =  rotation.ValueRO.Value;
            
            index++;
        }

        // set all neighbour data
        // TODO: convert to job?
        for (index = 0; index < boidsCount; index++)
        {
            float2 averagePos = new float2();
            float averageOrientation = 0;
            
            float2 averageOrientationVector = float2.zero;
            
            float2 boidPos = initialBoidPositions[index];
            float boidOrientation = initialBoidOrientations[index];
            int neighbourCount = 0;
            
            for (int otherIndex = 0; otherIndex < boidsCount; otherIndex++)
            {
                // skip if same index
                if (index == otherIndex) continue;

                float2 otherPos = initialBoidPositions[otherIndex];
                
                // ignore if outside of view range
                float squareDistance = math.distancesq(boidPos, otherPos);
                if (squareDistance > maxNeighbourDistanceSquared) continue;

                float2 directionToOther = otherPos - boidPos;
                float2 directionNormalized = math.normalize(directionToOther);
                float rotationToOther = MathUtility.DirectionToFloat(directionNormalized);
                rotationToOther = MathUtility.MapToRange0To2Pie(rotationToOther);
                float deltaOrientation = boidOrientation - rotationToOther;
                
                // ignore if outside FOV
                if (math.abs(deltaOrientation) > halfFOVInRadians) continue;

                // finally, add position and orientation to average
                averagePos += otherPos;
                averageOrientationVector += initialBoidOrientationVectors[otherIndex];
                
                // update separation forces
                if (squareDistance < minSeparationDistanceSquared)
                {
                    float strength = math.min(separationDecayCoefficient / (squareDistance), separationLinearSteering.maxAcceleration);
                    separationForces[index] -= strength * directionNormalized;
                }

                neighbourCount++;
            }
            
            // set average positions and orientations
            if (neighbourCount > 0)
            {
                averageOrientationVector /= neighbourCount;
                
                // convert average velocity vector back to radians
                averageOrientation = MathUtility.DirectionToFloat(averageOrientationVector);
                averageNeighbourOrientations[index] = averageOrientation;
                
                averagePos /= neighbourCount;
                averageNeighbourPositions[index] = averagePos;
            }
        }
        
        // set boid target positions
        index = 0;
        foreach (var targetTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<BoidTarget>())
        {
            targetPositions[index] = targetTransform.ValueRO.Position.xz;
            index++;
        }
        
        // set obstacle positions
        index = 0;
        foreach (var obstacleTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<Obstacle>())
        {
            obstaclePositions[index] = obstacleTransform.ValueRO.Position.xz;
            index++;
        }

        // loop over all boids, update their positions 
        index = 0;
        foreach (var (transform, velocity, rotationSpeed ) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRW<VelocityComponent>, RefRW<RotationSpeedComponent> >().WithAll<Boid>())
        {
            // total stear outputs
            float2 totalLinearOutput = float2.zero;
            float totalAngularOutput = 0f;

            // fetch the boid data
            float boidOrientatation = initialBoidOrientations[index];
            float boidRotationSpeed = rotationSpeed.ValueRO.Value;
            float2 position = initialBoidPositions[index];

            // prioritize system:
            // 1. if sees target, chase it
            totalLinearOutput += GetSeekLinearOutput(position, targetVisionDistanceSquared, targetPositions, targetLinearSteering, out float directionAsOrientation, out bool targetFound);
            totalAngularOutput += GetSeekAngularOutput(boidOrientatation, boidRotationSpeed, directionAsOrientation, targetAngularSteering, targetFound);
            
            // 2. else, wander around
            GetWanderOut(boidOrientatation, boidRotationSpeed, targetFound, wanderParameters, wanderLinearSteering, wanderAngularSteering, out float2 linearWander, out float angularWander);
            totalLinearOutput += linearWander;
            totalAngularOutput += angularWander;

            // 3. if has neighbours, use alignment and cohesion
            bool checkAlignAndCohesion = !targetFound && averageNeighbourOrientations[index] != 0;
            float2 directionToAvergae = averageNeighbourPositions[index] - position;
            float averageOrientation = averageNeighbourOrientations[index];
            totalLinearOutput += GetCohesionOutput(checkAlignAndCohesion, directionToAvergae, cohesionLinearSteering);
            totalAngularOutput += GetAlignmentOutput(checkAlignAndCohesion, boidOrientatation, boidRotationSpeed, averageOrientation, alignmentAngularSteering);
            
            // 4. always check for separation and obstacles
            totalLinearOutput += GetSeparatationSteering(separationForces[index], separationLinearSteering, targetFound);
            totalLinearOutput += GetObstacleSteering(position, obstacleAvoidanceDistanceSquared, obstaclePositions, obstacleLinearSteering);
            
            // update position based on current velocity
            transform.ValueRW.Position += MathUtility.Float2ToFloat3(velocity.ValueRO.Value) * deltaTime;

            // update rotation based on current rotationSpeed
            quaternion rotationDelta = quaternion.RotateY(-boidRotationSpeed * deltaTime);
            transform.ValueRW.Rotation = math.mul(transform.ValueRO.Rotation, rotationDelta);
            
            // update the current velocity based on linear steer output
            velocity.ValueRW.Value += totalLinearOutput * moveSpeed * deltaTime;
            float2 velocityRO = velocity.ValueRO.Value;

            float maxMoveSpeed = moveSpeed * (targetFound ? chaseSpeedModifier : 1);
            if (math.length(velocityRO) > maxMoveSpeed)
                velocity.ValueRW.Value = math.normalize(velocityRO) * maxMoveSpeed;
               
            // update the current rotationSpeed based on angular output
            rotationSpeed.ValueRW.Value += totalAngularOutput;

            index++;
        }
        

        #region Dispose Region

        // Dispose native arrays
        newBoidLTWMatrices.Dispose();
        
        initialBoidPositions.Dispose();
        initialBoidVelocities.Dispose();
        initialBoidOrientations.Dispose();
        initialBoidRotationSpeeds.Dispose();
        initialBoidOrientationVectors.Dispose();
        
        newVelocities.Dispose();
        newRotationSpeeds.Dispose();
        
        averageNeighbourPositions.Dispose();
        averageNeighbourOrientations.Dispose();
        
        separationForces.Dispose();

        fromOtherBoidsForces.Dispose();
        targetForces.Dispose();
        obstacleForces.Dispose();
        
        targetPositions.Dispose();
        obstaclePositions.Dispose();

        #endregion
        
    }

    private float2 GetObstacleSteering(float2 position, float avoidDisSquared, NativeArray<float2> obstacles, LinearSteering steering)
    {
        bool obstalceFound = GetDirectionToClosest(position, obstacles, avoidDisSquared, out var direction);

        if (obstalceFound)
        {
            return -GetLinearOutput(direction, steering) * steering.weight;
        }
        else
        {
            return float2.zero;
        }
    }

    private float2 GetSeparatationSteering(float2 separationForce, LinearSteering separationLinearSteering, bool targetFound)
    {
        if (separationForce.Equals(float2.zero))
            return float2.zero;

        float targetWeightModifier = targetFound ? 1.5f : 1;
        return GetLinearOutput(separationForce, separationLinearSteering) * separationLinearSteering.weight * targetWeightModifier;
    }

    private float GetAlignmentOutput(bool checkAlignAndCohesion, float charOr, float charRotSpeed, float targetOr, AngularSteering alignmentAngularSteering)
    {
        if (!checkAlignAndCohesion)
            return 0;

        return GetAngularOutput(charOr, charRotSpeed, targetOr, alignmentAngularSteering) * alignmentAngularSteering.weight;
    }

    private float2 GetCohesionOutput(bool checkAlignAndCohesion, float2 direction, LinearSteering cohesionSteering)
    {
        if (!checkAlignAndCohesion)
            return new float2();

        return GetLinearOutput(direction, cohesionSteering) * cohesionSteering.weight;
    }

    private void GetWanderOut(float characterOrientaion, float characterRotation, bool targetFound, WanderParameters wanderParameters, LinearSteering wanderLinearSteering, AngularSteering wanderAngularSteering, out float2 linearOutput, out float angularOutput)
    {
        angularOutput = 0;
        linearOutput = new float2();
        if (targetFound)
            return;
        
        float wanderOrientation = RandomBinomial() * wanderParameters.rate;
        float targetOrientation = wanderOrientation + characterOrientaion;
        
        float2 characterOrientationAsVector = MathUtility.AngleRotationAsFloat2(characterOrientaion);
        
        angularOutput = GetAngularOutput(characterOrientaion, characterRotation, targetOrientation, wanderAngularSteering) * wanderAngularSteering.weight;
        linearOutput = wanderLinearSteering.maxAcceleration * characterOrientationAsVector * wanderLinearSteering.weight;
    }

    private float RandomBinomial()
    {
        var result = random.NextFloat() - random.NextFloat();
        return result;
    }

    private float GetSeekAngularOutput(float orientationAsRad, float boidRotationSpeed, float directionAsOrientation, AngularSteering targetAngularSteering, bool targetFound)
    {
        if (!targetFound)
            return 0;

        return GetAngularOutput(orientationAsRad, boidRotationSpeed, directionAsOrientation, targetAngularSteering) * targetAngularSteering.weight;
    }

    private float2 GetSeekLinearOutput(float2 position, float targetDistanceSquared, NativeArray<float2> targets, LinearSteering targetLinearSteering, out float directionAsOrientation, out bool targetFound)
    {
        float weight = targetLinearSteering.weight;
        if (weight <= 0)
        {
            targetFound = false;
            directionAsOrientation = 0;
            return new float2();
        }
        
        var directionToTarget = FindDirectionToClosestTarget(position, targetDistanceSquared, targets, out targetFound);
        directionAsOrientation = MathUtility.DirectionToFloat(directionToTarget);
        return GetLinearOutput(directionToTarget, targetLinearSteering) * targetLinearSteering.weight;
    }

    private float GetAngularOutput(float characterOrientation, float characterRotationSpeed, float targetOrientation, AngularSteering steering)
    {
        // rotational difference to target
        float rotationAngle = targetOrientation - characterOrientation;
            
        // rotation mapped to [-PI, PI]
        rotationAngle = MathUtility.MapToRangeMinusPiToPi(rotationAngle);
        
        // absoulte value of rotation
        var rotationAbsValue = math.abs(rotationAngle);

        // return no steering if rotation is close enough to target
        if (rotationAbsValue < steering.targetRadius)
        {
            return 0;
        }

        // use max rotation if outside slow radius. Otherwise, scale it with slow radius
        float targetRotation = rotationAbsValue > steering.slowRadius
            ? steering.maxRotation
            : steering.maxRotation * rotationAbsValue / steering.slowRadius;

        targetRotation *= rotationAngle / rotationAbsValue;

        float output = (targetRotation - characterRotationSpeed) / steering.timeToTarget;

        var angularAcceleration = math.abs(output);
        if (angularAcceleration > steering.maxAngularAcceleration)
        {
            output /= angularAcceleration;
            output *= steering.maxAngularAcceleration;
        }
            
        return output;
    }

    private float2 GetLinearOutput(float2 direction, LinearSteering steering)
    {
        return direction * steering.maxAcceleration;
    }

    private float2 FindDirectionToClosestTarget( float2 oldPos, float maxTargetDistanceSquared, NativeArray<float2> targetPositions, out bool foundTarget)
     {
         foundTarget = GetDirectionToClosest(oldPos, targetPositions, maxTargetDistanceSquared, out float2 direction);
         if (foundTarget)
         {
             return math.normalizesafe(direction);
         }
         else
         {
             return float2.zero;
         }
     }

     private static bool GetDirectionToClosest(float2 oldPos, NativeArray<float2> positions, float maxDistanceSquared, out float2 direction)
     {
         bool foundClosest = false;
         
         direction = new float2();
         float closestDis = maxDistanceSquared;

         foreach (var position in positions)
         {
             float squareDis = math.distancesq(oldPos, position);

             if (squareDis < closestDis)
             {
                 closestDis = squareDis;
                 direction = position - oldPos;
                 foundClosest = true;
             }
         }

         return foundClosest;
     }
}
}
