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
        var added = new float3(horizontal, 0, vertical);
        
        if (added.Equals(float3.zero))
            return;
        
        var input = math.normalize(added) * SystemAPI.Time.DeltaTime * config.speed;

        // TODO: Add collision checks before movement
        foreach (var playerTransform in 
            SystemAPI.Query<RefRW<LocalTransform>>().WithAll<PlayerAuthoring.PlayerMovement>())
        {
            var newPos = playerTransform.ValueRO.Position + input;
            playerTransform.ValueRW.Position = newPos;

            float angle = math.atan2(-vertical, horizontal);
            playerTransform.ValueRW.Rotation = quaternion.EulerXYZ(0, angle, 0);
        }
    }
}
