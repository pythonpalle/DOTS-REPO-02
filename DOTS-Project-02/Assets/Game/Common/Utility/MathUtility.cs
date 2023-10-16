using System.Collections;
using System.Collections.Generic;
using DOTS;
using Unity.Assertions;
using Unity.Mathematics;
using UnityEngine;

namespace Common
{
    public static class MathUtility
    {
        public const float PI = 3.141593f;
        
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

        public static float MapToRangeMinusPiToPi(float rotation)
        {
            float mappedRotation = PositiveMod(rotation, 2*PI);
            if (mappedRotation > PI)
            {
                mappedRotation -= 2*PI;
            }

            Assert.IsTrue(-PI <= mappedRotation && mappedRotation <= PI);
            return  mappedRotation;
        }
        
        public static float MapToRange0To2Pie(float rotation)
        {
            float mappedRotation = PositiveMod(rotation, 2*PI);
            Assert.IsTrue(0 <= mappedRotation && mappedRotation <= 2*PI);
            return  mappedRotation;
        }
        
        public static float PositiveMod(float a, float b)
        {
            return ((a % b)+b)%b;
        }

        public static float DirectionAsFloat(Vector3 direction)
        {
            return Atan2(direction.z, direction.x); 
            return Mathf.Deg2Rad * Vector3.SignedAngle(direction, Vector3.right, Vector3.up);
        }

        public static Vector3 AngleRotationAsVector(float angleRotation)
        {
            return new Vector3(Mathf.Cos(angleRotation), 0, Mathf.Sin(angleRotation)).normalized;
        }

        public static float DirectionToFloat(float3 direction)
        {
            return Atan2(direction.z, direction.x);
        }
        
        public static float DirectionToFloat(float2 direction)
        {
            return Atan2(direction.y, direction.x);
        }

        static float Atan2(float y, float x)
        {
            return math.atan2(y, x);
        }

        public static float3 Float2ToFloat3(float2 float2)
        {
            return new float3(float2.x, 0, float2.y);
        }
    }
}
