using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BoidConfigAuthoring : MonoBehaviour
{
    [Header("Movement")] 
    public float moveSpeed = 2f;

    [Header("Alignment")] 
    public float alignmentWeight = 1f;
    
    [Header("Cohesion")] 
    public float cohesionWeight = 1f;
    
    [Header("Separation")] 
    public float separationWeight = 1f;
    
    [Header("Target")] 
    public float targetWeight = 1f;
    public float targetLookDistance = 10f;
    
    [Header("Obstacle Avoidance")] 
    public float obstacleWeight = 1f;
    public float obstacleAvoidanceDistance = 10f;
    
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
