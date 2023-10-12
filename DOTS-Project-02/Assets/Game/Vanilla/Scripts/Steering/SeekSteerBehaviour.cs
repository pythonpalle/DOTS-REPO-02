using System;
using Common;
using UnityEngine;

namespace Vanilla
{
    [System.Serializable]
    public class SeekSteerBehaviour : SteerBehaviour
    {
        public float maxVisionDistance;

        public float maxAcceleration;
        public override SteeringOutput GetSteeringOutput()
        {
            SteeringOutput output = new SteeringOutput();
            var direction = (target.position - character.position).normalized;

            output.linear = direction * maxAcceleration;
            output.angular = 0;
            return output;
        } 
    }
}