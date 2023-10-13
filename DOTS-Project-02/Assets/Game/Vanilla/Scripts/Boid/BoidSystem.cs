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
    [SerializeField] private float maxAppearenceDistance;
    [SerializeField] private float chaseSpeedModififer;
    [SerializeField] private float neighbourRadius;
    [SerializeField] private float neighbourFOV;

    [Header("Debug")]
    [SerializeField] private bool debugNeighbours;

    private float maxTargetDistnceSquared;
    private float neighbourRadiusSquared;
    private float halfFovInRadian;

    private Kinematic averageNeighbourKinematic = new Kinematic();
    private Camera mainCamera;
    
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
        
        mainCamera = Camera.main;
    }

    private void OnValidate()
    {
        UpdateConfigVariables();
    }

    private void UpdateConfigVariables()
    {
        maxTargetDistnceSquared = maxChaseDistance * maxChaseDistance;
        neighbourRadiusSquared = neighbourRadius * neighbourRadius;
        halfFovInRadian = neighbourFOV * Mathf.Deg2Rad;
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

    void Update()
    {
        UpdateBoids();
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
            
            // wrap around if heading outside screen bounds
            if (OutsideOfScreen(boidKinematic.position, out Vector3 viewPortPos))
            {
                boidKinematic.position = viewPortPos;
                boid.UpdateKinematicTransform();
                continue;
            }

            List<Kinematic> boidNeighbours = GetNeighbours(boid);
            SetAverageNeighbourKinematic(boidNeighbours);
            
            SteeringOutput totalSteeringOutput = new SteeringOutput();

            bool usePriority = true;
            bool seesPlayer = false;
            if (usePriority)
            {
                // prioritize chasing player if player nearby
                var seekOutput = GetSeekOutput(boidKinematic);
                seesPlayer = seekOutput.linear.magnitude > 0;
                if (seesPlayer)
                {
                    totalSteeringOutput += seekOutput;
                    totalSteeringOutput += GetLookWhereYouAreGoingOutput(boidKinematic);
                }
                else
                {
                    totalSteeringOutput += GetWanderOutput(boidKinematic);
                    
                    if (boidNeighbours.Count > 0)
                    {
                        totalSteeringOutput += GetAlignmentOutput(boidKinematic);
                        totalSteeringOutput += GetCohesionOutput(boidKinematic);
                    }
                }
            
                totalSteeringOutput += GetSeparationOutput(boidKinematic, boidNeighbours);
                totalSteeringOutput += GetObstacleAvoidanceSteering(boidKinematic);
            }
            else
            {
                totalSteeringOutput += GetSeekOutput(boidKinematic);
                totalSteeringOutput += GetLookWhereYouAreGoingOutput(boidKinematic);;
                totalSteeringOutput += GetAlignmentOutput(boidKinematic);
                totalSteeringOutput += GetCohesionOutput(boidKinematic);
                totalSteeringOutput += GetWanderOutput(boidKinematic);
                totalSteeringOutput += GetSeparationOutput(boidKinematic, boidNeighbours);
                totalSteeringOutput += GetObstacleAvoidanceSteering(boidKinematic);
            }




            float speed = maxMoveSpeed * (seesPlayer ? chaseSpeedModififer : 1);
            boidKinematic.UpdateSteering(totalSteeringOutput, speed, Time.deltaTime);
            BoidSet.Boids[i].UpdateKinematicTransform();
        }
    }

    private bool OutsideOfScreen(Vector3 inPosition, out Vector3 newPos)
    {
        return ScreenManager.OutsideOfScreen(inPosition, out newPos);
        
        var viewportPos = mainCamera.WorldToViewportPoint(inPosition);
        newPos = new Vector3();
        float offset = 0.05f;
        float halfOffset = offset * 0.5f;

        bool outside = false;
        
        if (viewportPos.x < -offset)
        {
            viewportPos.x = 1+halfOffset;
            outside = true;
        }
        else if (viewportPos.x > 1+offset)
        {
            viewportPos.x = -halfOffset;
            outside = true;
        }

        if (viewportPos.y < -offset)
        {
            viewportPos.y = 1+halfOffset;
            outside = true;
        }
        else if (viewportPos.y > 1+offset)
        {
            viewportPos.y = -halfOffset;
            outside = true;
        }

        newPos = mainCamera.ViewportToWorldPoint(viewportPos);
        return outside;
    }
    
    private void SetKinematicFromNeighbours(Kinematic boid, List<Kinematic> boidNeighbours, Kinematic kinematic,
        float fov, out int count)
    {
        kinematic.position = Vector3.zero;
        kinematic.orientation = 0;
        
        var boidPois = boid.position;
        var boidOrientation = boid.orientation;

        Vector3 averagePos = Vector3.zero;
        float averageOrientation = 0;

        count = 0;
        
        foreach (var otherBoid in boidNeighbours)
        {
            if (otherBoid == boid) continue;

            if (WithinFOV(boidPois, boidOrientation, otherBoid, fov))
            {
                averageOrientation += otherBoid.orientation;
                averagePos += otherBoid.position;
                count++;
            }
        }

        if (count == 0)
            return;

        kinematic.position = averagePos / count;
        kinematic.orientation = averageOrientation / count;
    }

    private bool WithinFOV(Vector3 boidPos, float boidOrientation, Kinematic otherBoid, float fov)
    {
        var directionToNeighbour = (otherBoid.position - boidPos).normalized;
        float orientationToOther = MathUtility.DirectionAsFloat(directionToNeighbour);
        orientationToOther = MathUtility.MapToRange(orientationToOther);
                
        var rotation = (boidOrientation - orientationToOther);
        return Mathf.Abs(rotation) < Mathf.Deg2Rad * fov * 0.5f;
    }

    private SteeringOutput GetObstacleAvoidanceSteering(Kinematic boidKinematic)
    {
        GetDirectionToClosestKinematic(boidKinematic.position, obstacleKinematics, out Kinematic obstacle);
        avoidanceSteeringBehaviour.target = obstacle;
        avoidanceSteeringBehaviour.character = boidKinematic;
        return avoidanceSteeringBehaviour.GetSteeringOutput() * avoidanceSteeringBehaviour.weight;
    }

    private SteeringOutput GetSeparationOutput(Kinematic boidKinematic, List<Kinematic> boidNeighbours)
    {
        separationSteerBehaviour.neighbours = boidNeighbours;
        separationSteerBehaviour.character = boidKinematic;
        return separationSteerBehaviour.GetSteeringOutput() * separationSteerBehaviour.weight;
    }

    private SteeringOutput GetCohesionOutput(Kinematic boidKinematic)
    {
        cohesionSteerBehaviour.character = boidKinematic;
        return cohesionSteerBehaviour.GetSteeringOutput() * cohesionSteerBehaviour.weight;
    }

    private SteeringOutput GetSeekOutput(Kinematic boid)
    {
        Vector3 boidPos = boid.position;
        float maxDistance = maxTargetDistnceSquared;
        Vector3 directionToClosestTarget = GetDirectionToClosestKinematic(boidPos, targetKinematics, out Kinematic target, maxDistance);
        if (directionToClosestTarget == Vector3.zero) 
            return new SteeringOutput();

        targetSeekSteerBehaviour.target = target;
        targetSeekSteerBehaviour.character = boid;
        return targetSeekSteerBehaviour.GetSteeringOutput() * targetSeekSteerBehaviour.weight;
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
        return wanderSteerBehaviour.GetSteeringOutput() * wanderSteerBehaviour.weight ;
    }
    
    private SteeringOutput GetAlignmentOutput(Kinematic boidKinematic)
    {
        alignSteeringBehaviour.character = boidKinematic;
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
                var directionToNeighbour = (otherKinematic.position - boidPois).normalized;
                float orientationToOther = MathUtility.DirectionAsFloat(directionToNeighbour);
                orientationToOther = MathUtility.MapToRange(orientationToOther);
                
                var rotation = (boidOrientation - orientationToOther);
                if (Mathf.Abs(rotation) < halfFovInRadian)
                {
                    neighbours.Add(otherBoid.Kinematic);

                    if (debugNeighbours)
                    {
                        Debug.Log($"Boid orientation: {boidOrientation}");
                        Debug.Log($"boid degrees: {boidOrientation*Mathf.Rad2Deg}");
                        Debug.Log($"Direction to other: {directionToNeighbour}");
                        Debug.Log($"Direction as radians: {orientationToOther}");
                    
                        Debug.Log($"Rotation: {rotation} rads, {rotation*Mathf.Rad2Deg} degrees");
                    
                        Vector3 orientationAsVector = MathUtility.AngleRotationAsVector(boidOrientation);
                        float angle = Vector3.Angle(directionToNeighbour, orientationAsVector);
                        Debug.Log($"Angle: {angle}");
                    
                        Debug.DrawLine(boidPois, boidPois + MathUtility.AngleRotationAsVector(boidOrientation) * neighbourRadius, Color.red);
                        Debug.DrawLine(boidPois, boidPois + boid.transform.forward * 3, Color.blue);
                        Debug.DrawLine(boidPois, boidPois + MathUtility.AngleRotationAsVector(boidOrientation) * neighbourRadius, Color.red);
                        Debug.DrawLine(boidPois, otherKinematic.position);
                    }
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
