using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boids/RecordCommunicator")]
public class BoidCommunicator : ScriptableObject
{
    public BoidProfilerName BoidProfilerName;
    public int boidCount;
    public float minSpawnRadius;
    public float maxSpawnRadius;
}
