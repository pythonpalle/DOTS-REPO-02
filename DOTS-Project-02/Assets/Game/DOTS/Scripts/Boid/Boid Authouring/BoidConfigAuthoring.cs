using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BoidConfigAuthoring : MonoBehaviour
{
    [Header("Movement")] 
    public float moveSpeed = 2f;

    [Header("Weights")] 
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;
    public float separationWeight = 1f;
    public float targetWeight = 1f;
    public float avoidanceWeight = 1f;

    [Header("Vision")]
    public float boidVisionDistance = 10f;
    public float targetVisionDistance = 10f;
    public float obstacleAvoidanceDistance = 10f;

    [Header("System Settings")] 
    public bool runSystem = true;
    public bool useJobs;
    
    class Baker : Baker<BoidConfigAuthoring>
    {
        public override void Bake(BoidConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new BoidConfig
            {
                useJobs = authoring.useJobs,
                runSystem = authoring.runSystem,
                
                moveSpeed = authoring.moveSpeed,

                alignmentWeight = authoring.alignmentWeight,
                cohesionWeight = authoring.cohesionWeight,
                separationWeight = authoring.cohesionWeight,
                targetWeight = authoring.targetWeight,
                avoidanceWeight = authoring.avoidanceWeight,
                
                targetVisionDistanceSquared = authoring.targetVisionDistance * authoring.targetVisionDistance,
                obstacleAvoidanceDistanceSquared = authoring.obstacleAvoidanceDistance * authoring.obstacleAvoidanceDistance,
            });
        }
    }
}

public struct BoidConfig : IComponentData
{
    [Header("Movement")] 
    public float moveSpeed;

    [Header("Weights")] 
    public float alignmentWeight ;
    public float cohesionWeight ;
    public float separationWeight ;
    public float targetWeight ;
    public float avoidanceWeight ;

    [Header("Vision")]
    public float boidVisionDistance ;
    public float targetVisionDistanceSquared ;
    public float obstacleAvoidanceDistanceSquared ;

    [Header("System Settings")] 
    public bool runSystem;
    public bool useJobs;
}
