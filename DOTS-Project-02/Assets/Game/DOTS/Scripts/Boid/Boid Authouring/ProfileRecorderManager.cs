using System;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public enum BoidProfilerName
{
    Vanilla,
    DotsWithoutJobs,
    DotsWithJobs
}

namespace Common
{
    public class ProfileRecorderManager : MonoBehaviour
    {
        [SerializeField] private int samplesToRecordCount;
        [SerializeField] private BoidProfilerName _profilerName;

        string statsText;
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
            boidRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, _profilerName.ToString(), samplesToRecordCount);
        }

        void OnDisable()
        {
            boidRecorder.Dispose();
        }

        void Update()
        {
            var sb = new StringBuilder(500);
            var samplesCount = boidRecorder.Count;
            sb.AppendLine($"Samples used: {samplesCount}");

            string recordedData = string.Empty;
            if (samplesCount >= samplesToRecordCount)
            {
                float frameAverage = (float)GetRecorderFrameAverage(boidRecorder) * (1e-6f);
                var frameAverageAsString = $"{frameAverage:F2}";
                recordedData = $"Time for {_profilerName.ToString()}: {frameAverageAsString} ms";
                Debug.Log(recordedData);
            }
            
            sb.AppendLine(recordedData);

            statsText = sb.ToString();
        }

        void OnGUI()
        {
            GUI.TextArea(new Rect(10, 30, 250, 50), statsText);
        }
    }
}