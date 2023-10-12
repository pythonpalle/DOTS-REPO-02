using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Vanilla
{
    [System.Serializable]
    public class SeparationSteerBehaviour : SteerBehaviour
    {
        [NonSerialized] public List<Kinematic> neighbours;

        public float threshold;
        public float decayCoefficient;
        public float maxAcceleration;
        
        public override SteeringOutput GetSteeringOutput()
        {
            var result = new SteeringOutput();

            foreach (var neighbour in neighbours)
            {
                var direction = neighbour.position - character.position;
                float distanceToTarget = direction.magnitude;

                if (distanceToTarget < threshold)
                {
                    float strength = Mathf.Min(decayCoefficient / distanceToTarget*distanceToTarget, maxAcceleration);
                    direction.Normalize();
                    result.linear -= strength * direction;
                }
            }

            return result;
        }
    }
}