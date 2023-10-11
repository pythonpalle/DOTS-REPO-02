using Unity.Entities;
using Unity.Mathematics;

public struct Kinematic : IComponentData
{
    public float2 position;
    public float orientation;
    public float2 velocity;
    public float rotationSpeed;
}