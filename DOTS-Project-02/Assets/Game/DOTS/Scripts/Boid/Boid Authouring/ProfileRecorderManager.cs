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
        [SerializeField] private int secondsToRecord;
        private BoidProfilerName _profilerName;
        [SerializeField] private BoidCommunicator boidCommunicator;

        string statsText;
        ProfilerRecorder boidRecorder;
        
        bool finishedRecording = false;
        string recordedData = string.Empty;

        private float timeOfStart;

        void OnEnable()
        {
            int capacity = secondsToRecord * 300;
            _profilerName = boidCommunicator.BoidProfilerName;
            boidRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, _profilerName.ToString(), capacity);
            timeOfStart = Time.time;
        }

        private void OnValidate()
        {
            _profilerName = boidCommunicator.BoidProfilerName;
        }

        void OnDisable()
        {
            boidRecorder.Dispose();
        }

        static double GetRecorderFrameAverage(ProfilerRecorder recorder)
        {
            var samplesCount = recorder.Capacity;
            if (samplesCount == 0)
                return 0;

            double r = 0;

            var samples = new List<ProfilerRecorderSample>(samplesCount);
            recorder.CopyTo(samples);
            
            for (var i = 0; i < samples.Count; ++i)
            {
                r += samples[i].Value;
            }

            r /= samples.Count;
            return r;
        }


        void Update()
        {
            if (finishedRecording)
                return;
            
            var sb = new StringBuilder(500);
            var samplesCount = boidRecorder.Count;
            float timeSinceStart = Time.time - timeOfStart;
            
            if (timeSinceStart > secondsToRecord && !finishedRecording)
            {
                float frameAverage = (float)GetRecorderFrameAverage(boidRecorder) * (1e-6f);
                var frameAverageAsString = $"{frameAverage:F2}";
                recordedData = $"Time average: {frameAverageAsString} ms per frame";
                Debug.Log(recordedData);
                finishedRecording = true;
            }
            
            sb.AppendLine($"Recording {_profilerName.ToString()}...");

            if (finishedRecording)
            {
                sb.AppendLine("Recording complete!");
                sb.AppendLine(recordedData);
            }
            
            sb.AppendLine($"Samples used: {samplesCount}");
            sb.AppendLine($"Time: {(int)timeSinceStart}/{secondsToRecord}");

            statsText = sb.ToString();
        }

        void OnGUI()
        {
            GUIStyle style = new GUIStyle
            {
                fontSize = 20
            };
            GUI.TextArea(new Rect(10, 30, 300, 80), statsText, style);
        }
    }
}