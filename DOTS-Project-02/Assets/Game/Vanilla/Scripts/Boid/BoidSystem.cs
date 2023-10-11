using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using Vanilla;

public class BoidSystem : MonoBehaviour
{
    [SerializeField] private BoidSet BoidSet;

    [SerializeField] private List<Transform> targetTransforms;
    private List<Transform> obstacleTransforms = new List<Transform>();
    private List<Vector3> targetPositions = new List<Vector3>();
    private List<Vector3> obstaclePositions = new List<Vector3>();

    [Header("Steer Behaviours")]
    [SerializeField] private SeekSteerBehaviour SeekSteerBehaviour;
    [SerializeField] private AlignSteeringBehaviour AlignSteeringBehaviour;

    [Header("Config")] 
    [SerializeField] private float maxMoveSpeed;

    private float maxTargetDistnceSquared;
    
    private void Start()
    {
        obstacleTransforms = ObstacleManager.Instance.Obstacles;
        InitializePositions();
        InitializeBoids();

        maxTargetDistnceSquared = SeekSteerBehaviour.maxVisionDistance * SeekSteerBehaviour.maxVisionDistance;
    }

    private void InitializeBoids()
    {
        foreach (var boid in BoidSet.Boids)
        {
            boid.InitializeKinematic();
        }
    }

    private void InitializePositions()
    {
        for (int i = 0; i < targetTransforms.Count; i++)
        {
            targetPositions.Add(targetTransforms[i].position);
        }
        
        for (int i = 0; i < obstacleTransforms.Count; i++)
        {
            obstaclePositions.Add(obstacleTransforms[i].position);
        }
    }

    void Update()
    {
        UpdateTargetPositions();
        UpdateObstaclePositions();
        UpdateBoids();
    }

    private void UpdateTargetPositions()
    {
        for (int i = 0; i < targetTransforms.Count; i++)
        {
            targetPositions[i] = (targetTransforms[i].position);
        }
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
            Kinematic boid = BoidSet.Boids[i].Kinematic;

            SteeringOutput totalSteeringOutput = new SteeringOutput();
            totalSteeringOutput += GetSeekOutput(boid, i, boidCount);
            
            boid.UpdateSteering(totalSteeringOutput, maxMoveSpeed, Time.deltaTime);
            boid.UpdateTransform();
        }
    }

    private SteeringOutput GetSeekOutput(Kinematic boid, int i, int boidCount)
    {
        Vector3 boidPos = boid.position;
        float maxDistance = maxTargetDistnceSquared;
        Vector3 directionToClosestTarget = GetDirectionToClosest(boidPos, targetPositions, out Vector3 target, maxDistance);
        if (directionToClosestTarget == Vector3.zero)
            return new SteeringOutput();

        SeekSteerBehaviour.characterPosition = boidPos;
        SeekSteerBehaviour.targetPosition = target;
        return SeekSteerBehaviour.GetSteeringOutput();
    }

    private Vector3 GetDirectionToClosest(Vector3 boidPos, List<Vector3> positions,  out Vector3 closest, float maxDistance = float.MaxValue)
    {
        Vector3 direction = Vector3.zero;
        closest = Vector3.zero;

        float shortestDistance = maxDistance;

        foreach (var position in positions)
        {
            float squaredDistance = MathUtility.distancesq(boidPos, position);
            if (squaredDistance < shortestDistance)
            {
                direction = position - boidPos;
                shortestDistance = squaredDistance;
                closest = position;
            }
        }

        return direction;
    }
}
