using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerConfigAuthoring : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject prefab;

    [Header("Spawning")] 
    public float3 spawnPosition = new float3(0, 0.5f, 0);

    [Header("Movement")]
    public float speed;
    public float sprintModifier;
    
    [Header("Body")]
    public float radius;

    class Baker : Baker<PlayerConfigAuthoring>
    {
        public override void Bake(PlayerConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new PlayerConfig
            {
                prefab = GetEntity(authoring.prefab),
                speed = authoring.speed,
                spawnPosition = authoring.spawnPosition,
                radius = authoring.radius,
                sprintModifer = authoring.sprintModifier
            });
        }
    }
}

public struct PlayerConfig : IComponentData
{
    public Entity prefab;
    public float speed;
    public float sprintModifer;
    public float3 spawnPosition;
    public float radius;
}
