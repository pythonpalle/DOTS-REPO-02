using System.Collections;
using System.Collections.Generic;
using Common;
using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Analytics;

namespace DOTS
{
    
[UpdateAfter(typeof(BoidSpawnSystem))]
[UpdateAfter(typeof(PlayerSpawnerSystem))]
public partial struct BoidBehaviourSystem : ISystem
{
    private static readonly float3 EPSILON_FLOAT3 = new float3(0.0001f, 0f, 0.0001f);
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoidConfig>();
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
        float moveSpeed;
        float chaseSpeedModifier;
            
        // neighbour data
        float maxNeighbourDistanceSquared = boidConfig.neighbourDistanceSquared;
        float halfFOVInRadians = boidConfig.halfFovInRadians;
            
        // target data
        float targetVisionDistanceSquared = boidConfig.targetVisionDistanceSquared;
        float targetWeight = boidConfig.targetWeight;

        // alignment data
        float alignmentWeight = boidConfig.alignmentWeight;
                
        // cohesion data
        float cohesionWeight = boidConfig.cohesionWeight;
                
        // separation data
        float separationWeight = boidConfig.separationWeight;
        float minSeparationDistanceSquared = boidConfig.separationDistanceSquared;
        float maxSeparationAcceleration = boidConfig.separationMaxAcceleration;
        float separationDecayCoefficient = boidConfig.separationDecayCoefficient;
                
        // obstacle data
        float obstacleAvoidanceDistanceSquared = boidConfig.obstacleAvoidanceDistanceSquared;
        float avoidanceWeight = boidConfig.obstacleAvoidanceDistanceSquared;
        
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

        // TODO: Move to OnCreate and store as public variable?
        NativeArray<LocalTransform> boidLocalTransforms = new NativeArray<LocalTransform>(boidsCount, Allocator.TempJob);
        
        // to store boid data
        NativeArray<float2> initialBoidPositions = new NativeArray<float2>(boidsCount, Allocator.TempJob);
        NativeArray<float2> initialBoidVelocities = new NativeArray<float2>(boidsCount, Allocator.TempJob);
        NativeArray<float> initialBoidOrientations = new NativeArray<float>(boidsCount, Allocator.TempJob);
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
        foreach (var (velocity, rotation, localToWorld) in SystemAPI.Query<VelocityComponent, RotationSpeedComponent, 
            RefRW<LocalTransform>>().WithAll<Boid>())
        {
            initialBoidPositions[index] = localToWorld.ValueRO.Position.xz;
            initialBoidOrientations[index] = MathUtility.DirectionToFloat(localToWorld.ValueRO.Forward());

            initialBoidVelocities[index] =  velocity.Value;
            initialBoidRotationSpeeds[index] =  rotation.Value;
            boidLocalTransforms[index] = localToWorld.ValueRW;
            
            index++;
        }

        // set all neighbour data
        // TODO: convert to job?
        for (index = 0; index < boidsCount; index++)
        {
            float2 averagePos = new float2();
            float averageOrientation = 0;

            float2 boidPos = initialBoidPositions[index];
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
                
                // ignore if outside FOV
                if (math.abs(rotationToOther) > halfFOVInRadians) continue;

                // finally, add position and orientation to average
                averagePos += otherPos;
                averageOrientation += initialBoidOrientations[otherIndex];
                
                // update separation forces
                // TODO: ignore if separation weight is 0 (?)
                if (squareDistance < minSeparationDistanceSquared)
                {
                    float strength = math.min(separationDecayCoefficient / (squareDistance), maxSeparationAcceleration);
                    separationForces[index] -= strength * directionNormalized;
                }
                
                neighbourCount++;
            }
            
