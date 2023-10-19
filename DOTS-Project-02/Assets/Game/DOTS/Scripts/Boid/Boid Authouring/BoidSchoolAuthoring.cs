using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DOTS
{
    public class BoidSchoolAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public int Count;
        public float minRadius;
        public float maxRadius;

        class Baker : Baker<BoidSchoolAuthoring>
        {
            public override void Bake(BoidSchoolAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent(entity, new BoidSchool
                {
                    Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                    Count = authoring.Count,
                    MinRadius = authoring.minRadius,
                    MaxRadius = authoring.maxRadius
                });
            }
        }
    }

    public struct BoidSchool : IComponentData
    {
        public Entity Prefab;
        public int Count;
        public float MinRadius;
        public float MaxRadius;
    }

}