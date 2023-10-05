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
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoidConfig>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityQuery boidQuery = SystemAPI.QueryBuilder().WithAll<Boid>().WithAllRW<LocalToWorld>().Build();
        EntityQuery targetQuery = SystemAPI.QueryBuilder().WithAll<BoidTarget>().WithAllRW<LocalToWorld>().Build();
        
        // empty native array with float4x4 for new boid positions, which are later assigned to the boids
        NativeArray<float4x4> newBoidLTWs = new NativeArray<float4x4>(boidQuery.CalculateEntityCount(), Allocator.Temp);
        NativeArray<float3> boidPositions = new NativeArray<float3>(targetQuery.CalculateEntityCount(), Allocator.Temp);
        
        
        NativeArray<float3> targetPositions = new NativeArray<float3>(targetQuery.CalculateEntityCount(), Allocator.Temp);
        NativeArray<float3> targetForces = new NativeArray<float3>(boidQuery.CalculateEntityCount(), Allocator.Temp);

        var deltaTime = Time.deltaTime;
        var boidConfig = SystemAPI.GetSingleton<BoidConfig>();


        targetPositions = SetTargetPositions(targetPositions, ref state);

        // ToTargetForcesJob toTargetForcesJob = new ToTargetForcesJob
        // {
        //     targetForces = targetForces,
        //     boidPositions = boidPositions,
        //     targetPositions = targetPositions,
        //     maxTargetDistance = boidConfig.targetVisionDistanceSquared,
        // };
        
        newBoidLTWs = SetNewBoidLocalToWorlds(boidConfig, targetPositions, deltaTime, newBoidLTWs, ref state);

        UpdateBoidPositions(newBoidLTWs, ref state);
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

    private NativeArray<float4x4> SetNewBoidLocalToWorlds(BoidConfig boidConfig, NativeArray<float3> targetPositions, float deltaTime,
        NativeArray<float4x4> newBoidLTWs, ref SystemState state)
    {
        int boidIndex = 0;
        // set new LTWs in native array
        foreach (var localToWorld in SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<Boid>())
        {
            var boidPos = localToWorld.ValueRO.Position;

            var directionToTarget =
                FindDirectionToClosestTarget(boidPos, boidConfig.targetVisionDistanceSquared, targetPositions);

            var speed = boidConfig.moveSpeed;
            var newPos = boidPos + directionToTarget * deltaTime * speed;

            newBoidLTWs[boidIndex] = float4x4.TRS(
                newPos,
                quaternion.LookRotation(directionToTarget, math.up()),
                new float3(1f)
            );

            boidIndex++;
        }

        return newBoidLTWs;
    }

    private  NativeArray<float3> SetTargetPositions(NativeArray<float3> targetPositions, ref SystemState _)
    {
        // set all target positions
        int targetIndex = 0;
        foreach (var localToWorld in SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<BoidTarget>())
        {
            targetPositions[targetIndex] = localToWorld.ValueRO.Position;
            targetIndex++;
        }

        return targetPositions;
    }

    private float3 FindDirectionToClosestTarget( float3 oldPos, float maxTargetDistance, NativeArray<float3> targetPositions)
    {
        var direction = new float3();

        float closestDis = float.MaxValue;
        
        foreach (var targetPos in targetPositions)
        {
            float squareDis = math.distancesq(oldPos, targetPos);

            if (squareDis < closestDis)
            {
                closestDis = squareDis;
                direction = targetPos - oldPos;
            }
        }

        if (closestDis < maxTargetDistance)
        {
            return math.normalizesafe(direction);
        }
        else
        {
            return new float3(0.0001f, 0, 0.00001f);
        }

    }
    
    [BurstCompile]
     private struct ToTargetForcesJob : IJobParallelFor {
         
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
                 targetForces[index]=new float3(0.0001f, 0, 0.00001f);
             }

         }
     }
}

