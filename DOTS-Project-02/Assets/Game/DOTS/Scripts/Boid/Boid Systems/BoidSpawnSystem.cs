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
        // public void OnUpdate(ref SystemState state)
        // {
        //     state.Enabled = false;
        //
        //     ComponentLookup<LocalToWorld> localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        //     EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        //     WorldUnmanaged world = state.World.Unmanaged;
        //
        //     foreach (var (boidSchool, boidSchoolLocalToWorld, entity) in
        //         SystemAPI.Query<RefRO<BoidSchool>, RefRO<LocalToWorld>>()
        //             .WithEntityAccess())
        //     {
        //
        //         var boidEntities =
        //             CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(boidSchool.ValueRO.Count,
        //                 ref world.UpdateAllocator);
        //
        //         // makes multiple clones of entity
        //         state.EntityManager.Instantiate(boidSchool.ValueRO.Prefab, boidEntities);
        //
        //         var setBoidLocalToWorldJob = new SetBoidLocalToWorldJob
        //         {
        //             LocalToWorldFromEntity = localToWorldLookup,
        //             Entities = boidEntities,
        //             Center = boidSchoolLocalToWorld.ValueRO.Position,
        //             Radius = boidSchool.ValueRO.InitialRadius
        //         };
        //
        //         state.Dependency = setBoidLocalToWorldJob.Schedule(boidSchool.ValueRO.Count, 64, state.Dependency);
        //         state.Dependency.Complete();
        //
        //         ecb.DestroyEntity(entity);
        //     }
        //
        //     ecb.Playback(state.EntityManager);
        // }

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoidConfig>();
        } 
        
        public void OnUpdate(ref SystemState state)
        {
            // only run once
            state.Enabled = false;

            var entityManager = state.EntityManager;
            
            foreach (var (boidSchool, boidSchoolLocalToWorld) in
                SystemAPI.Query<RefRO<BoidSchool>, RefRO<LocalTransform>>())
            {
                for (int i = 0; i < boidSchool.ValueRO.Count; i++)
                {
                    var boid = entityManager.Instantiate(boidSchool.ValueRO.Prefab);
                    // entityManager.SetComponentData(boid, new LocalTransform
                    // {
                    //     Position = new float3(1,1,1),
                    //     //Scale = 1,
                    //     //Rotation = quaternion.identity
                    // });
                }
            }

        }
    }

    [BurstCompile]
    struct SetBoidLocalToWorldJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction]
        public ComponentLookup<LocalToWorld> LocalToWorldFromEntity;

        public NativeArray<Entity> Entities;
        public float3 Center;
        public float Radius;

        public void Execute(int index)
        {
            var entity = Entities[index];

            // random seed
            var random = new Random(((uint) (entity.Index + index + 1) * 0x9F6ABC1));

            // TODO: fix direction
            var direction = (random.NextFloat3() - new float3(0.5f, 0f, 0.5f));
            direction = math.normalizesafe(new float3(direction.x, 0, direction.z));

            float randomOffset = random.NextFloat(Radius);

            var position = Center + direction * randomOffset;
            var localToWorld = new LocalToWorld
            {
                Value = float4x4.TRS(position,
                    quaternion.LookRotationSafe(direction, math.up()),
                    new float3(1, 1, 1))
            };
            LocalToWorldFromEntity[entity] = localToWorld;
        }
    }

}