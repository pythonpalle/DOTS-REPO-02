using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
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
        // var boidQuery = SystemAPI.QueryBuilder().WithAll<Boid>().WithAllRW<LocalToWorld>().Build();
        // var targetQuery = SystemAPI.QueryBuilder().WithAll<BoidTarget, LocalToWorld>().Build();
        // var obstacleQuery = SystemAPI.QueryBuilder().WithAll<Obstacle, LocalToWorld>().Build();

        var deltaTime = Time.deltaTime;
        var moveSpeed = 2f;

        //
        // foreach (var boidTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<Boid>())
        // {
        //     var boidPos = boidTransform.ValueRO.Position;
        //     
        //     foreach (var targetTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<BoidTarget>())
        //     {
        //         var direction = (targetTransform.ValueRO.Position - boidPos);
        //         var directionNorm = math.normalize(direction);
        //
        //         boidTransform.ValueRW.Position = boidPos + directionNorm * deltaTime * moveSpeed;
        //     }
        // }
    }
}

[BurstCompile]
partial struct SteerBoidJob : IJobEntity
{
    
}
