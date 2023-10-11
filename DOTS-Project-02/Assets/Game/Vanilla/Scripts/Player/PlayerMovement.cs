using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vanilla
{
    public class PlayerMovement : MonoBehaviour
    {
        public Kinematic Kinematic;
        
        [Header("Movement Variables")]
        public float sprintSpeedModifer = 2f;
        public float speed = 5f;
        public float maxRotationSpeed = 1f;

        [Header("Steering Behaviours")]
        [SerializeField] private SeekSteerBehaviour _seekSteerBehaviour = new SeekSteerBehaviour();
        [SerializeField] private LookWhereYoureGoingSteeringBehaviour lookSteeringBehaviour = new LookWhereYoureGoingSteeringBehaviour();

        private void Start()
        {
            _seekSteerBehaviour.character = Kinematic;
            lookSteeringBehaviour.character = Kinematic;
        }

        void Update()
        {
            UpdateMovement();
        }

        private Vector3 input;
        private void UpdateMovement()
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");
            var added = new Vector3(horizontal, 0, vertical);

            if (added.Equals(Vector3.zero))
            {
                Kinematic.UpdateSteering(new SteeringOutput(), 0, Time.deltaTime);
                return;
            }
            

            input = added.normalized ;

            var currentPosition = transform.position;
            var newPos = currentPosition + input;

            float minDis = ObstacleManager.Instance.obstacleRadius + 0.5f;
            float minDisSq = minDis * minDis;

            // obstacle avoidance
            bool hitObstacle = false;
            foreach (var obstacle in ObstacleManager.Instance.Obstacles)
            {
                var obstaclePos = obstacle.position;

                float squareDisToObstacle = Common.MathUtility.distancesq(obstaclePos, newPos);

                if (squareDisToObstacle < minDisSq)
                {
                    //newPos = currentPosition;
                    hitObstacle = true;
                    Kinematic.velocity = Vector3.zero;
                    break;
                }
            }

            SteeringOutput seekOutput = new SteeringOutput();
            if (!hitObstacle)
                seekOutput = UpdateLinearSteering(newPos);

            SteeringOutput alignOutput = UpdateAlignmentSteering(input);

            SteeringOutput sumOutput = new SteeringOutput();
            sumOutput += seekOutput * _seekSteerBehaviour.weight;
            sumOutput += alignOutput * lookSteeringBehaviour.weight;
            
            float moveSpeed = speed * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeedModifer : 1);
            
            sumOutput.linear.Normalize();
            if (Mathf.Abs(sumOutput.angular) > maxRotationSpeed)
                sumOutput.angular /= Mathf.Abs(sumOutput.angular) * maxRotationSpeed;
                
            
            Kinematic.UpdateSteering(sumOutput, moveSpeed, Time.deltaTime);
            Kinematic.UpdateTransform();
        }

        private SteeringOutput UpdateLinearSteering(Vector3 newPos)
        {
            return _seekSteerBehaviour.GetSteeringOutput();
        }

        private SteeringOutput UpdateAlignmentSteering(Vector3 input)
        {
            var normInput = input.normalized;

            float targetAngle = Mathf.Atan2(normInput.z, normInput.x);
            float currentAngle = Kinematic.orientation;

            // lookSteeringBehaviour.targetOrientation = targetAngle;
            // lookSteeringBehaviour.characterOrientation = currentAngle;
            // lookSteeringBehaviour.characterRotation = Kinematic.rotationSpeed;
            // lookSteeringBehaviour.characterVelocity = Kinematic.velocity;

            return lookSteeringBehaviour.GetSteeringOutput(); 
        }
    }
}