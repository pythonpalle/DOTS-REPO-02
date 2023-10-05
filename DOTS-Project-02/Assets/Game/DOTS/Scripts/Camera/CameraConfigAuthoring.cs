using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class CameraConfigAuthoring : MonoBehaviour
{
    [Header("Offset")] 
    public float3 DistanceFromPlayer = new float3(0, 35f, 7);
    public float3 Rotation = new float3(90f, 0, 0);


    class Baker : Baker<CameraConfigAuthoring>
    {
        public override void Bake(CameraConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new CameraConfig
            {
                distanceFromPlayer = authoring.DistanceFromPlayer,
                rotation = authoring.Rotation
            });
        }
    }
}

public struct CameraConfig : IComponentData
{
    public float3 distanceFromPlayer;
    public float3 rotation;
}
