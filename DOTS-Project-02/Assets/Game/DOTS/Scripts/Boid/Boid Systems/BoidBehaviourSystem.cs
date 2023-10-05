using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(BoidSpawnSystem))]
[UpdateAfter(typeof(PlayerSpawnerSystem))]
public partial struct BoidBehaviourSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerConfig>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityQuery boidQuery = SystemAPI.QueryBuilder().WithAll<Boid>().WithAllRW<LocalToWorld>().Build();
        
        // empty native array with float4x4
        NativeArray<float4x4> newBoidLTWs = new NativeArray<float4x4>(boidQuery.CalculateEntityCount(), Allocator.Temp);

        var deltaTime = Time.deltaTime;

        int boidIndex = 0;
        // set new LTWs in native array
        foreach (var  localToWorld in SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<Boid>())
        {
            var oldPos = localToWorld.ValueRO.Position;


            var directionNorm = new float3();
            foreach (var target in SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<BoidTarget>())
            {
                var targetPos = target.ValueRO.Position;
                var directionToTarget = targetPos - oldPos;
                directionNorm = math.normalize(directionToTarget);
            }

            var speed = 2f;
            var newPos = localToWorld.ValueRO.Position + directionNorm * deltaTime * speed;
            
            newBoidLTWs[boidIndex] = float4x4.TRS(
                newPos,
                quaternion.LookRotation(directionNorm, math.up()),
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
}