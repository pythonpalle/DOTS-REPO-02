using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Vanilla
{
    public class PlayerMovement : KinematicBehaviour
    {
        
        [Header("Movement Variables")]
        public float sprintSpeedModifer = 2f;
        public float speed = 5f;
        public float maxRotationSpeed = 1f;

        // [Header("Steering Behaviours")]
        // [SerializeField] private SeekSteerBehaviour _seekSteerBehaviour = new SeekSteerBehaviour();
        // [SerializeField] private LookWhereYoureGoingSteeringBehaviour lookSteeringBehaviour = new LookWhereYoureGoingSteeringBehaviour();

        private void Start()
        {
            // _seekSteerBehaviour.character = Kinematic;
            // lookSteeringBehaviour.character = Kinematic;
            //
            // _seekSteerBehaviour.target = new Kinematic();
            // lookSteeringBehaviour.target = new Kinematic();
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

            // if (added.Equals(Vector3.zero))
            // {
            //     Kinematic.UpdateSteering(new SteeringOutput(), 0, Time.deltaTime);
            //     return;
            // }
            
            input = added.normalized ;

            var currentPosition = transform.position;
            float moveSpeed = speed * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeedModifer : 1); 
            var newPos = currentPosition + input * moveSpeed * Time.deltaTime;

            float minDis = ObstacleManager.Instance.obstacleRadius + 0.5f;
            float minDisSq = minDis * minDis;

            // obstacle avoidance
            bool hitObstacle = false;
            foreach (var obstacle in ObstacleManager.Instance.ObstacleKinematics)
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

            Kinematic.orientation = MathUtility.DirectionAsFloat(input);
            if (!hitObstacle)
                Kinematic.position = newPos;
            
            UpdateKinematicTransform();

            // SteeringOutput seekOutput = new SteeringOutput();
            // if (!hitObstacle)
            //     seekOutput = UpdateLinearSteering(newPos);
            //
            // SteeringOutput alignOutput = UpdateAlignmentSteering(input);
            //
            // SteeringOutput sumOutput = new SteeringOutput();
            // sumOutput += seekOutput * _seekSteerBehaviour.weight;
            // sumOutput += alignOutput * lookSteeringBehaviour.weight;


            // sumOutput.linear.Normalize();
            // if (Mathf.Abs(sumOutput.angular) > maxRotationSpeed)
            //     sumOutput.angular /= Mathf.Abs(sumOutput.angular) * maxRotationSpeed;
            //     
            // float moveSpeed = speed * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeedModifer : 1);
            // Kinematic.UpdateSteering(sumOutput, moveSpeed, Time.deltaTime);
            // UpdateKinematicTransform();
        }

        // private SteeringOutput UpdateLinearSteering(Vector3 newPos)
        // {
        //     _seekSteerBehaviour.target.position = newPos;
        //     return _seekSteerBehaviour.GetSteeringOutput();
        // }
        //
        // private SteeringOutput UpdateAlignmentSteering(Vector3 input)
        // {
        //     float targetAngle = Mathf.Atan2(input.z, input.x);
        //     lookSteeringBehaviour.target.orientation = targetAngle;
        //     lookSteeringBehaviour.target.rotationSpeed = Kinematic.rotationSpeed;
        //
        //     return lookSteeringBehaviour.GetSteeringOutput(); 
        // }
    }
}