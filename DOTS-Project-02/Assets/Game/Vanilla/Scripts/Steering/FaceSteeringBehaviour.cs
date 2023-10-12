using System;
using Common;
using UnityEngine;

namespace Vanilla
{
    [System.Serializable]
    public class FaceSteeringBehaviour: AlignSteeringBehaviour
    {
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
        public override SteeringOutput GetSteeringOutput()
        {
            if (character.velocity == Vector3.zero)
                return new SteeringOutput();
            
            target.orientation = Mathf.Atan2(character.velocity.z, character.velocity.x);
            return base.GetSteeringOutput();
        }
    }
}