using System;

namespace Vanilla
{
    public abstract class SteerBehaviour
    {
        public float weight = 1;
        [NonSerialized] public Kinematic character;
        [NonSerialized] public Kinematic target;
        
        // TODO: byt ut alla vector3 targetPos etc mot character.Position
        
        public abstract SteeringOutput GetSteeringOutput();
    }
}