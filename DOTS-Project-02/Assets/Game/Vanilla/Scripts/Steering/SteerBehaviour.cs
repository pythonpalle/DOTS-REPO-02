namespace Vanilla
{
    public abstract class SteerBehaviour
    {
        public float weight = 1;
        
        public abstract SteeringOutput GetSteeringOutput();
    }
}