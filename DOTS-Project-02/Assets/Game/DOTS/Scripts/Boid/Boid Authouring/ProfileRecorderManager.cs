using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;

namespace DOTS
{
    public class ProfileRecorderManager : MonoBehaviour
    {
        string statsText;
        // ProfilerRecorder systemMemoryRecorder;
        // ProfilerRecorder gcMemoryRecorder;
        // ProfilerRecorder mainThreadTimeRecorder;
        ProfilerRecorder boidRecorder;

        static double GetRecorderFrameAverage(ProfilerRecorder recorder)
        {
            var samplesCount = recorder.Capacity;
            if (samplesCount == 0)
                return 0;

            double r = 0;
            
            var samples = new List<ProfilerRecorderSample>(samplesCount);
            recorder.CopyTo(samples);

            if (samplesCount > 0)
            {
                for (var i = 0; i < samples.Count; ++i)
                {
                    r += samples[i].Value;
                }
                r /= samplesCount;
            }

            return r;
        }

        void OnEnable()
        {
            // systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            // gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            // mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
            
            boidRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, "Boids", 1_000);
        }

        void OnDisable()
        {
            // systemMemoryRecorder.Dispose();
            // gcMemoryRecorder.Dispose();
            // mainThreadTimeRecorder.Dispose();
            boidRecorder.Dispose();
        }

        void Update()
        {
            var sb = new StringBuilder(500);
            // sb.AppendLine($"Frame Time: {GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f):F1} ms");
            // sb.AppendLine($"GC Memory: {gcMemoryRecorder.LastValue / (1024 * 1024)} MB");
            // sb.AppendLine($"System Memory: {systemMemoryRecorder.LastValue / (1024 * 1024)} MB");
            var samplesCount = boidRecorder.Count;
            sb.AppendLine($"Boid time: {GetRecorderFrameAverage(boidRecorder) * (1e-6f):F1} ms ms");
            sb.AppendLine($"Samples used: {samplesCount}");
            statsText = sb.ToString();
        }

        void OnGUI()
        {
            GUI.TextArea(new Rect(10, 30, 250, 50), statsText);
        }
    }
}