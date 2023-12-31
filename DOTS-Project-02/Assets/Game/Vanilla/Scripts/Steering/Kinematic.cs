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
            orientation = MathUtility.MapToRange0To2Pie(orientation);
            orientationInDegrees = Mathf.Rad2Deg * orientation;
            
            velocity += steering.linear;
            rotationSpeed += steering.angular;

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