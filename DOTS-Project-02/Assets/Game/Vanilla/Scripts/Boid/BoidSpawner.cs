using UnityEngine;


namespace Vanilla
{
    public class BoidSpawner : MonoBehaviour
    {
        public Boid boidPrefab;
        public int spawnCount;
        public float initialRadius;
        public BoidSet BoidSet;
        
        System.Random random = new System.Random();


        public void SpawnBoids(Transform boidParent)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnBoid(boidParent);
            }
        }

        private void SpawnBoid(Transform boidParent)
        {
            Vector3 direction = new Vector3
            {
                x = 1 - 2 * (float) random.NextDouble(),
                y = 0,
                z = 1 - 2 * (float) random.NextDouble()
            }.normalized;
            
            Boid boidInstance = Instantiate(boidPrefab, boidParent);

            float randomOffset = (float)random.NextDouble() * initialRadius;
            var position = transform.position + direction * randomOffset;
            position = new Vector3(position.x, 1f, position.z);
            var rotation = Quaternion.LookRotation(direction, Vector3.up);

            var boidTransform = boidInstance.transform;
            boidTransform.position = position;
            boidTransform.rotation = rotation;
            
            BoidSet.Boids.Add(boidInstance);
        }
    }

}