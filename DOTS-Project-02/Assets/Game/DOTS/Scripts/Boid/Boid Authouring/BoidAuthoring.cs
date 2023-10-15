using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DOTS
{
    public class BoidAuthoring : MonoBehaviour
    {
        class Baker : Baker<BoidAuthoring>
        {
            public override void Bake(BoidAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Boid());
                AddComponent(entity, new WrapComponent());
                AddComponent(entity, new VelocityComponent());
                AddComponent(entity, new RotationSpeedComponent());
            }
        }
    }

    // [Serializable]
    // [WriteGroup(typeof(LocalToWorld))]
    public struct Boid : IComponentData
    {
    }
    
    public struct VelocityComponent : IComponentData
    {
        public float2 Value;
    }
    
    public struct RotationSpeedComponent : IComponentData
    {
        public float Value;
    }
}



