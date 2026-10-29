using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace FortIdle
{
    // Frame times for -demo and -perf. Presented ms includes the software GL wait on this VM.
    public class FrameMeter : MonoBehaviour
    {
        public static FrameMeter Current { get; private set; }

        readonly List<float> frames = new List<float>(4096);
        readonly List<int> allocs = new List<int>(4096);
        ProfilerRecorder drawCalls;
        ProfilerRecorder batches;
        ProfilerRecorder triangles;
        ProfilerRecorder gcAlloc;
        readonly List<long> gcSamples = new List<long>(4096);
        long lastBytes;
        float worst = -1f;
        int worstMonsters;
        bool open;
        bool warmed;

        public static bool Wanted()
        {
            return Has("-demo") || Has("-perf") || Has("-qashot");
        }

        void Awake()
        {
            Current = this;
            drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            lastBytes = Allocated();
            Debug.Log("FORTIDLE perf-device gpu=" + SystemInfo.graphicsDeviceName
                + " api=" + SystemInfo.graphicsDeviceType
                + " threads=" + SystemInfo.processorCount);
        }

        void OnDestroy()
        {
            drawCalls.Dispose();
            batches.Dispose();
            triangles.Dispose();
            gcAlloc.Dispose();
            if (Current == this)
                Current = null;
        }

        public void Begin(string name)
        {
            frames.Clear();
            allocs.Clear();
            gcSamples.Clear();
            worst = -1f;
            worstMonsters = 0;
            open = true;
            warmed = false;
            lastBytes = Allocated();
            Debug.Log("FORTIDLE perf-begin " + name);
        }

        public void End(string name)
        {
            open = false;
            int count = frames.Count;
            if (count < 1)
            {
                Debug.Log("FORTIDLE perf name=" + name + " frames=0");
                return;
            }

            float sum = 0f;
            for (int i = 0; i < count; i++)
                sum += frames[i];
            long allocSum = 0;
            int allocFrames = 0;
            for (int i = 0; i < allocs.Count; i++)
            {
                if (allocs[i] < 0)
                    continue;
                allocSum += allocs[i];
                allocFrames += 1;
            }

            long gcSum = 0;
            int gcCount = 0;
            for (int i = 0; i < gcSamples.Count; i++)
            {
                if (gcSamples[i] < 0)
                    continue;
                gcSum += gcSamples[i];
                gcCount += 1;
            }

            frames.Sort();
            float avg = sum / count;
            float p95 = frames[Mathf.Clamp(Mathf.CeilToInt(count * 0.95f) - 1, 0, count - 1)];
            float p99 = frames[Mathf.Clamp(Mathf.CeilToInt(count * 0.99f) - 1, 0, count - 1)];
            int monsters = FortWorld.Monsters == null ? 0 : Living();
            Debug.Log("FORTIDLE perf name=" + name
                + " frames=" + count
                + " avg=" + avg.ToString("0.00")
                + " p95=" + p95.ToString("0.00")
                + " p99=" + p99.ToString("0.00")
                + " worst=" + worst.ToString("0.00")
                + " worstMonsters=" + worstMonsters
                + " allocAvg=" + (allocFrames > 0 ? allocSum / allocFrames : 0)
                + " gcFrame=" + (gcCount > 0 ? gcSum / gcCount : -1)
                + " monsters=" + monsters
                + " draws=" + Read(drawCalls)
                + " batches=" + Read(batches)
                + " tris=" + Read(triangles));
        }

        void Update()
        {
            long now = Allocated();
            int bytes = (int)(now - lastBytes);
            lastBytes = now;
            if (!open)
                return;
            if (!warmed)
            {
                warmed = true;
                return;
            }

            float ms = Time.unscaledDeltaTime * 1000f;
            frames.Add(ms);
            allocs.Add(bytes);
            if (gcAlloc.Valid)
                gcSamples.Add(gcAlloc.LastValue);
            int living = Living();
            if (ms > worst)
            {
                worst = ms;
                worstMonsters = living;
            }
        }

        static int Living()
        {
            int living = 0;
            if (FortWorld.Monsters == null)
                return 0;
            for (int i = 0; i < FortWorld.Monsters.Count; i++)
            {
                Monster monster = FortWorld.Monsters[i];
                if (monster != null && monster.Alive)
                    living += 1;
            }

            return living;
        }

        static long Read(ProfilerRecorder recorder)
        {
            if (!recorder.Valid)
                return -1;
            return recorder.LastValue;
        }

        static long Allocated()
        {
            return Profiler.GetMonoUsedSizeLong();
        }

        static bool Has(string flag)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == flag)
                    return true;
            }

            return false;
        }
    }
}
