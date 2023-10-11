using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using Vanilla;

public class BoidSystem : MonoBehaviour
{
    [SerializeField] private BoidSet BoidSet;

    [SerializeField] private List<Kinematic> targetKinematics;
    private List<Transform> obstacleTransforms = new List<Transform>();
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
        Vector3 directionToClosestTarget = GetDirectionToClosestKinematic(boidPos, targetKinematics, out Kinematic target, maxDistance);
        if (directionToClosestTarget == Vector3.zero) 
            return new SteeringOutput();

        SeekSteerBehaviour.target = target;
        SeekSteerBehaviour.character = boid;
        return SeekSteerBehaviour.GetSteeringOutput();
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
