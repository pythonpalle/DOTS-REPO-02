using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Unity.Collections;
using UnityEngine;
using Vanilla;

public class BoidSystem : MonoBehaviour
{
    [SerializeField] private BoidSet BoidSet;

    [SerializeField] private List<KinematicBehaviour> targetKinematicsBehaviours;
    private List<Kinematic> targetKinematics = new List<Kinematic>();
    private List<Transform> obstacleTransforms = new List<Transform>();
    private List<Vector3> obstaclePositions = new List<Vector3>();

    [Header("Steer Behaviours")] 
    [SerializeField] private SeekSteerBehaviour seekSteerBehaviour = new SeekSteerBehaviour();
    [SerializeField] private WanderSteerBehaviour wanderSteerBehaviour = new WanderSteerBehaviour(); 
    [SerializeField] private LookWhereYoureGoingSteeringBehaviour lookWhereYoureGoingSteering = new LookWhereYoureGoingSteeringBehaviour();
    [SerializeField] private AlignSteeringBehaviour alignSteeringBehaviour = new AlignSteeringBehaviour();

    [Header("Config")]
    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float neighbourRadius;
    [SerializeField] private float neighbourFOV;

    private float maxTargetDistnceSquared;
    private float neighbourRadiusSquared;
    private float fovInRadians;

    private Kinematic averageNeighbourKinematic = new Kinematic();
    
    private void Start()
    {
        obstacleTransforms = ObstacleManager.Instance.Obstacles;
        InitializePositions();
        InitializeBoids();
        InitializeTargets();

        UpdateConfigVariables();
        lookWhereYoureGoingSteering.target = new Kinematic();
        wanderSteerBehaviour.target = new Kinematic();
    }

    private void OnValidate()
    {
        UpdateConfigVariables();
    }

    private void UpdateConfigVariables()
    {
        maxTargetDistnceSquared = seekSteerBehaviour.maxVisionDistance * seekSteerBehaviour.maxVisionDistance;
        neighbourRadiusSquared = neighbourRadius * neighbourRadius;
        fovInRadians = neighbourFOV * Mathf.Deg2Rad;
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

    private void InitializePositions()
    {
        for (int i = 0; i < obstacleTransforms.Count; i++)
        {
            obstaclePositions.Add(obstacleTransforms[i].position);
        }
    }

    void Update()
    {
        UpdateObstaclePositions();
        UpdateBoids();
    }

    private void UpdateObstaclePositions()
    {
        for (int i = 0; i < obstacleTransforms.Count; i++)
        {
            obstaclePositions[i] = (obstacleTransforms[i].position);
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
            totalSteeringOutput += GetSeekOutput(boidKinematic, i, boidCount);
            totalSteeringOutput += GetLookWhereYouAreGoingOutput(boidKinematic);
            totalSteeringOutput += GetAlignmentOutput(boidKinematic, boidNeighbours);
            totalSteeringOutput += GetWanderOutput(boidKinematic);
            
            boidKinematic.UpdateSteering(totalSteeringOutput, maxMoveSpeed, Time.deltaTime);
            BoidSet.Boids[i].UpdateKinematicTransform();
        }
    }
    
    private SteeringOutput GetSeekOutput(Kinematic boid, int i, int boidCount)
    {
        Vector3 boidPos = boid.position;
        float maxDistance = maxTargetDistnceSquared;
        Vector3 directionToClosestTarget = GetDirectionToClosestKinematic(boidPos, targetKinematics, out Kinematic target, maxDistance);
        if (directionToClosestTarget == Vector3.zero) 
            return new SteeringOutput();

        seekSteerBehaviour.target = target;
        seekSteerBehaviour.character = boid;
        return seekSteerBehaviour.GetSteeringOutput() * seekSteerBehaviour.weight;
    }
    
    private SteeringOutput GetLookWhereYouAreGoingOutput(Kinematic boidKinematic)
    {
        lookWhereYoureGoingSteering.character = boidKinematic;
        
        var boidDirection = boidKinematic.velocity;
        float targetAngle = Mathf.Atan2(boidDirection.z, boidDirection.x);
        lookWhereYoureGoingSteering.target.orientation = targetAngle;
        lookWhereYoureGoingSteering.target.rotationSpeed = boidKinematic.rotationSpeed;

        return lookWhereYoureGoingSteering.GetSteeringOutput() * lookWhereYoureGoingSteering.weight; 
    }

    private SteeringOutput GetWanderOutput(Kinematic boidKinematic)
    {
        wanderSteerBehaviour.character = boidKinematic;
        return wanderSteerBehaviour.GetSteeringOutput() * wanderSteerBehaviour.weight;
    }
    
    private SteeringOutput GetAlignmentOutput(Kinematic boidKinematic, List<Kinematic> boidNeighbours)
    {
        alignSteeringBehaviour.character = boidKinematic;
        alignSteeringBehaviour.target = averageNeighbourKinematic;

        return alignSteeringBehaviour.GetSteeringOutput() * alignSteeringBehaviour.weight;
    }

    private void SetAverageNeighbourKinematic(List<Kinematic> boidNeighbours)
    {
        int neighbourCount = boidNeighbours.Count;
        if (neighbourCount == 0)
            return;
        
        Vector3 averagePosition = Vector3.zero;
        float averageOrientation = 0f;
        
        foreach (var boid in boidNeighbours)
        {
            averagePosition += boid.position;
            averageOrientation += boid.orientation;
        }

        averagePosition /= neighbourCount;
        averageOrientation /= neighbourCount;
        averageNeighbourKinematic.position = averagePosition;
        averageNeighbourKinematic.orientation = averageOrientation;
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
                var rotation = MathUtility.MapToRange(boidOrientation - otherKinematic.orientation);
                if (Mathf.Abs(rotation) < fovInRadians)
                    neighbours.Add(otherBoid.Kinematic);
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
