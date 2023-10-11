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

            SteeringOutput seekOutput = new SteeringOutput();
            if (newPos != currentPosition)
                seekOutput = UpdateLinearSteering(newPos);

            SteeringOutput alignOutput = UpdateAlignmentSteering(input);

            SteeringOutput sumOutput = new SteeringOutput();
            sumOutput += seekOutput;
            sumOutput += alignOutput;
            
            Debug.Log($"Linear sum: {sumOutput.linear}");
            Debug.Log($"Angular sum: {sumOutput.angular}");
            
            float moveSpeed = speed * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeedModifer : 1);
            Kinematic.UpdateSteering(sumOutput, moveSpeed, Time.deltaTime);
            Kinematic.UpdateTransform();
        }

        private SteeringOutput UpdateLinearSteering(Vector3 newPos)
        {
            _seekSteerBehaviour.characterPosition = transform.position;
            _seekSteerBehaviour.targetPosition = newPos;
            
            return _seekSteerBehaviour.GetSteeringOutput();
        }

        private SteeringOutput UpdateAlignmentSteering(Vector3 input)
        {
            var normInput = input.normalized;

            float targetAngle = Mathf.Atan2(normInput.z, normInput.x);
            float currentAngle = Kinematic.orientation;

            alignSteeringBehaviour.targetOrientation = targetAngle;
            alignSteeringBehaviour.characterOrientation = currentAngle;
            alignSteeringBehaviour.characterRotation = Kinematic.rotationSpeed;
            
            return alignSteeringBehaviour.GetSteeringOutput();
        }
    }
}