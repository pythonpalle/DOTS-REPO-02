using System;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


namespace Vanilla
{
    public class BoidSpawner : MonoBehaviour
    {
        public Boid boidPrefab;
        public int spawnCount;
        public float initialRadius;
        public BoidSet BoidSet;

        public void SpawnBoids()
        {
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnBoid(i);
            }
        }

        private void SpawnBoid(int i)
        {
            var random = new System.Random();
            Vector3 direction = new Vector3
            {
                x = 1 - 2 * (float) random.NextDouble(),
                y = 0,
                z = 1 - 2 * (float) random.NextDouble()
            }.normalized;

            float randomOffset = (float)random.NextDouble() * initialRadius;
            var position = transform.position + direction * randomOffset;
            var boidInstance = Instantiate(boidPrefab, position, quaternion.LookRotation(direction, Vector3.up));
           BoidSet.Boids.Add(boidInstance);
        }
    }

}