using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Analytics;

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

        // queries
        EntityQuery boidQuery = SystemAPI.QueryBuilder().WithAll<Boid>().WithAllRW<LocalToWorld>().Build();
        EntityQuery targetQuery = SystemAPI.QueryBuilder().WithAll<BoidTarget>().WithAllRW<LocalToWorld>().Build();
        EntityQuery obstacleQuery = SystemAPI.QueryBuilder().WithAll<Obstacle>().WithAllRW<LocalToWorld>().Build();
        
        // number of different objects
        int boidsCount = boidQuery.CalculateEntityCount();
        int targetCount = targetQuery.CalculateEntityCount();
        int obstacleCount = obstacleQuery.CalculateEntityCount();
        
        // empty native array with float4x4 for new boid positions, which are later assigned to the boids
        NativeArray<float4x4> newBoidLTWs = new NativeArray<float4x4>(boidsCount, Allocator.Persistent);
        
        // TODO: Move to OnCreate and store as public variable?
        NativeArray<LocalToWorld> boidLocalToWorlds = new NativeArray<LocalToWorld>(boidsCount, Allocator.Persistent);
        
        // to store positions
        NativeArray<float2> initialBoidPositions = new NativeArray<float2>(boidsCount, Allocator.Persistent);
        NativeArray<float2> targetPositions = new NativeArray<float2>(targetCount, Allocator.Persistent);
        NativeArray<float2> obstaclePositions = new NativeArray<float2>(obstacleCount, Allocator.Persistent);
       
        // to store forces from different boid rules
        NativeArray<float2> fromOtherBoidsForces = new NativeArray<float2>(boidsCount, Allocator.Persistent);
        NativeArray<float2> targetForces = new NativeArray<float2>(boidsCount, Allocator.Persistent);
        NativeArray<float2> obstacleForces = new NativeArray<float2>(boidsCount, Allocator.Persistent);

        // store positions of boids, targets and obstacles.
        // TODO: Convert to jobs?
        GetInitialBoidPositions(initialBoidPositions, boidLocalToWorlds, ref state);
        GetTargetPositions(targetPositions, ref state);
        GetObstaclePositions(obstaclePositions, ref state);  // TODO: sätt i initialize      
        

        bool useJobs = boidConfig.useJobs;
        if (useJobs)
        {
            // // declare jobs
            // SetTargetForcesJob setTargetForcesJob = new SetTargetForcesJob
            // {
            //     boidPositions = initialBoidPositions,
            //     targetForces = targetForces,
            //     targetPositions = targetPositions,
            //     maxTargetDistance = boidConfig.targetVisionDistanceSquared,
            // };
            //
            // SetNewLocalToWorldsJob localToWorldsJob = new SetNewLocalToWorldsJob
            // {
            //     boidLocalToWorlds = newBoidLTWs,
            //     boidPositions = initialBoidPositions,
            //     moveDistance = deltaTime * boidConfig.moveSpeed,
            //     targetForces = targetForces
            // };
            //
            // UpdateBoidLocalToWorldsJob updateBoidLocalToWorldsJob = new UpdateBoidLocalToWorldsJob
            // {
            //     newLocalToWorlds = newBoidLTWs
            // };
            //
            // // create job handles
            // JobHandle toTargetForceJobHandle = setTargetForcesJob.Schedule(boidsCount, 64);
            // toTargetForceJobHandle.Complete();
            //
            // JobHandle setLocalToWorldsHandle = localToWorldsJob.Schedule(boidsCount, 64);
            // setLocalToWorldsHandle.Complete();
            //
            // JobHandle updateLocalToWorldsHandle = updateBoidLocalToWorldsJob.ScheduleParallel(boidQuery, setLocalToWorldsHandle);
            // updateLocalToWorldsHandle.Complete();
        }
        else
        {
            SetNewBoidLocalToWorlds(boidConfig, targetPositions, obstaclePositions, deltaTime, newBoidLTWs, ref state);
            UpdateBoidPositions(newBoidLTWs, ref state);
        }

        // Dispose persistant allocated native arrays
        newBoidLTWs.Dispose();
        boidLocalToWorlds.Dispose();
        
        initialBoidPositions.Dispose();
        targetPositions.Dispose();
        obstaclePositions.Dispose();

        fromOtherBoidsForces.Dispose();
        targetForces.Dispose();
        obstacleForces.Dispose();
    }

    private void GetInitialBoidPositions(NativeArray<float2> initialBoidPositions, 
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

     private void UpdateBoidPositions(NativeArray<float4x4> newBoidLTWs, ref SystemState state)
     {
         int boidIndex = 0;
         // update LTWs from native array
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

