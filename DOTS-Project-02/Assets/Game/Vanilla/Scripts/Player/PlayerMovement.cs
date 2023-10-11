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

        void Update()
        {
            UpdateMovement();
        }

        private void UpdateMovement()
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");
            var added = new Vector3(horizontal, 0, vertical);

            if (added.Equals(Vector3.zero))
                return;

            float sprintModifier = Input.GetKey(KeyCode.LeftShift) ? sprintSpeedModifer : 1;
            var input = added.normalized * Time.deltaTime * speed * sprintModifier;

            var currentPosition = transform.position;
            var newPos = currentPosition + input;

            float minDis = ObstacleManager.Instance.obstacleRadius + 1f;
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

            Kinematic.UpdateTransform(newPos, input);
        }
    }
}