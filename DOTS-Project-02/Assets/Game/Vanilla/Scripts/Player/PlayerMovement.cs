using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vanilla
{
    public class PlayerMovement : MonoBehaviour
    {
        public Kinematic Kinematic;
        public float sprintSpeedModifer = 2f;
        public float speed = 5f;

        [SerializeField] private SeekSteerBehaviour _seekSteerBehaviour = new SeekSteerBehaviour();
        [SerializeField] private AlignSteeringBehaviour alignSteeringBehaviour = new AlignSteeringBehaviour();

        void Update()
        {
            UpdateMovement();
        }

        // private void OnDrawGizmos()
        // {
        //     Gizmos.DrawLine(transform.position,transform.position+input );
        //     Gizmos.DrawLine(transform.position,transform.forward );
        // }

        private Vector3 input;
        private void UpdateMovement()
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");
            var added = new Vector3(horizontal, 0, vertical);

            if (added.Equals(Vector3.zero))
            {
                Kinematic.velocity = Vector3.zero;
                Kinematic.rotationSpeed = 0f;
                
                return;
            }
            

            input = added.normalized ;

            var currentPosition = transform.position;
            var newPos = currentPosition + input;

            float minDis = ObstacleManager.Instance.obstacleRadius + 0.5f;
            float minDisSq = minDis * minDis;

            // obstacle avoidance
            foreach (var obstacle in ObstacleManager.Instance.Obstacles)
            {
                var obstaclePos = obstacle.position;

                float squareDisToObstacle = Common.MathUtility.distancesq(obstaclePos, newPos);

                if (squareDisToObstacle < minDisSq)
                {
                    newPos = currentPosition;
                    break;
                }
            }

            if (newPos != currentPosition)
                HandleLinearMovement(newPos);

            HandleRotationalMovement(input);
        }

        private void HandleLinearMovement(Vector3 newPos)
        {
            _seekSteerBehaviour.characterPosition = transform.position;
            _seekSteerBehaviour.targetPosition = newPos;

            float moveSpeed = speed * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeedModifer : 1);

            Kinematic.UpdateSteering(_seekSteerBehaviour.GetSteeringOutput(), moveSpeed, Time.deltaTime);
            Kinematic.UpdateTransform();
        }

        private void HandleRotationalMovement(Vector3 input)
        {
            var normInput = input.normalized;

            float targetAngle = Mathf.Atan2(normInput.z, normInput.x);
            float currentAngle = Kinematic.orientation;

            alignSteeringBehaviour.targetOrientation = targetAngle;
            alignSteeringBehaviour.characterOrientation = currentAngle;
            
            Kinematic.UpdateSteering(alignSteeringBehaviour.GetSteeringOutput(), speed, Time.deltaTime);
            Kinematic.UpdateTransform();
        }
    }
}