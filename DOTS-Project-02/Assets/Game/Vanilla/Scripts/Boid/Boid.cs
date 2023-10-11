
using System;
using UnityEngine;

namespace Vanilla
{
    public class Boid : MonoBehaviour
    {
        public Kinematic Kinematic;

        public void InitializeKinematic()
        {
            Kinematic.position = transform.position;
            Kinematic.orientation = Vector3.SignedAngle(transform.forward, Vector3.right, Vector3.up);
        }
    }
}