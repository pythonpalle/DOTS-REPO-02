﻿using System;
using Common;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Vanilla
{
    [System.Serializable]
    public class AlignSteeringBehaviour: SteerBehaviour
    {
        public float maxAngularAcceleration ;
        public float maxRotation ;
        
        public float targetRadius ;
        
        public float slowRadius ;
        public float timeToTarget ;
        
        public override SteeringOutput GetSteeringOutput()
        {
            SteeringOutput steeringOutput = new SteeringOutput();

            // rotational difference to target
            var rotation = target.orientation - character.orientation;
            
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

            steeringOutput.angular = (targetRotation - character.rotationSpeed) / timeToTarget;

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