using System;
using UnityEngine;
using Random = System.Random;

namespace Vanilla
{
    [System.Serializable]
    public class WanderSteerBehaviour : FaceSteeringBehaviour
    {
        [Header("Wander")]
        public float wanderOffset;
        public float wanderRadius;
        public float wanderRate;
        
        public float maxAcceleration;

        public override SteeringOutput GetSteeringOutput()
        {
            float wanderOrientation = RandomBinomial() * wanderRate;

            float targetOrientation = wanderOrientation + character.orientation;
            
            // center of wander circle
            Vector3 characterOrientationAsVector = new Vector3(Mathf.Cos(character.orientation), 0, Mathf.Sin(character.orientation));
            Vector3 targetOrientationAsVector = new Vector3(Mathf.Cos(target.orientation), 0, Mathf.Sin(target.orientation));
            
            var wanderTargetPosition = character.position + wanderOffset * characterOrientationAsVector;
            wanderTargetPosition += wanderRadius * targetOrientationAsVector;
           
            target.position = wanderTargetPosition;
            target.orientation = targetOrientation;
            
            var result = base.GetSteeringOutput();
            result.linear = maxAcceleration * characterOrientationAsVector;
            return result;

        }

        private float RandomBinomial()
        {
            Random random = new Random();
            return random.Next() - random.Next();
        }
    }
}