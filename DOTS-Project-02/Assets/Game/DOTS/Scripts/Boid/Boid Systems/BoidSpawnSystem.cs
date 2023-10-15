using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = Unity.Mathematics.Random;

namespace DOTS
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct BoidSpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoidConfig>();
        } 
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // only run once
            state.Enabled = false;

            var entityManager = state.EntityManager;

            uint schoolCount = 0;
            foreach (var (boidSchool, boidSchoolLocalToWorld) in
                SystemAPI.Query<RefRO<BoidSchool>, RefRO<LocalToWorld>>())
            {
                var center = boidSchoolLocalToWorld.ValueRO.Position;
                float radius = boidSchool.ValueRO.InitialRadius;
                
                for (uint i = 0; i < boidSchool.ValueRO.Count; i++)
                {
                    var boid = entityManager.Instantiate(boidSchool.ValueRO.Prefab);
                    uint randomSeed = (i + 1) * (schoolCount + 1) * 0x2E1BB2;
                    GetRandomPositionAndRotation(randomSeed, center, radius, out float3 position, out quaternion rotation);
                    
                    entityManager.SetComponentData(boid, new LocalTransform
                    {
                        Position = position,
                        Scale = 1,
                        Rotation = rotation,
                    });
                }

                schoolCount++;
            }
        }

        private void GetRandomPositionAndRotation(uint seed, float3 center, float radius, out float3 position, out quaternion rotation)
        {
            var random = new Random(seed);

            var direction = (random.NextFloat3() - new float3(0.5f, 0f, 0.5f));
            direction = math.normalizesafe(new float3(direction.x, 0, direction.z));

            float randomOffset = random.NextFloat(radius);

            position = center + direction * randomOffset;
            rotation = quaternion.LookRotationSafe(direction, math.up());
        }
    }
}