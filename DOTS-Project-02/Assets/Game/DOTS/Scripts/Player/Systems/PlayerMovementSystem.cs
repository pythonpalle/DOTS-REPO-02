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
        state.RequireForUpdate<PlayerConfig>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<PlayerConfig>();
        var obstacleConfig = SystemAPI.GetSingleton<ObstacleConfig>();

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var added = new float3(horizontal, 0, vertical);

        if (added.Equals(float3.zero))
            return;

        float sprintModifier = Input.GetKey(KeyCode.LeftShift) ? config.sprintModifer : 1;
        var input = math.normalize(added) * SystemAPI.Time.DeltaTime * config.speed * sprintModifier;


        float minDis = config.radius + obstacleConfig.radius;
        float minDisSq = minDis * minDis;
        
        foreach (var playerTransform in
            SystemAPI.Query<RefRW<LocalTransform>>().WithAll<PlayerMovement>())
        {
            playerTransform.ValueRW.Rotation = quaternion.LookRotationSafe(input, math.up());
            var newPos = playerTransform.ValueRW.Position + input;

            foreach (var (obstacleTransform, obstacle) in 
                SystemAPI.Query<RefRO<LocalTransform>, RefRO<Obstacle>>())
            {
                var obstclePos = obstacleTransform.ValueRO.Position;

                float squareDis = math.distancesq(newPos, obstclePos);

                if (squareDis < minDisSq)
                {
                    newPos = playerTransform.ValueRO.Position;
                    break;
                }
            }

            playerTransform.ValueRW.Position = newPos;
        }
    }
}