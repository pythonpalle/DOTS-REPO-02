using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct PlayerSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerConfigAuthoring.Config>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // only run once for spawning
        state.Enabled = false;

        var config = SystemAPI.GetSingleton<PlayerConfigAuthoring.Config>();

        var em = state.EntityManager;
        var player = em.Instantiate(config.prefab);

        em.SetComponentData(player, new LocalTransform
        {
            Position = float3.zero,
            Scale = 1,
            Rotation = Quaternion.identity
        });
        
        Debug.Log("Player spawned!");
    }
}
