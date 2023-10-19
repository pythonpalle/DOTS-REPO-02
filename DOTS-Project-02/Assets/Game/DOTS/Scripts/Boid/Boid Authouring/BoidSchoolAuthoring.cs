using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DOTS
{
    public class BoidSchoolAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public BoidCommunicator Communicator;

        class Baker : Baker<BoidSchoolAuthoring>
        {
            public override void Bake(BoidSchoolAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent(entity, new BoidSchool
                {
                    Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                    Count = authoring.Communicator.boidCount,
                    MinRadius = authoring.Communicator.minSpawnRadius,
                    MaxRadius = authoring.Communicator.maxSpawnRadius
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