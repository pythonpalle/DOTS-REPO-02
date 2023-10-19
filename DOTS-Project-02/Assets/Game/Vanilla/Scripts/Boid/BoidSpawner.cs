using UnityEngine;


namespace Vanilla
{
    public class BoidSpawner : MonoBehaviour
    {
        public Boid boidPrefab;
        public BoidCommunicator BoidCommunicator;
        public BoidSet BoidSet;
        
        System.Random random = new System.Random();


        public void SpawnBoids(Transform boidParent)
        {
            for (int i = 0; i < BoidCommunicator.boidCount; i++)
            {
                SpawnBoid(boidParent);
            }
        }

        private void SpawnBoid(Transform boidParent)
        {
            Vector3 positionOffsetDirection = new Vector3
            {
                x = 1 - 2 * (float) random.NextDouble(),
                y = 0,
                z = 1 - 2 * (float) random.NextDouble()
            }.normalized;
            
            float radiusDif = BoidCommunicator.minSpawnRadius - BoidCommunicator.maxSpawnRadius;
            float randomOffset = radiusDif + (float)random.NextDouble() * radiusDif;
            var position = transform.position + positionOffsetDirection * randomOffset;
            position = new Vector3(position.x, 1f, position.z);
            
            Vector3 rotationVector = new Vector3
            {
                x = 1 - 2 * (float) random.NextDouble(),
                y = 0,
                z = 1 - 2 * (float) random.NextDouble()
            }.normalized;
            var rotation = Quaternion.LookRotation(rotationVector, Vector3.up);

            Boid boidInstance = Instantiate(boidPrefab, boidParent);
            var boidTransform = boidInstance.transform;
            boidTransform.position = position;
            boidTransform.rotation = rotation;
            
            BoidSet.Boids.Add(boidInstance);
        }
    }

}