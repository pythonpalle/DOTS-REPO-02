using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

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
        var added = new float3(horizontal, 0, vertical);

        if (added.Equals(float3.zero))
            return;

        var input = math.normalize(added) * SystemAPI.Time.DeltaTime * config.speed;
        var normInput = math.normalize(input);

        // TODO: Add collision checks before movement
        foreach (var playerTransform in
            SystemAPI.Query<RefRW<LocalTransform>>().WithAll<PlayerAuthoring.PlayerMovement>())
        {
            var playerPos = playerTransform.ValueRO.Position;
            var newPos = playerPos + input;

            playerTransform.ValueRW.Position = newPos;
            playerTransform.ValueRW.Rotation = quaternion.LookRotation(normInput, math.up());
        }
    }
}