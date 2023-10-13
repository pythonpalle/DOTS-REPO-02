using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public static class MathUtility
    {
        public static float PI = Mathf.PI;
        
        public static float distancesq(Vector2 a, Vector2 b)
        {
            var dx = b.x - a.x;
            var dy = b.y - a.y;

            return dx * dx + dy * dy;
        }

        public static float distancesq(Vector3 a, Vector3 b)
        {
            var dx = b.x - a.x;
            var dz = b.z - a.z;

            return dx * dx + dz * dz;
        }

        public static float MapToRange(float rotation)
        {
            return  (rotation + PI) % (2 * PI) - PI;
        }

        public static float DirectionAsFloat(Vector3 direction)
        {
            return Mathf.Deg2Rad * Vector3.SignedAngle(direction, Vector3.right, Vector3.up);
        }

        public static Vector3 AngleRotationAsVector(float angleRotation)
        {
            return new Vector3(Mathf.Cos(angleRotation), 0, Mathf.Sin(angleRotation)).normalized;
        }
    }
}
