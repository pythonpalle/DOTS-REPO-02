using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Unity.Assertions;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using Vanilla;

namespace Vanilla
{
    public class BoidSystem : MonoBehaviour
{
    [SerializeField] private BoidSet BoidSet;

    [SerializeField] private List<KinematicBehaviour> targetKinematicsBehaviours;
    private List<Kinematic> targetKinematics = new List<Kinematic>();
    private List<Kinematic> obstacleKinematics = new List<Kinematic>();

    [Header("Steer Behaviours")] 
    [SerializeField] private SeekSteerBehaviour targetSeekSteerBehaviour = new SeekSteerBehaviour();
    [SerializeField] private LookWhereYoureGoingSteeringBehaviour lookWhereYoureGoingSteering = new LookWhereYoureGoingSteeringBehaviour();
    [Space]
    [SerializeField] private WanderSteerBehaviour wanderSteerBehaviour = new WanderSteerBehaviour(); 
    [SerializeField] private ObstacleAvoidanceSteeringBehaviour avoidanceSteeringBehaviour = new ObstacleAvoidanceSteeringBehaviour(); 
    [Space]
    [SerializeField] private SeekSteerBehaviour cohesionSteerBehaviour = new SeekSteerBehaviour();
    [SerializeField] private AlignSteeringBehaviour alignSteeringBehaviour = new AlignSteeringBehaviour();
    [SerializeField] private SeparationSteerBehaviour separationSteerBehaviour = new SeparationSteerBehaviour();

    [Header("Config")]
    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float maxChaseDistance;
    [SerializeField] private float chaseSpeedModififer;
    [SerializeField] private float neighbourRadius;
    [SerializeField] private float neighbourFOV;
    
    private float maxTargetDistnceSquared;
    private float neighbourRadiusSquared;
    private float halfFovInRadian;

    private Kinematic averageNeighbourKinematic = new Kinematic();
    
    private ProfilerMarker boidMarker;

    private void Start()
    {
        obstacleKinematics = ObstacleManager.Instance.ObstacleKinematics;
        InitializeBoids();
        InitializeTargets();

        UpdateConfigVariables();
        
        lookWhereYoureGoingSteering.target = new Kinematic();
        wanderSteerBehaviour.target = new Kinematic();
        
        cohesionSteerBehaviour.target = averageNeighbourKinematic;
        alignSteeringBehaviour.target = averageNeighbourKinematic;
        
        boidMarker = new ProfilerMarker("Vanilla");
    }

    private void OnValidate()
    {
        UpdateConfigVariables();
    }

    private void UpdateConfigVariables()
    {
        maxTargetDistnceSquared = maxChaseDistance * maxChaseDistance;
        neighbourRadiusSquared = neighbourRadius * neighbourRadius;
        halfFovInRadian = neighbourFOV * Mathf.Deg2Rad * 0.5f;
    }

    private void InitializeTargets()
    {
        foreach (var targetKinematicBehaviour in targetKinematicsBehaviours)
        {
            targetKinematics.Add(targetKinematicBehaviour.Kinematic);
        }
    }

    private void InitializeBoids()
    {
        foreach (var boid in BoidSet.Boids)
        {
            boid.InitalizeKinematic();
        }
    }

    void LateUpdate()
    {
        using (boidMarker.Auto())
        {
            UpdateBoids();
        }
    }
    

    private void UpdateBoids()
    {
        int boidCount = BoidSet.Boids.Count;
        if (boidCount == 0) 
            return;

        for (int i = 0; i < boidCount; i++)
        {
            Boid boid = BoidSet.Boids[i];
            Kinematic boidKinematic = boid.Kinematic;

            List<Kinematic> boidNeighbours = GetNeighbours(boid);
            SetAverageNeighbourKinematic(boidNeighbours);
            
            SteeringOutput totalSteeringOutput = new SteeringOutput();

            var seekOutput = GetSeekOutput(boidKinematic);
            bool seesPlayer = seekOutput.linear != Vector3.zero;
            int neighbourCount = boidNeighbours.Count;
            bool checkAlignAndCohesion = neighbourCount > 0 && !seesPlayer;

            // prioritize system:
            // 1. if sees target, chase it
            totalSteeringOutput += seekOutput;
            totalSteeringOutput += GetLookWhereYouAreGoingOutput(boidKinematic, seesPlayer);
            
            // 2. else, wander around
            totalSteeringOutput += GetWanderOutput(boidKinematic, seesPlayer);
            
            // 3. if has neighbour, use alignment and cohesion
            totalSteeringOutput += GetAlignmentOutput(boidKinematic, checkAlignAndCohesion);
            totalSteeringOutput += GetCohesionOutput(boidKinematic, checkAlignAndCohesion);
            
            // 4. always check for separation and obstacles
            totalSteeringOutput += GetSeparationOutput(boidKinematic, boidNeighbours, seesPlayer);
            totalSteeringOutput += GetObstacleAvoidanceSteering(boidKinematic);

            float speed = maxMoveSpeed * (seesPlayer ? chaseSpeedModififer : 1);
            boidKinematic.UpdateSteering(totalSteeringOutput, speed, Time.deltaTime);
            BoidSet.Boids[i].UpdateKinematicTransform();
        }
    }

    private SteeringOutput GetObstacleAvoidanceSteering(Kinematic boidKinematic)
    {
        GetDirectionToClosestKinematic(boidKinematic.position, obstacleKinematics, out Kinematic obstacle);
        avoidanceSteeringBehaviour.target = obstacle;
        avoidanceSteeringBehaviour.character = boidKinematic;
        return avoidanceSteeringBehaviour.GetSteeringOutput() * avoidanceSteeringBehaviour.weight;
    }

