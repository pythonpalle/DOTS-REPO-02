using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BoidConfigAuthoring : MonoBehaviour
{
    [Header("Movement")] 
    public float moveSpeed;
    
    class Baker : Baker<BoidConfigAuthoring>
    {
        public override void Bake(BoidConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new BoidConfig
            {
                moveSpeed = authoring.moveSpeed
            });
        }
    }
}

public struct BoidConfig : IComponentData
{
    public float moveSpeed;
}
