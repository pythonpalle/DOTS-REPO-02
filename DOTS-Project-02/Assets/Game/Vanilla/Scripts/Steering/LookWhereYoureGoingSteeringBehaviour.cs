using UnityEngine;

namespace Vanilla
{
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