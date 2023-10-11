using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vanilla
{
    public class BoidSpawnManager : MonoBehaviour
    {
        public BoidSet BoidSet;
        public List<BoidSpawner> BoidSpawners;
        public Transform BoidParent;

        private void OnEnable()
        {
            BoidSet.Boids.Clear();

            foreach (var spawner in BoidSpawners)
            {
                spawner.SpawnBoids(BoidParent);
            }
        }

        private void OnDestroy()
        {
            BoidSet.Boids.Clear();
        }
    }
}
