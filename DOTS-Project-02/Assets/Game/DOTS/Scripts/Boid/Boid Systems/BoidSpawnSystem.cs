using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial struct BoidSpawnSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        
        var localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var world = state.World.Unmanaged;

        foreach (var (boidSchool, boidSchoolLocalToWorld, entity) in 
            SystemAPI.Query<RefRO<BoidSchool>, RefRO<LocalToWorld>>()
            .WithEntityAccess())
        {
            
            var boidEntities = 
                CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(boidSchool.ValueRO.Count,
                    ref world.UpdateAllocator);

            // makes multiple clones of entity
            state.EntityManager.Instantiate(boidSchool.ValueRO.Prefab, boidEntities);

            var setBoidLocalToWorldJob = new SetBoidLocalToWorldJob
            {
                LocalToWorldFromEntity = localToWorldLookup,
                Entities = boidEntities,
                Center = boidSchoolLocalToWorld.ValueRO.Position,
                Radius = boidSchool.ValueRO.InitialRadius
            };

            state.Dependency = setBoidLocalToWorldJob.Schedule(boidSchool.ValueRO.Count, 64, state.Dependency);
            state.Dependency.Complete();
            
            ecb.DestroyEntity(entity);
        }
        
        ecb.Playback(state.EntityManager);
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
        var random = new Random(((uint)(entity.Index + index + 1) * 0x9F6ABC1));
        
        // TODO: fix direction
        var direction = (random.NextFloat3() - new float3(0.5f, 0f, 0.5f));
        direction = math.normalizesafe(new float3(direction.x, 0, direction.z));

        float randomOffset = random.NextFloat(Radius);
        
        var position = Center + direction * randomOffset;
        var localToWorld = new LocalToWorld
        {
            Value = float4x4.TRS(position, 
                quaternion.LookRotationSafe(direction, math.up()), 
                new float3(1,1,1))
        };
        LocalToWorldFromEntity[entity] = localToWorld;
    }
}