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
        
        [Header("Target Steer")]
        public float targetVisionDistance = 10f;
        public LinearSteering targetLinearSteering;
        public AngularSteering targetAngularSteering;

        [Header("Wander")] 
        public LinearSteering wanderLinearSteering;
        public AngularSteering wanderAngularSteering;
        public WanderParameters wanderParameters;

        [Header("Alignment")]
        public AngularSteering alignAngularSteering;

        [Header("Cohesion")]
        public LinearSteering cohesionLinearSteering;

        [Header("Separation")]
        public LinearSteering separationLinearSteering;
        public float separationDistance = 1f;
        public float separationDecayCoefficient;

        [Header("Obstacle Avoidance")]
        public LinearSteering obstacleLinearSteering;
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
                    // movement
                    moveSpeed = authoring.moveSpeed,
                    chaseSpeedModifier = authoring.chaseSpeedModifier,
                    
                    // neighbour
                    neighbourDistanceSquared = authoring.neighbourDistance * authoring.neighbourDistance,
                    halfFovInRadians = math.radians(authoring.neighbourFOV) * 0.5f,
                    
                    // target
                    targetVisionDistanceSquared = authoring.targetVisionDistance * authoring.targetVisionDistance,
                    TargetAngularSteering = authoring.targetAngularSteering,
                    targetLinearSteering = authoring.targetLinearSteering,
                    
                    // alignment
                    AlignAngularSteering = authoring.alignAngularSteering,
                    
                    // cohesion
                    cohesionLinearSteering = authoring.cohesionLinearSteering,
                    
                    // separation
                    separationLinearSteering = authoring.separationLinearSteering,
                    separationDistanceSquared = authoring.separationDistance * authoring.separationDistance,
                    separationDecayCoefficient = authoring.separationDecayCoefficient,

                    // obstacle avoidance
                    obstacleLinearSteering = authoring.obstacleLinearSteering,
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
            public LinearSteering targetLinearSteering;
            public AngularSteering TargetAngularSteering;
            
            [Header("Wander")] 
            public LinearSteering wanderLinearSteering;
            public AngularSteering WanderAngularSteering;
            public WanderParameters wanderParameters;

            [Header("Alignment")]
            public AngularSteering AlignAngularSteering;
            
            [Header("Cohesion")]
            public LinearSteering cohesionLinearSteering;
            
            [Header("Separation")]
            public LinearSteering separationLinearSteering;
            public float separationDistanceSquared;
            public float separationDecayCoefficient;
            
            [Header("Obstacle Avoidance")]
            public LinearSteering obstacleLinearSteering;
            public float obstacleAvoidanceDistanceSquared;

            [Header("System Settings")]
            public bool runSystem;
            public bool useJobs;
        }

}
