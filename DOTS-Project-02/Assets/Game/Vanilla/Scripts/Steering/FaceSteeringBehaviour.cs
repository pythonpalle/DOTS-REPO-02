using System;
using Common;
using UnityEngine;

namespace Vanilla
{
    [System.Serializable]
    public class FaceSteeringBehaviour: AlignSteeringBehaviour
    {
        // [NonSerialized] public Vector3 targetPosition;
        // [NonSerialized] public Vector3 characterPosition;
        
        public override SteeringOutput GetSteeringOutput()
        {
            Vector3 direction = target.position - character.position;
            target.orientation = Mathf.Atan2(direction.x, direction.z);
            return base.GetSteeringOutput();
        }
    }
    
    [System.Serializable]
    public class LookWhereYoureGoingSteeringBehaviour: AlignSteeringBehaviour
    {
        [NonSerialized]  public Vector3 characterVelocity;
        public override SteeringOutput GetSteeringOutput()
        {
            if (characterVelocity == Vector3.zero)
                return new SteeringOutput();
            
            target.orientation = Mathf.Atan2(characterVelocity.z, characterVelocity.x);
            return base.GetSteeringOutput();
        }
    }
}