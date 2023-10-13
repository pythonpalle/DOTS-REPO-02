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
                    hitObstacle = true;
                    Kinematic.velocity = Vector3.zero;
                    break;
                }
            }

            Kinematic.orientation = MathUtility.DirectionAsFloat(input);
            if (!hitObstacle)
                Kinematic.position = newPos;
            
            UpdateKinematicTransform();
        }
    }
}