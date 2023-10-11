using System;
using UnityEngine;

namespace Vanilla
{
    [System.Serializable]
    public class SeekSteerBehaviour : ISteerBehaviour
    {
        [NonSerialized] public Vector3 characterPosition; 
        [NonSerialized] public Vector3 targetPosition;

        public float maxAcceleration = 1;
        public SteeringOutput GetSteeringOutput()
        {
            SteeringOutput output = new SteeringOutput();
            var direction = (targetPosition - characterPosition).normalized;

            output.linear = direction * maxAcceleration;
            return output;
        } 
    }
}