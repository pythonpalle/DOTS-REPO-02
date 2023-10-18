using Common;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DOTS
{
    [UpdateAfter(typeof(BoidSpawnSystem))]
    [UpdateAfter(typeof(PlayerSpawnerSystem))]
    public partial struct BoidMoveSystemWithJobs : ISystem
    {
        Random random;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoidConfig>();
            state.RequireForUpdate<RunBoidsWithJobs>();
            random = new Random(123456);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var boidConfig = SystemAPI.GetSingleton<BoidConfig>();
            var deltaTime = SystemAPI.Time.DeltaTime;
            
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
                WithAll<Boid, LocalTransform>().
                WithAllRW<VelocityComponent, RotationSpeedComponent>().
                Build();

            EntityQuery targetQuery = SystemAPI.QueryBuilder().WithAll<BoidTarget, LocalTransform>().Build();
            EntityQuery obstacleQuery = SystemAPI.QueryBuilder().WithAll<Obstacle, LocalTransform>().Build();
            
            // number of different objects
            int boidsCount = boidQuery.CalculateEntityCount();
            int targetCount = targetQuery.CalculateEntityCount();
            int obstacleCount = obstacleQuery.CalculateEntityCount();

            #endregion
            #region Native Array Region

            // boid components
            NativeArray<LocalTransform> boidTransforms = boidQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
            NativeArray<RotationSpeedComponent> boidRotationSpeeds = boidQuery.ToComponentDataArray<RotationSpeedComponent>(state.WorldUpdateAllocator);
            NativeArray<VelocityComponent> boidVelocities = boidQuery.ToComponentDataArray<VelocityComponent>(state.WorldUpdateAllocator);

            // to store initial boid data
            NativeArray<float2> initialBoidPositions = new NativeArray<float2>(boidsCount, Allocator.TempJob);
            NativeArray<float> initialBoidOrientations = new NativeArray<float>(boidsCount, Allocator.TempJob);
            NativeArray<float2> initialBoidOrientationVectors = new NativeArray<float2>(boidsCount, Allocator.TempJob);
            
            // to store neighbour data
            NativeArray<float2> averageNeighbourPositions = new NativeArray<float2>(boidsCount, Allocator.TempJob);
            NativeArray<float> averageNeighbourOrientations = new NativeArray<float>(boidsCount, Allocator.TempJob);
            
            // to store separation forces from other boids
            NativeArray<float2> separationForces = new NativeArray<float2>(boidsCount, Allocator.TempJob);
            
            // target data
            NativeArray<LocalTransform> targetTransforms = targetQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
            NativeArray<float2> targetPositions = new NativeArray<float2>(targetCount, Allocator.TempJob);
            
            // obstacle data
            NativeArray<LocalTransform> obstacleTransforms = obstacleQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
            NativeArray<float2> obstaclePositions = new NativeArray<float2>(obstacleCount, Allocator.TempJob);
            
            // entity indices
            NativeArray<int> boidChunkBaseEntityIndexArray = boidQuery.CalculateBaseEntityIndexArrayAsync(
                Allocator.TempJob, state.Dependency,
                out JobHandle boidChunkBaseIndexJobHandle);
            
            #endregion

            #region Job Declarations

            // initialize boids data job
            var initializeBoidsJob = new BoidInitializeJob()
            {
                boidTransforms = boidTransforms,
                
                initialBoidPositions = initialBoidPositions,
                initialBoidOrientations = initialBoidOrientations,
                initialBoidOrientationVectors = initialBoidOrientationVectors,
            };
            JobHandle initializeBoidHandle = initializeBoidsJob.Schedule(boidsCount, 64);
            
            // get target positions job (really pointless in this case since their is only one target)
            var targetJob = new GetPositionsFromTransformsJob()
            {
                transforms = targetTransforms,
                positions = targetPositions,
            };
            JobHandle targetHandle = targetJob.Schedule(targetCount, 1);
            
            // get obstacle positions job
            var obstacleJob = new GetPositionsFromTransformsJob()
            {
                transforms = obstacleTransforms,
                positions = obstaclePositions,
            };
            JobHandle obstacleHandle = obstacleJob.Schedule(obstacleCount, 2);
            
            // get neighbour data job
            var neighbourJob = new SetNeighbourDataJob()
            {
                initialBoidPositions = initialBoidPositions,
                initialBoidOrientations = initialBoidOrientations,
                initialBoidOrientationVectors = initialBoidOrientationVectors,

                separationForces = separationForces,
                averageNeighbourPositions = averageNeighbourPositions,
                averageNeighbourOrientations = averageNeighbourOrientations,

                boidsCount = boidsCount,
                maxNeighbourDistanceSquared = maxNeighbourDistanceSquared,
                halfFOVInRadians = halfFOVInRadians,
                minSeparationDistanceSquared = minSeparationDistanceSquared,
                separationDecayCoefficient = separationDecayCoefficient,
                separationLinearSteering = separationLinearSteering
            };
            JobHandle neighbourHandle = neighbourJob.Schedule(boidsCount, 32, initializeBoidHandle);

            targetHandle.Complete();
            obstacleHandle.Complete();
            neighbourHandle.Complete();

            // move boids job
            var boidMoveJob = new BoidMoveJob
            {
                ChunkBaseEntityIndices = boidChunkBaseEntityIndexArray,

                initialBoidPositions = initialBoidPositions,
                initialBoidOrientations = initialBoidOrientations,

                averageNeighbourPositions = averageNeighbourPositions,
                averageNeighbourOrientations = averageNeighbourOrientations,
                separationForces = separationForces,

                targetPositions = targetPositions,
                obstaclePositions = obstaclePositions,

                deltaTime = deltaTime,
                targetVisionDistanceSquared = targetVisionDistanceSquared,
                obstacleAvoidanceDistanceSquared = obstacleAvoidanceDistanceSquared,
                moveSpeed = moveSpeed,
                chaseSpeedModifier = chaseSpeedModifier,

                targetLinearSteering = targetLinearSteering,
                targetAngularSteering = targetAngularSteering,

                wanderLinearSteering = wanderLinearSteering,
                wanderAngularSteering = wanderAngularSteering,
                wanderParameters = wanderParameters,

                cohesionLinearSteering = cohesionLinearSteering,
                separationLinearSteering = separationLinearSteering,
                obstacleLinearSteering = obstacleLinearSteering,

                alignmentAngularSteering = alignmentAngularSteering,

                random = random
            };
            var boidMoveHandle = boidMoveJob.ScheduleParallel(boidQuery, boidChunkBaseIndexJobHandle);
            boidMoveHandle.Complete();

            #endregion
            
            #region Dispose Region

            // Dispose native arrays
            boidRotationSpeeds.Dispose();
            boidVelocities.Dispose();
            boidTransforms.Dispose();
            
            initialBoidPositions.Dispose();
            initialBoidOrientations.Dispose();
            initialBoidOrientationVectors.Dispose();
            
            averageNeighbourPositions.Dispose();
            averageNeighbourOrientations.Dispose();
            
            separationForces.Dispose();
            
            targetPositions.Dispose();
            obstaclePositions.Dispose();

            boidChunkBaseEntityIndexArray.Dispose();

            #endregion
        }
    }
    
    // ----------------------------------------------------------- JOBS --------------------------------------------------------

    #region Job Structs

    [BurstCompile]
    [WithAll(typeof(Boid))]
    [WithAll(typeof(LocalTransform))]
    [WithAll(typeof(RotationSpeedComponent))]
    [WithAll(typeof(VelocityComponent))]
    public partial struct BoidInitializeJob : IJobParallelFor
    {
        [Unity.Collections.ReadOnly] public NativeArray<LocalTransform> boidTransforms;
        
        [WriteOnly] public NativeArray<float2> initialBoidPositions;
        [WriteOnly] public NativeArray<float> initialBoidOrientations;
        [WriteOnly] public NativeArray<float2> initialBoidOrientationVectors;

        public void Execute(int i)
        {
            // set all initial boid data
            var transform = boidTransforms[i];
            
            float3 forward = transform.Forward();
            float2 orientationVector = new float2(forward.x, forward.z);
            float orientation = MathUtility.DirectionToFloat(orientationVector);
            orientation = MathUtility.MapToRange0To2Pie(orientation);
            
            initialBoidPositions[i] = transform.Position.xz;
            initialBoidOrientations[i] = orientation;
            initialBoidOrientationVectors[i] = orientationVector;
        }
    }
    
    [BurstCompile]
    [WithAll(typeof(LocalTransform))]
    public partial struct GetPositionsFromTransformsJob : IJobParallelFor
    {
        [Unity.Collections.ReadOnly] public NativeArray<LocalTransform> transforms;
        [WriteOnly] public NativeArray<float2> positions;

        public void Execute(int i)
        {
            positions[i] = transforms[i].Position.xz;
        }
    }
    
    [BurstCompile]
    public partial struct SetNeighbourDataJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> initialBoidPositions;
        [ReadOnly] public NativeArray<float> initialBoidOrientations;
        [ReadOnly] public NativeArray<float2> initialBoidOrientationVectors;
        
        public NativeArray<float2> separationForces;
        [WriteOnly] public NativeArray<float2> averageNeighbourPositions;
        [WriteOnly] public NativeArray<float> averageNeighbourOrientations;
        
        public int boidsCount;
        public float maxNeighbourDistanceSquared;
        public float halfFOVInRadians;
        public float minSeparationDistanceSquared;
        public float separationDecayCoefficient;
        public LinearSteering separationLinearSteering;
        
        public void Execute(int index)
        {
            float2 averagePos = new float2();
            float2 averageOrientationVector = float2.zero;
            
            float2 boidPos = initialBoidPositions[index];
            float boidOrientation = initialBoidOrientations[index];
            int neighbourCount = 0;
            
            // loop over all other boids to find potential neighbours
            for (int otherIndex = 0; otherIndex < boidsCount; otherIndex++)
            {
                // skip if same index
                if (index == otherIndex) continue;

                float2 otherPos = initialBoidPositions[otherIndex];
                
                // ignore if outside of view range
                float squareDistance = math.distancesq(boidPos, otherPos);
                if (squareDistance > maxNeighbourDistanceSquared) continue;

                // find direction to other boid
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
                float averageOrientation = MathUtility.DirectionToFloat(averageOrientationVector);
                averageNeighbourOrientations[index] = averageOrientation;
                
                averagePos /= neighbourCount;
                averageNeighbourPositions[index] = averagePos;
            }
        }
    }

    [BurstCompile]
    [WithAll(typeof(Boid))]
    [WithAll(typeof(LocalTransform))]
    [WithAll(typeof(RotationSpeedComponent))]
    [WithAll(typeof(VelocityComponent))]
    public partial struct BoidMoveJob : IJobEntity {
        
        [ReadOnly] public NativeArray<int> ChunkBaseEntityIndices;
        
        [ReadOnly] public NativeArray<float2> initialBoidPositions;
        [ReadOnly] public NativeArray<float> initialBoidOrientations;
        
        [ReadOnly] public NativeArray<float2> averageNeighbourPositions;
        [ReadOnly] public NativeArray<float> averageNeighbourOrientations;
        [ReadOnly] public NativeArray<float2> separationForces;
        
        [ReadOnly] public NativeArray<float2> targetPositions;
        [ReadOnly] public NativeArray<float2> obstaclePositions;

        public float deltaTime;
        public float targetVisionDistanceSquared;
        public float obstacleAvoidanceDistanceSquared;
        public float moveSpeed;
        public float chaseSpeedModifier;

        public LinearSteering targetLinearSteering;
        public AngularSteering targetAngularSteering;
        
        public LinearSteering wanderLinearSteering;
        public AngularSteering wanderAngularSteering;
        public WanderParameters wanderParameters;
        
        public LinearSteering cohesionLinearSteering;
        public LinearSteering separationLinearSteering;
        public LinearSteering obstacleLinearSteering;
        
        public AngularSteering alignmentAngularSteering;

        public Random random;

        void Execute([ChunkIndexInQuery] int chunkIndexInQuery, [EntityIndexInChunk] int entityIndexInChunk, 
            ref LocalTransform transform, ref VelocityComponent velocity, ref RotationSpeedComponent rotationSpeed)
        {
            int index = ChunkBaseEntityIndices[chunkIndexInQuery] + entityIndexInChunk;
            //Debug.Log($"chunkIndexInQuery: {ChunkBaseEntityIndices[chunkIndexInQuery]} Entity Index Chunk: {entityIndexInChunk} index: {index}");
            
            // total stear outputs
            float2 totalLinearOutput = float2.zero;
            float totalAngularOutput = 0f;
        
            // fetch the boid data
            float boidOrientatation = initialBoidOrientations[index];
            float boidRotationSpeed = rotationSpeed.Value;
            float2 position = initialBoidPositions[index];
        
            // prioritize system:
            // 1. if sees target, chase it
            totalLinearOutput += GetSeekLinearOutput(position, targetVisionDistanceSquared, targetPositions, targetLinearSteering, out float directionAsOrientation, out bool targetFound);
            totalAngularOutput += GetSeekAngularOutput(boidOrientatation, boidRotationSpeed, directionAsOrientation, targetAngularSteering, targetFound);
            
            // 2. else, wander around
            GetWanderOut(boidOrientatation, boidRotationSpeed, targetFound, wanderParameters, wanderLinearSteering, wanderAngularSteering, out float2 linearWander, out float angularWander);
            totalLinearOutput += linearWander;
            totalAngularOutput += angularWander;
        
            // 3. if has neighbours (and doesn't see the player), use alignment and cohesion
            bool checkAlignAndCohesion = !targetFound && averageNeighbourOrientations[index] != 0;
            float2 directionToAvergae = averageNeighbourPositions[index] - position;
            float averageOrientation = averageNeighbourOrientations[index];
            totalLinearOutput += GetCohesionOutput(checkAlignAndCohesion, directionToAvergae, cohesionLinearSteering);
            totalAngularOutput += GetAlignmentOutput(checkAlignAndCohesion, boidOrientatation, boidRotationSpeed, averageOrientation, alignmentAngularSteering);
            
            // 4. always check for separation and obstacles
            // TODO: Can be their own since no dependencies? (if remove target found multiplier)
            totalLinearOutput += GetSeparatationSteering(separationForces[index], separationLinearSteering, targetFound);
            totalLinearOutput += GetObstacleSteering(position, obstacleAvoidanceDistanceSquared, obstaclePositions, obstacleLinearSteering);
            
            // update position based on current velocity
            transform.Position += MathUtility.Float2ToFloat3(velocity.Value) * deltaTime;
        
            // update rotation based on current rotationSpeed
            quaternion rotationDelta = quaternion.RotateY(-boidRotationSpeed * deltaTime);
            transform.Rotation = math.mul(transform.Rotation, rotationDelta);
            
            // update the current velocity based on linear steer output
            velocity.Value += totalLinearOutput * moveSpeed * deltaTime;
            float2 velocityRO = velocity.Value;
        
            float maxMoveSpeed = moveSpeed * (targetFound ? chaseSpeedModifier : 1);
            if (math.length(velocityRO) > maxMoveSpeed)
                velocity.Value = math.normalize(velocityRO) * maxMoveSpeed;
               
            // update the current rotationSpeed based on angular output
            rotationSpeed.Value += totalAngularOutput;
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
            
            // absolute value of rotation
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

    #endregion
}
