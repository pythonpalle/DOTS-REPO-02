using Unity.Entities;
using Unity.Mathematics;

namespace DOTS
{
    public struct Kinematic : IComponentData
    {
        public float2 position;
        public float orientation;
        public float2 velocity;
        public float rotationSpeed;
    }
}