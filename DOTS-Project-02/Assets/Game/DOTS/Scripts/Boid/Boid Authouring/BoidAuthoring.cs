using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class BoidAuthoring : MonoBehaviour
{
    class Baker : Baker<BoidAuthoring>
    {
        public override void Bake(BoidAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Boid
            {
            });
        }
    }
}

[Serializable]
[WriteGroup(typeof(LocalToWorld))]
public struct Boid : IComponentData
{
}

