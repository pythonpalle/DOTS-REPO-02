using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class MultiplyAuthoring : MonoBehaviour
{
    [Header("Settings")] 
    public bool runSystem;
    public bool useJobs;

    [Header("Input info")]
    public int count = 1;
    public float multiplier = 2;
    
    [Header("Output info")]
    public float inputSum;
    public float multiplySum;
    

    class Baker : Baker<MultiplyAuthoring>
    {

        public override void Bake(MultiplyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new MultiplyConfig
            {
                runSystem = authoring.runSystem,
                useJobs = authoring.useJobs,

                count = authoring.count,
                multiplier =  authoring.multiplier,
                inputSum =  authoring.inputSum,
                multiplySum =  authoring.multiplySum,
            });
        }
    }
}

public struct MultiplyConfig : IComponentData
{
    public int count;
    public bool useJobs;
    
    public float multiplier;
    
    public float inputSum;
    public float multiplySum;
    public bool runSystem;
}
