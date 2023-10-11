using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vanilla;

public class BoidSystem : MonoBehaviour
{
    public BoidSet BoidSet;

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
}