            // set average positions and orientations
            if (neighbourCount > 0)
            {
                averageOrientation /= neighbourCount;
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
        foreach (var localToWorld in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<Boid>())
        {
            // prioritize system:
            // 1. if sees target, chase it
            var directionToTarget = FindDirectionToClosestTarget(initialBoidPositions[index], targetVisionDistanceSquared, targetPositions);

            newVelocities[index] = directionToTarget;
            
            var boidPos = initialBoidPositions[index];
            var newBoidPos = boidPos + directionToTarget * deltaTime;

            float3 newPosAsFloat3 = new float3(newBoidPos.x, 1, newBoidPos.y);
            localToWorld.ValueRW.Position = newPosAsFloat3;
            index++;
        }
        

        #region Dispose Region

        // Dispose native arrays
        newBoidLTWMatrices.Dispose();
        boidLocalTransforms.Dispose();
        
        initialBoidPositions.Dispose();
        initialBoidVelocities.Dispose();
        initialBoidOrientations.Dispose();
        initialBoidRotationSpeeds.Dispose();
        
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

    private void GetInitialBoidData(NativeArray<float2> initialBoidPositions, NativeArray<float> initialBoidOrientations, NativeArray<float2> initialBoidVelocities, NativeArray<float> initialBoidRotations, NativeArray<LocalToWorld> boidLocalToWorlds, ref SystemState state)
    {
        int index = 0;
        // TODO: Change to Read Only for the query components
        foreach (var (velocity, rotation, localToWorld) in SystemAPI.Query<VelocityComponent, RotationSpeedComponent, LocalToWorld>().WithAll<Boid>())
        {
            initialBoidPositions[index] = localToWorld.Position.xz;
            initialBoidOrientations[index] = MathUtility.DirectionToFloat(localToWorld.Forward);

            initialBoidVelocities[index] =  velocity.Value;
            initialBoidRotations[index] =  rotation.Value;
            boidLocalToWorlds[index] = localToWorld;
            index++;
        }
    }

    private void GetInitialBoidData(NativeArray<float2> initialBoidPositions, 
        NativeArray<LocalToWorld> localToWorlds, ref SystemState state)
    {
        int index = 0;
        foreach (var localToWorld in SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<Boid>())
        {
            initialBoidPositions[index] =  localToWorld.ValueRO.Position.xz;
            localToWorlds[index] = localToWorld.ValueRO;
            index++;
        }
    }

    private  void GetTargetPositions(NativeArray<float2> targetPositions, ref SystemState _)
    {
        int index = 0;
        foreach (var localToWorld in SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<BoidTarget>())
        {
            targetPositions[index] = localToWorld.ValueRO.Position.xz;
            index++;
        }
    }
    
    private void GetObstaclePositions(NativeArray<float2> obstaclePositions, ref SystemState state)
    {
        int index = 0;
        foreach (var localToWorld in SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<Obstacle>())
        {
            obstaclePositions[index] = localToWorld.ValueRO.Position.xz;
            index++;
        }
    }

    [BurstCompile]
     private struct SetTargetForcesJob : IJobParallelFor {
         
         [WriteOnly] public NativeArray<float3> targetForces;
         [ReadOnly] public NativeArray<float3> boidPositions;
         [ReadOnly] public NativeArray<float3> targetPositions;
         [ReadOnly] public float maxTargetDistance;

         public void Execute(int index)
         {
             var boidPos = boidPositions[index];
             
             var direction = new float3();

             float closestDis = float.MaxValue;
        
             foreach (var targetPos in targetPositions)
             {
                 float squareDis = math.distancesq(boidPos, targetPos);

                 if (squareDis < closestDis)
                 {
                     closestDis = squareDis;
                     direction = targetPos - boidPos;
                 }
             }

             if (closestDis < maxTargetDistance)
             {
                 targetForces[index]= math.normalizesafe(direction);
             }
             else
             {
                 targetForces[index]=EPSILON_FLOAT3;
             }

         }
     }


     [BurstCompile]
     private struct SetNewLocalToWorldsJob : IJobParallelFor
     {
         [WriteOnly] public NativeArray<float4x4> boidLocalToWorlds;
         [ReadOnly] public NativeArray<float3> boidPositions;

         [ReadOnly] public NativeArray<float3> targetForces;
         [ReadOnly] public float moveDistance;

         public void Execute(int index)
         {
             var boidPos = boidPositions[index];
             
             // TODO: Add forces from other boids and obstacles
             var forcesSum = targetForces[index];

             var direction = math.normalize(forcesSum) * moveDistance;

             var newPos = boidPos + direction;
             var lookRotation = quaternion.LookRotation(direction, math.up());
             
             boidLocalToWorlds[index] = float4x4.TRS(
                 newPos,
                 lookRotation,
                 new float3(1f)
             );

         }
     }
     
     [BurstCompile]
     private partial struct UpdateBoidLocalToWorldsJob : IJobEntity 
     {
         [ReadOnly] public NativeArray<float4x4> newLocalToWorlds;
         
         public void Execute([EntityIndexInQuery] int entityIndexInQuery, ref LocalToWorld localToWorld)
         {
             localToWorld.Value = newLocalToWorlds[entityIndexInQuery];
         }
     }
     
     private void SetNewBoidLocalToWorlds(BoidConfig boidConfig, NativeArray<float2> targetPositions, NativeArray<float2> obstaclePositions, float deltaTime,
         NativeArray<float4x4> newBoidLTWs, ref SystemState state)
     {
         int boidIndex = 0;
         foreach (var localToWorld in SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<Boid>())
         {
             float2 boidPos = localToWorld.ValueRO.Position.xz;
             
             float2 directionToTarget = FindDirectionToClosestTarget(boidPos, boidConfig.targetVisionDistanceSquared, targetPositions) * boidConfig.targetWeight;
             float2 obstacleAvoidance = GetObstacleFactor(boidPos, boidConfig.obstacleAvoidanceDistanceSquared, obstaclePositions) * boidConfig.avoidanceWeight;

             float2 combined = directionToTarget + obstacleAvoidance;
             float2 combinedAdjusted = math.distancesq(combined, float2.zero) < 0.001f 
                 ? new float2(0.001f, 0.001f) 
                 : math.normalize(combined);
             
             var speed = boidConfig.moveSpeed;
             float2 newPos = boidPos + combinedAdjusted * deltaTime * speed;

             var rotation = math.distancesq(combined, float2.zero) < 0.001f
                 ? localToWorld.ValueRO.Rotation
                 : quaternion.LookRotation(new float3(combinedAdjusted.x, 0, combinedAdjusted.y), math.up());
             
             newBoidLTWs[boidIndex] = float4x4.TRS(
                 new float3(newPos.x, 0, newPos.y),
                 rotation,
                 new float3(1f)
             );

             boidIndex++;
         }
     }
     
     private float2 FindDirectionToClosestTarget( float2 oldPos, float maxTargetDistance, NativeArray<float2> targetPositions)
     {
         if (GetDirectionToClosest(oldPos, targetPositions, maxTargetDistance, out float2 direction))
         {
             return math.normalizesafe(direction);
         }
         else
         {
             return float2.zero;
         }

     }

     private static bool GetDirectionToClosest(float2 oldPos, NativeArray<float2> positions, float maxDistance, out float2 direction)
     {
         bool foundClosest = false;
         
         direction = new float2();
         float closestDis = maxDistance;

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

     private void UpdateBoidLTWs(NativeArray<float4x4> newBoidLTWs, ref SystemState state)
     {
         int boidIndex = 0;
         foreach (var localToWorld in SystemAPI.Query<RefRW<LocalToWorld>>().WithAll<Boid>())
         {
             localToWorld.ValueRW.Value = newBoidLTWs[boidIndex];
             boidIndex++;
         }
     }
     
     

     private float2 GetObstacleFactor(float2 boidPos, float avoidDistance, NativeArray<float2> obstaclePositions)
     {
         if (GetDirectionToClosest(boidPos, obstaclePositions, avoidDistance, out float2 direction))
         {
             return -math.normalizesafe(direction);
         }
         else
         {
             return float2.zero;
         }
     }
}


}
