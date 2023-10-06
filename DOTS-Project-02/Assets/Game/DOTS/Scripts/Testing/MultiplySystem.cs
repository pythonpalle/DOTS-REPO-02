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

        NativeArray<float> multiplyResults = new NativeArray<float>(rootCount, Allocator.Persistent);

        

        if (config.useJobs)
        {
            MultiplyJob multiplyJob = new MultiplyJob
            {
                outArray = multiplyResults,
                multiplier = multiplier
            };

            JobHandle multiplyHandle = multiplyJob.Schedule(rootCount, 1000);
            multiplyHandle.Complete();
        }
        else
        {
            for (int i = 0; i < rootCount; i++)
            {
                multiplyResults[i] = 10 * multiplier;
            }
        }

        multiplyResults.Dispose();
    }

    private static  float GetRandomNumber()
    {
        return Random.value;
    }

    [BurstCompile]
    private partial struct MultiplyJob : IJobParallelFor
    {
        [WriteOnly]public NativeArray<float> outArray;
        [ReadOnly]public float multiplier;

        public void Execute(int index)
        {
            outArray[index] = 10 * multiplier;
        }
    }
}
