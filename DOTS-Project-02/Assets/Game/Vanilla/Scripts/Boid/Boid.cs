
using System;
using UnityEngine;

namespace Vanilla
{
    public class Boid : MonoBehaviour
    {
        public Kinematic Kinematic;

        public void InitializeKinematic()
        {
            var boidTransform = transform;
            Kinematic.position = boidTransform.position;
            Kinematic.orientation = Vector3.SignedAngle(boidTransform.forward, Vector3.right, Vector3.up);
        }
    }
}