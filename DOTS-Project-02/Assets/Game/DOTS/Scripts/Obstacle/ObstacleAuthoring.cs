using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.Entities;
using UnityEngine;

namespace DOTS
{
    public class ObstacleAuthoring : MonoBehaviour
    {

        class Baker : Baker<ObstacleAuthoring>
        {
            public override void Bake(ObstacleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Obstacle { });
                AddComponent(entity, new WrapComponent() { });
            }
        }
    }

    public struct Obstacle : IComponentData
    {
    }
}
