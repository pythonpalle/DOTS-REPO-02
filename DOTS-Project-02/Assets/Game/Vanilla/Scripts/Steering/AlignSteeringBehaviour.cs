using System;
using Common;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Vanilla
{
    [System.Serializable]
    public class AlignSteeringBehaviour: ISteerBehaviour
    {
        [NonSerialized] public float characterOrientation;
        [NonSerialized] public float characterRotation;
        [NonSerialized] public float targetOrientation;

        public float maxAngularAcceleration = 1;
        public float maxRotation = 1;
        
        public float targetRadius = 1;
        
        public float slowRadius = 1;
        public float timeToTarget = 0.1f;
        
        public SteeringOutput GetSteeringOutput()
        {
            SteeringOutput steeringOutput = new SteeringOutput();

            // rotational difference to target
            var rotation = targetOrientation - characterOrientation;
            
            // rotation mapped to [-PI, PI]
            rotation = MathUtility.MapToRange(rotation);
            
            // absoulte value of rotation
            var rotationAbsValue = Mathf.Abs(rotation);

            // return no steering if rotation is close enough to target
            if (rotationAbsValue < targetRadius)
            {
                return steeringOutput;
            }

            // use max rotation if outside slow radius. Otherwise, scale it with slow radius
            float targetRotation = rotationAbsValue > slowRadius
                ? maxRotation
                : maxRotation * rotationAbsValue / slowRadius;

            targetRotation *= rotation / rotationAbsValue;

            steeringOutput.angular = (targetRotation - characterRotation) / timeToTarget;

            // TODO: Lägg till characterRotation, targetRotation
            var angularAcceleration = Mathf.Abs(steeringOutput.angular);
            if (angularAcceleration > maxAngularAcceleration)
            {
                steeringOutput.angular /= angularAcceleration;
                steeringOutput.angular *= maxAngularAcceleration;
            }
            
            return steeringOutput;
        }

        
    }
}