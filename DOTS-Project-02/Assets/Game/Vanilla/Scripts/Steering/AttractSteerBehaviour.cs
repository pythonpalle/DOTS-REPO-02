using Common;

namespace Vanilla
{
    [System.Serializable]
    public class AttractSteerBehaviour : SteerBehaviour
    {
        public float maxAcceleration;
        public float innerRadius;
        public float outerRadius;
        float innerRadiusSquared => innerRadius*innerRadius;
        float outerRadiusSquared => outerRadius*outerRadius;

        public override SteeringOutput GetSteeringOutput()
        {
            float distanceSquared = MathUtility.distancesq(target.position, character.position);
            if (distanceSquared < innerRadiusSquared)
                return new SteeringOutput();

            SteeringOutput output = new SteeringOutput();
            var direction = (target.position - character.position).normalized;


            float acceleration = maxAcceleration * (distanceSquared - innerRadiusSquared) / (outerRadiusSquared - innerRadiusSquared);
            
            output.linear = direction * acceleration;
            output.angular = 0;
            return output;
        } 
    }
}