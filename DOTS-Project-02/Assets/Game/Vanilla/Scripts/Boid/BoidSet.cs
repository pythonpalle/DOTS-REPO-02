using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vanilla
{
    [CreateAssetMenu(menuName = "BoidSet")]
    public class BoidSet : ScriptableObject
    {
        public List<Boid> Boids = new List<Boid>();
    }

}