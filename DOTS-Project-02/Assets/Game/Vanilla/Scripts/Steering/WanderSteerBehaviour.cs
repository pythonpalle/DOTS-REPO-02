using System;
using UnityEngine;
using Random = System.Random;

namespace Vanilla
{
    [System.Serializable]
    public class WanderSteerBehaviour : SteerBehaviour
    {
        public float weight;
        
        [NonSerialized] public Vector3 characterPosition; 
        [NonSerialized] public float characterOrientation;
        [NonSerialized] public Vector3 targetPosition;
        
        [Header("Wander")]
        public float wanderOffset;
        public float wanderRadius;
        public float wanderRate;
        
        public float maxAcceleration;

        public override SteeringOutput GetSteeringOutput()
        {
            float wanderOrientation = RandomBinomial() * wanderRate;

            float targetOrientation = wanderOrientation + characterOrientation;
            
            // center of wander circle
            Vector3 orientationAsVector = new Vector3(Mathf.Cos(characterOrientation), 0, Mathf.Sin(characterOrientation));
            var targetPos = characterPosition + wanderOffset * orientationAsVector;

            // TODO: return right value
            return new SteeringOutput();

        }

        private float RandomBinomial()
        {
            Random random = new Random();
            return random.Next() - random.Next();
        }
    }
}