using System.Collections;
using UnityEngine;

namespace FortIdle
{
    // Night-5 framing for the upright-tree and ground clip.
    public class LookReel : MonoBehaviour
    {
        FollowCamera rig;

        public static bool Requested()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-look")
                    return true;
            }

            return false;
        }

        public void Init(FollowCamera camera)
        {
            rig = camera;
        }

        IEnumerator Start()
        {
            yield return null;
            if (FortWorld.Match != null)
                FortWorld.Match.ShowNight(5);
            if (rig != null)
                rig.Frame(new Vector3(0f, 0f, 6f), 1.55f, 0f);
            NightPlan plan = ThreatMath.Build(5, 0);
            int shamblers = plan.Shamblers < 1 ? 25 : plan.Shamblers;
            float width = FortWorld.Config.layout.spawnHalfWidth;
            float z = FortWorld.Config.layout.spawnZ;
            int total = shamblers + 1;
            for (int i = 0; i < shamblers; i++)
            {
                float t = shamblers <= 1 ? 0.5f : i / (float)(shamblers - 1);
                Spawn(MonsterKind.Shambler, i, total, Mathf.Lerp(-width, width, t), z);
            }

            Spawn(MonsterKind.Treant, shamblers, total, 0f, z + 1.2f);
            Debug.Log("FORTIDLE look night=5 shamblers=" + shamblers + " boss=1");
            yield return new WaitForSeconds(40f);
            Debug.Log("FORTIDLE look=done");
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
