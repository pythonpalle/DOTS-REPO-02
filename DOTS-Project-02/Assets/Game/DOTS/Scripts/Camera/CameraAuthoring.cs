using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CameraAuthoring :  MonoBehaviour
{
    class Baker : Baker<CameraAuthoring>
    {
        public override void Bake(CameraAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<CameraComponent>(entity);
        }
    }
}

public struct CameraComponent : IComponentData
{
}
