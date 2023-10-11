using System;
using UnityEngine;

namespace Vanilla
{
    [System.Serializable]
    public struct SeekSteerBehaviour : ISteerBehaviour
    {
        public float weight;
        public float maxVisionDistance;
        [NonSerialized] public Vector3 characterPosition; 
        [NonSerialized] public Vector3 targetPosition;

        public float maxAcceleration;
        public SteeringOutput GetSteeringOutput()
        {
            SteeringOutput output = new SteeringOutput();
            var direction = (targetPosition - characterPosition).normalized;

            output.linear = direction * maxAcceleration;
            return output;
        } 
    }
}