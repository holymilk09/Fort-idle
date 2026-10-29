using System.Collections;
using UnityEngine;

namespace FortIdle
{
    // Three windows: a quiet day, a night-5 sized fight, and 150 shamblers.
    public class PerfHarness : MonoBehaviour
    {
        public static bool Requested()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-perf")
                    return true;
            }

            return false;
        }

        IEnumerator Start()
        {
            if (FortWorld.Match != null)
                FortWorld.Match.ClockHeld = true;
            FrameMeter meter = FrameMeter.Current;
            yield return new WaitForSeconds(1.2f);
            if (meter != null)
                meter.Begin("quiet");
            yield return new WaitForSeconds(4f);
            if (meter != null)
                meter.End("quiet");

            for (int i = FortWorld.Workers.Count; i < 4; i++)
                WorkerAgent.SpawnNext();
            SpawnCrowd(25, true);
            yield return null;
            if (meter != null)
                meter.Begin("night5");
            yield return new WaitForSeconds(5f);
            if (meter != null)
                meter.End("night5");

            if (FortWorld.Match != null)
                FortWorld.Match.ClearAllMonsters();
            SpawnCrowd(150, false);
            yield return null;
            if (meter != null)
                meter.Begin("peak150");
            yield return new WaitForSeconds(5f);
            if (meter != null)
                meter.End("peak150");
            Debug.Log("FORTIDLE perf-done");
            Application.Quit();
        }

        static void SpawnCrowd(int shamblers, bool boss)
        {
            FortIdleConfig config = FortWorld.Config;
            float width = config.layout.spawnHalfWidth;
            float z = config.layout.spawnZ;
            int total = shamblers + (boss ? 1 : 0);
            for (int i = 0; i < shamblers; i++)
            {
                float t = shamblers <= 1 ? 0.5f : i / (float)(shamblers - 1);
                float x = Mathf.Lerp(-width, width, t);
                Spawn(MonsterKind.Shambler, i, total, x, z);
            }

            if (boss)
                Spawn(MonsterKind.Treant, shamblers, total, 0f, z + 1.5f);
            Debug.Log("FORTIDLE perf-spawn shamblers=" + shamblers + " boss=" + (boss ? 1 : 0) + " workers=" + FortWorld.Workers.Count);
        }

        static void Spawn(MonsterKind kind, int slot, int total, float x, float z)
        {
            int key = 400 + (int)kind;
            GameObject go = FortPool.Take(key, () => new GameObject(kind.ToString()));
            go.transform.position = new Vector3(x, FortWorld.Config.layout.spawnHeight, z);
            Monster monster = go.GetComponent<Monster>();
            if (monster == null)
                monster = go.AddComponent<Monster>();
            monster.Init(kind, slot, total, 1f, 1f);
            FortWorld.Monsters.Add(monster);
        }
    }
}