    private SteeringOutput GetSeparationOutput(Kinematic boidKinematic, List<Kinematic> boidNeighbours, bool seesPlayer)
    {
        separationSteerBehaviour.neighbours = boidNeighbours;
        separationSteerBehaviour.character = boidKinematic;

        float weightModifer = seesPlayer ? 1.5f : 1;
        return separationSteerBehaviour.GetSteeringOutput() * separationSteerBehaviour.weight * weightModifer;
    }

    private SteeringOutput GetCohesionOutput(Kinematic boidKinematic, bool checkCohesion)
    {
        if (!checkCohesion)
            return new SteeringOutput();
        
        cohesionSteerBehaviour.character = boidKinematic;
        return cohesionSteerBehaviour.GetSteeringOutput() * cohesionSteerBehaviour.weight;
    }

    private SteeringOutput GetSeekOutput(Kinematic boid)
    {
        Vector3 boidPos = boid.position;
        float maxDistance = maxTargetDistnceSquared;
        Vector3 directionToClosestTarget = GetDirectionToClosestKinematic(boidPos, targetKinematics, out Kinematic target, maxDistance);
        if (directionToClosestTarget == Vector3.zero)
        {
            return new SteeringOutput();
        }

        targetSeekSteerBehaviour.target = target;
        targetSeekSteerBehaviour.character = boid;
        return targetSeekSteerBehaviour.GetSteeringOutput() * targetSeekSteerBehaviour.weight;
    }
    
    private SteeringOutput GetLookWhereYouAreGoingOutput(Kinematic boidKinematic, bool seesPlayer)
    {
        if (!seesPlayer)
            return new SteeringOutput();
        
        lookWhereYoureGoingSteering.character = boidKinematic;
        
        var boidDirection = boidKinematic.velocity;
        float targetAngle = Mathf.Atan2(boidDirection.z, boidDirection.x);
        lookWhereYoureGoingSteering.target.orientation = targetAngle;
        lookWhereYoureGoingSteering.target.rotationSpeed = boidKinematic.rotationSpeed;

        return lookWhereYoureGoingSteering.GetSteeringOutput() * lookWhereYoureGoingSteering.weight; 
    }

    private SteeringOutput GetWanderOutput(Kinematic boidKinematic, bool seesPlayer)
    {
        if (seesPlayer)
            return new SteeringOutput();

        wanderSteerBehaviour.character = boidKinematic;
        return wanderSteerBehaviour.GetSteeringOutput() *  wanderSteerBehaviour.weight;
    }
    
    private SteeringOutput GetAlignmentOutput(Kinematic boidKinematic, bool checkAlignment)
    {
        if (!checkAlignment)
            return new SteeringOutput();
        
        alignSteeringBehaviour.character = boidKinematic;
        return alignSteeringBehaviour.GetSteeringOutput() * alignSteeringBehaviour.weight;
    }

    private void SetAverageNeighbourKinematic(List<Kinematic> boidNeighbours)
    {
        int neighbourCount = boidNeighbours.Count;
        if (neighbourCount == 0)
            return;
        
        Vector3 averagePosition = Vector3.zero;
        Vector2 averageOrientationAsVector = Vector2.zero;
        float averageOrientation = 0f;
        
        foreach (var boid in boidNeighbours)
        {
            averagePosition += boid.position;
            averageOrientationAsVector += MathUtility.AngleRotationAsVector2(boid.orientation);
        }

        averageOrientationAsVector /= neighbourCount;

        averageOrientation = MathUtility.DirectionToFloat(averageOrientationAsVector);
        averagePosition /= neighbourCount;
        averageNeighbourKinematic.position = averagePosition;
        averageNeighbourKinematic.orientation = MathUtility.MapToRange0To2Pie(averageOrientation);
    }

    private List<Kinematic> GetNeighbours(Boid boid)
    {
        List<Kinematic> neighbours = new List<Kinematic>();
        var boidPois = boid.Kinematic.position;
        var boidOrientation = boid.Kinematic.orientation;
        foreach (var otherBoid in BoidSet.Boids)
        {
            if (otherBoid == boid) continue;

            var otherKinematic = otherBoid.Kinematic;

            // check if other boid is close enough
            if (MathUtility.distancesq(boidPois, otherKinematic.position) < neighbourRadiusSquared)
            {
                var directionToNeighbour = (otherKinematic.position - boidPois).normalized;
                
                float orientationToOther = MathUtility.DirectionAsFloat(directionToNeighbour);
                orientationToOther = MathUtility.MapToRange0To2Pie(orientationToOther);
                
                // check if within FOV
                var rotation = (boidOrientation - orientationToOther);
                if (Mathf.Abs(rotation) < halfFovInRadian)
                {
                    neighbours.Add(otherBoid.Kinematic);
                }
            }
        }

        return neighbours;
    }

    private Vector3 GetDirectionToClosestKinematic(Vector3 boidPos, List<Kinematic> positions,  out Kinematic closest, float maxDistance = float.MaxValue)
    {
        Vector3 direction = Vector3.zero;
        closest = null;

        float shortestDistance = maxDistance;

        foreach (var kinematic in positions)
        {
            float squaredDistance = MathUtility.distancesq(boidPos, kinematic.position);
            if (squaredDistance < shortestDistance)
            {
                direction = kinematic.position - boidPos;
                shortestDistance = squaredDistance;
                closest = kinematic;
            }
        }

        return direction;
    }
}

}

