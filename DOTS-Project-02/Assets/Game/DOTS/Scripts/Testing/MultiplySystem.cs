using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;


public partial struct MultiplySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MultiplyConfig>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<MultiplyConfig>(); 
        if (!config.runSystem)
            return;

        int rootCount = config.count;
        float multiplier = config.multiplier;

        NativeArray<float> floatsToMultiply = new NativeArray<float>(rootCount, Allocator.Persistent);
        NativeArray<float> multiplyResults = new NativeArray<float>(rootCount, Allocator.Persistent);

        for (int i = 0; i < rootCount; i++)
        {
            floatsToMultiply[i] = GetRandomNumber();
        }

        if (config.useJobs)
        {
            MultiplyJob multiplyJob = new MultiplyJob
            {
                inArray = floatsToMultiply,
                outArray = multiplyResults,
                multiplier = multiplier
            };

            JobHandle multiplyHandle = multiplyJob.Schedule(rootCount, 64);
            multiplyHandle.Complete();
        }
        else
        {
            for (int i = 0; i < rootCount; i++)
            {
                multiplyResults[i] = floatsToMultiply[i] * multiplier;
            }
        }

        floatsToMultiply.Dispose();
        multiplyResults.Dispose();
    }

    private float GetRandomNumber()
    {
        return Random.value;
    }

    [BurstCompile]
    private partial struct MultiplyJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> inArray;
        [WriteOnly]public NativeArray<float> outArray;
        [ReadOnly]public float multiplier;

        public void Execute(int index)
        {
            outArray[index] = inArray[index] * multiplier;
        }
    }
}
