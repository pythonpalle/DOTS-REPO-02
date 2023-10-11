using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Unity.Mathematics;
using UnityEngine;

namespace Vanilla
{
    public class Kinematic : MonoBehaviour
    {
        public Vector3 position;
        public float orientation;
        public Vector3 velocity;
        public float rotationSpeed;

        private void OnEnable()
        {
            position = transform.position;
            orientation = Mathf.Atan2(transform.forward.z, transform.forward.x);
            velocity = Vector3.zero;
            rotationSpeed = 0f;
        }

        public void UpdateSteering(SteeringOutput steering, float maxMoveSpeed, float time)
        {
            position += velocity * time;
            orientation += rotationSpeed * time;
            orientation = MathUtility.MapToRange(orientation);
            
            velocity += steering.linear;
            rotationSpeed += steering.angular;

            if (velocity.magnitude > maxMoveSpeed)
            {
                velocity.Normalize();
                velocity *= maxMoveSpeed;
            }
        }

        public void UpdateTransform()
        {
            transform.position = position;
            transform.forward = new Vector3(Mathf.Cos(orientation), 0, Mathf.Sin(orientation));
        }
    }
}