using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS
{
    public class BoidConfigAuthoring : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 3f;
        public float chaseSpeedModifier = 1.3f;
        
        [Header("Neighbour")]
        public float neighbourDistance = 10f;
        public float neighbourFOV = 120f;
        
        [Header("Target")]
        public float targetVisionDistance = 10f;
        public float targetWeight = 1f;

        [Header("Alignment")]
        public float alignmentWeight = 1f;
        
        [Header("Cohesion")]
        public float cohesionWeight = 1f;
        
        [Header("Separation")]
        public float separationWeight = 1f;
        public float separationDistance = 1f;
        public float separationDecayCoefficient;
        public float separationMaxAcceleration;

        [Header("Obstacle Avoidance")]
        public float obstacleAvoidanceDistance = 10f;
        public float avoidanceWeight = 1f;

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
                    // movement
                    moveSpeed = authoring.moveSpeed,
                    chaseSpeedModifier = authoring.chaseSpeedModifier,
                    
                    // neighbour
                    neighbourDistanceSquared = authoring.neighbourDistance * authoring.neighbourDistance,
                    halfFovInRadians = math.radians(authoring.neighbourFOV) * 0.5f,
                    
                    // target
                    targetVisionDistanceSquared = authoring.targetVisionDistance * authoring.targetVisionDistance,
                    targetWeight = authoring.targetWeight,
                    
                    // alignment
                    alignmentWeight = authoring.alignmentWeight,
                    
                    // cohesion
                    cohesionWeight = authoring.cohesionWeight,
                    
                    // separation
                    separationWeight = authoring.cohesionWeight,
                    separationDistanceSquared = authoring.separationDistance * authoring.separationDistance,
                    separationDecayCoefficient = authoring.separationDecayCoefficient,
                    separationMaxAcceleration = authoring.separationMaxAcceleration,

                    // obstacle avoidance
                    avoidanceWeight = authoring.avoidanceWeight,
                    obstacleAvoidanceDistanceSquared = authoring.obstacleAvoidanceDistance * authoring.obstacleAvoidanceDistance,
                    
                    // settings
                    useJobs = authoring.useJobs,
                    runSystem = authoring.runSystem,
                });
            }
        }

        }

        public struct BoidConfig : IComponentData
        {
            [Header("Movement")]
            public float moveSpeed;
            public float chaseSpeedModifier;
        
            [Header("Neighbour")]
            public float neighbourDistanceSquared;
            public float halfFovInRadians;
        
            [Header("Target")]
            public float targetVisionDistanceSquared;
            public float targetWeight;

            [Header("Alignment")]
            public float alignmentWeight;
            
            [Header("Cohesion")]
            public float cohesionWeight;
            
            [Header("Separation")]
            public float separationWeight;
            public float separationDistanceSquared;
            public float separationDecayCoefficient;
            public float separationMaxAcceleration;
            
            [Header("Obstacle Avoidance")]
            public float obstacleAvoidanceDistanceSquared;
            public float avoidanceWeight;

            [Header("System Settings")]
            public bool runSystem;
            public bool useJobs;
        }

}
