using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using Unity.Profiling;
using UnityEngine;

namespace DOTS
{
    
    
    // public static class TimeKeeper
    // {
    //     private static long before;
    //     private static long after;
    //     private static long difference;
    //     public static void SetBefore(long time)
    //     {
    //         before = time;
    //     }
    //     
    //     public static void SetAfter(long time)
    //     {
    //         after = time;
    //
    //         difference = after - before;
    //
    //         float ms = (float) difference / 10_000;
    //         Debug.Log($"Difference: {difference}");
    //         Debug.Log($"Elapsed time in ms: {ms}");
    //     }
    // }
    //
    // [UpdateAfter(typeof(BoidSpawnSystem))]
    // [UpdateAfter(typeof(PlayerSpawnerSystem))]
    // [UpdateBefore(typeof(BoidMoveSystemWithJobs))]
    // [UpdateBefore(typeof(BoidMoveSystemWithoutJobs))]
    // public partial struct TimeMeasureBefore : ISystem
    // {
    //     public void OnUpdate(ref SystemState state)
    //     {
    //         TimeKeeper.SetBefore(DateTime.Now.Ticks);
    //     }
    // }
    //
    // [UpdateAfter(typeof(BoidSpawnSystem))]
    // [UpdateAfter(typeof(PlayerSpawnerSystem))]
    // [UpdateAfter(typeof(BoidMoveSystemWithJobs))]
    // [UpdateAfter(typeof(BoidMoveSystemWithoutJobs))]
    // public partial struct TimeMeasureAfter : ISystem
    // {
    //     public void OnUpdate(ref SystemState state)
    //     {
    //         TimeKeeper.SetAfter(DateTime.Now.Ticks);
    //     }
    // }
}