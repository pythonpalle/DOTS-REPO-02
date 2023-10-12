using System;
using Common;
using UnityEngine;

namespace Vanilla
{
    [System.Serializable]
    public class FaceSteeringBehaviour: AlignSteeringBehaviour
    {
        public override SteeringOutput GetSteeringOutput()
        {
            Vector3 direction = target.position - character.position;
            target.orientation = Mathf.Atan2(direction.z, direction.x);
            return base.GetSteeringOutput();
        }
    }
}