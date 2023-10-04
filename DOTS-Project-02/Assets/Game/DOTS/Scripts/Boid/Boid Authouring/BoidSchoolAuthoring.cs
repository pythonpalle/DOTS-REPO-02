using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class BoidSchoolAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public int Count;
    public float initialRadius;
    
    class Baker : Baker<BoidSchoolAuthoring>
    {
        public override void Bake(BoidSchoolAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new BoidSchool
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                Count = authoring.Count,
                InitialRadius = authoring.initialRadius
            });
        }
    }
}

public struct BoidSchool : IComponentData
{
    public Entity Prefab;
    public int Count;
    public float InitialRadius;
}
