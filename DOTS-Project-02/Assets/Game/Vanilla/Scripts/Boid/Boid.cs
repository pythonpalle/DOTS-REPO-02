
using System;
using Common;
using UnityEngine;

namespace Vanilla
{
    public class Boid : KinematicBehaviour
    {
        private void OnDrawGizmosSelected()
        {
            float orientation = Kinematic.orientation;
            float vectorLength = 5;
            var forward = MathUtility.AngleRotationAsVector(orientation) * vectorLength;
            var position = Kinematic.position;

            float fov = 120;
            float halfFov = fov*0.5f;
            float halfFovInRadians = Mathf.Deg2Rad * halfFov;

            int numberOfExtraLines = 3;
            for (int i = 1; i <= numberOfExtraLines; i++)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    float rotation = (float)i / numberOfExtraLines * halfFovInRadians*j + orientation;
                    var vector = MathUtility.AngleRotationAsVector(rotation) * vectorLength;
                    Debug.DrawLine(position, position+vector);
                }
            }

            Debug.DrawLine(position, position+forward, Color.red);
        }
    }
}