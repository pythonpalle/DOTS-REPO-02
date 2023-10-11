using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS
{
    public partial struct CameraFollowPlayerSystem : ISystem
    {

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerConfig>();
            state.RequireForUpdate<CameraConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var cameraConfig = SystemAPI.GetSingleton<CameraConfig>();

            var camera = Camera.main;
            if (!camera)
                return;


            // player
            foreach (var playerTransform in
                SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerComponent>())
            {
                camera.transform.position = playerTransform.ValueRO.Position + cameraConfig.distanceFromPlayer;
            }
        }
    }
}