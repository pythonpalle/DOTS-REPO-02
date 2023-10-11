﻿using UnityEngine;

namespace Vanilla
{
    public struct SteeringOutput
    {
        public Vector3 linear;
        public float angular;

        public static SteeringOutput operator +(SteeringOutput a, SteeringOutput b)
        {
            SteeringOutput output = new SteeringOutput
            {
                angular = a.angular + b.angular,
                linear = a.linear + b.linear
            };

            return output;
        } 
    }
}