using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PlayerConfigAuthoring : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject prefab;

    [Header("Movement")]
    public float speed;

    class Baker : Baker<PlayerConfigAuthoring>
    {
        public override void Bake(PlayerConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new Config
            {
                prefab = GetEntity(authoring.prefab),
                speed = authoring.speed
            });
        }
    }

    public struct Config : IComponentData
    {
        public Entity prefab;
        public float speed;
    }
}
