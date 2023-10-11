using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS
{
    public class ObstacleConfigAuthoring : MonoBehaviour
    {
        public GameObject prefab;
        public float radius;

        class Baker : Baker<ObstacleConfigAuthoring>
        {
            public override void Bake(ObstacleConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new ObstacleConfig
                {
                    prefab = GetEntity(authoring.prefab),
                    radius = authoring.radius
                });
            }
        }
    }

    public struct ObstacleConfig : IComponentData
    {
        public Entity prefab;
        public float radius;
    }
}