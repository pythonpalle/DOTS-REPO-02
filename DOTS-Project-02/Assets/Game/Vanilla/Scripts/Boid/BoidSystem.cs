using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vanilla;

public class BoidSystem : MonoBehaviour
{
    public BoidSet BoidSet;
    public List<BoidSpawner> BoidSpawners;

    private void Start()
    {
        BoidSet.Boids.Clear();

        foreach (var spawner in BoidSpawners)
        {
            spawner.SpawnBoids();
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBoids();
    }

    private void UpdateBoids()
    {
        int boidCount = BoidSet.Boids.Count;
        
        if (boidCount == 0)
            return;
    }

    private void OnDestroy()
    {
        BoidSet.Boids.Clear();
    }
}
