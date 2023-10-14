using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Unity.Mathematics;
using UnityEngine;

namespace Vanilla
{
    public class Kinematic 
    {
        public Vector3 position;
        public float orientation;
        public Vector3 velocity;
        public float rotationSpeed;

        public float orientationInDegrees;

        public void UpdateSteering(SteeringOutput steering, float maxMoveSpeed, float time)
        {
            position += velocity * time;
            orientation += rotationSpeed * time;
            orientation = MathUtility.MapToRange(orientation);
            orientationInDegrees = Mathf.Rad2Deg * orientation;
            
            velocity += steering.linear;

            // if (steering.linear.magnitude == 0)
            // {
            //     float slowDownFactor = 0.95f;
            //     velocity *= slowDownFactor;
            // }
            
            rotationSpeed += steering.angular;
            // if (steering.angular == 0)
            // {
            //     float slowDownFactor = 0.95f;
            //     rotationSpeed *= slowDownFactor;
            // }

            if (velocity.magnitude > maxMoveSpeed)
            {
                velocity.Normalize();
                velocity *= maxMoveSpeed;
            }

            // clamp to ground
            position = new Vector3(position.x, 1, position.z);
        }
    }
}