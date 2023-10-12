using Common;
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
            
            Vector3 characterOrientationAsVector = MathUtility.AngleRotationAsVector(character.orientation);
            Vector3 targetOrientationAsVector = MathUtility.AngleRotationAsVector(targetOrientation);
            
            // center of wander circle
            var wanderTargetPosition = character.position + wanderOffset * characterOrientationAsVector;
            wanderTargetPosition += wanderRadius * targetOrientationAsVector;
            
            Debug.DrawLine(character.position,wanderTargetPosition);
           
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