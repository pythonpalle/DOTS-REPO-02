using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerConfigAuthoring.Config>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<PlayerConfigAuthoring.Config>();

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var input = new float3(horizontal, 0, vertical) * SystemAPI.Time.DeltaTime * config.speed;

        if (input.Equals(float3.zero))
            return;

        // TODO: Add collision checks before movement
        foreach (var playerTransform in SystemAPI.Query<RefRW<LocalTransform>>())
        {
            var newPos = playerTransform.ValueRO.Position + input;
            playerTransform.ValueRW.Position = newPos;
        }
    }
}
