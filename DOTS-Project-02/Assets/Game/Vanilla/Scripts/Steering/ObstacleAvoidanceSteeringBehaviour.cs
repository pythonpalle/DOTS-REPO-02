using Common;
using UnityEngine;

namespace Vanilla
{
    [System.Serializable]
    public class ObstacleAvoidanceSteeringBehaviour : SeekSteerBehaviour
    {
        public float avoidDistance;
        
        public override SteeringOutput GetSteeringOutput()
        {
            var steeringOutput = new SteeringOutput();
            
            float distanceToObstacle = MathUtility.distancesq(character.position, target.position);
            if (distanceToObstacle < avoidDistance)
            {
                Vector3 directionToObstacle = target.position - character.position;
                target = new Kinematic
                {
                    position = character.position - directionToObstacle
                };
                steeringOutput = base.GetSteeringOutput();
            }

            return steeringOutput;
        }
    }
}