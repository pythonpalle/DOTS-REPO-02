using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
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
        
        // empty native array with float4x4
        NativeArray<float4x4> newBoidLTWs = new NativeArray<float4x4>(boidQuery.CalculateEntityCount(), Allocator.Temp);

        var deltaTime = Time.deltaTime;

        var boidConfig = SystemAPI.GetSingleton<BoidConfig>();
        
        int boidIndex = 0;
        // set new LTWs in native array
        foreach (var  localToWorld in SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<Boid>())
        {
            var oldPos = localToWorld.ValueRO.Position;
            var directionToTarget = FindDirectionToClosestTarget(ref state, oldPos);
            
            var speed = boidConfig.moveSpeed;
            var newPos = oldPos + directionToTarget * deltaTime * speed;
        
            newBoidLTWs[boidIndex] = float4x4.TRS(
                newPos,
                quaternion.LookRotation(directionToTarget, math.up()),
                new float3(1f)
            );
            boidIndex++;
        }
    
        boidIndex = 0;
        // update LTWs from native array
        foreach (var  localToWorld in SystemAPI.Query<RefRW<LocalToWorld>>().WithAll<Boid>())
        {
            localToWorld.ValueRW.Value = newBoidLTWs[boidIndex];
            boidIndex++;
        }
    }

    private float3 FindDirectionToClosestTarget(ref SystemState state, float3 oldPos)
    {
        var direction = new float3();

        float closestDis = float.MaxValue;
        
        foreach (var target in SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<BoidTarget>())
        {
            var targetPos = target.ValueRO.Position;
            float squareDis = math.distancesq(oldPos, targetPos);

            if (squareDis < closestDis)
            {
                closestDis = squareDis;
                direction = targetPos - oldPos;
            }
        }

        return math.normalize(direction);
    }
}