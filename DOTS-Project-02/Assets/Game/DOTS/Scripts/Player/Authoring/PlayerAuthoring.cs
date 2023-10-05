using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PlayerComponent>(entity);
            AddComponent<PlayerMovement>(entity);
            AddComponent<BoidTarget>(entity);
        }
    }

    
}

public struct PlayerComponent : IComponentData
{
}
    
public struct PlayerMovement : IComponentData
{
}

public struct BoidTarget : IComponentData
{
}
