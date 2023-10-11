using System.Collections;
using System.Collections.Generic;
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

        public void UpdateSteeringVariables(SteeringOutput steering, float maxSpeed, float time)
        {
            position += velocity * time;
            orientation += rotationSpeed * time;

            velocity += steering.linear;
            rotationSpeed += steering.angular;

            if (velocity.magnitude > maxSpeed)
            {
                velocity.Normalize();
                velocity *= maxSpeed;
            }
        }

        public void UpdateTransform()
        {
            transform.position = position;
            transform.forward = new Vector3(Mathf.Cos(orientation), 0, Mathf.Sin(orientation));
        }
        
        public void UpdateTransform(Vector3 position, float orienation)
        {
            this.position = position;
            this.orientation = orienation;
            UpdateTransform();
        }
        
        public void UpdateTransform(Vector3 position, Vector3 direction)
        {
            this.position = position;
            this.orientation = Mathf.Atan2(direction.z, direction.x);
            UpdateTransform();
        }
    }
}