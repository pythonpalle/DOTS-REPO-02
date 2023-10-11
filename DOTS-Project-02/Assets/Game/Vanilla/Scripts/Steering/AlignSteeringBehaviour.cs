using System;
using UnityEngine;

namespace Vanilla
{
    [System.Serializable]
    public class AlignSteeringBehaviour: ISteerBehaviour
    {
        [NonSerialized] public float characterOrientation;
        [NonSerialized] public float targetOrientation;

        public float maxAngularAcceleration = 1;
        public float maxRotation = 1;
        
        public float targetRadius = 1;
        
        public float slowRadius = 1;
        public float timeToTarget = 0.1f;
        
        public SteeringOutput GetSteeringOutput()
        {
            SteeringOutput steeringOutput = new SteeringOutput();

            var rotation = targetOrientation - characterOrientation;

            rotation = MapToRange(rotation);
            var rotationAbsValue = Mathf.Abs(rotation);

            if (rotationAbsValue < targetRadius)
            {
                return steeringOutput;
            }

            float targetRotation = 0;
            if (rotationAbsValue > slowRadius)
            {
                targetRotation = maxRotation;
            }
            else
            {
                targetRotation = maxRotation * rotationAbsValue / slowRadius;
            }

            targetRotation *= rotation / rotationAbsValue;

            steeringOutput.angular = (targetRotation - characterOrientation) / timeToTarget;

            var angularAcceleration = Mathf.Abs(steeringOutput.angular);
            if (angularAcceleration > maxAngularAcceleration)
            {
                steeringOutput.angular /= angularAcceleration;
                steeringOutput.angular *= maxAngularAcceleration;
            }
            
            steeringOutput.linear = Vector3.zero;

            return steeringOutput;
        }

        private float MapToRange(float rotation)
        {
            if (rotation > Mathf.PI)
            {
                rotation -= 2 * Mathf.PI;
            }else if (rotation < -Mathf.PI)
            {
                rotation += 2 * Mathf.PI;
            }

            return rotation;
        }
    }
}