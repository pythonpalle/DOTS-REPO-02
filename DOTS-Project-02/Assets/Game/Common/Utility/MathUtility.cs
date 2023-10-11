using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public static class MathUtility
    {
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
    }
}
