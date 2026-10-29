using System.Collections.Generic;
using UnityEngine;

namespace FortIdle
{
    // Remembers the bind pose at spawn. A real clip moves the arms off that pose.
    public class PoseWatch : MonoBehaviour
    {
        struct Sample
        {
            public Transform Bone;
            public Quaternion Bind;
        }

        Sample[] samples = new Sample[0];
        string who = "Actor";

        public string Who => who;

        public static void Attach(GameObject piece, string name)
        {
            if (piece == null || piece.GetComponent<PoseWatch>() != null)
                return;
            PoseWatch watch = piece.AddComponent<PoseWatch>();
            watch.who = name;
        }

        void Awake()
        {
            Transform[] bones = GetComponentsInChildren<Transform>(true);
            List<Sample> found = new List<Sample>();
            for (int i = 0; i < bones.Length; i++)
            {
                string bone = bones[i].name;
                if (bone.IndexOf("Arm") < 0 && bone.IndexOf("Spine") < 0 && bone.IndexOf("Head") < 0 && bone.IndexOf("Leg") < 0)
                    continue;
                Sample sample = new Sample();
                sample.Bone = bones[i];
                sample.Bind = bones[i].localRotation;
                found.Add(sample);
            }

            samples = found.ToArray();
        }

        public float Drift()
        {
            float max = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                if (samples[i].Bone == null)
                    continue;
                float angle = Quaternion.Angle(samples[i].Bind, samples[i].Bone.localRotation);
                if (angle > max)
                    max = angle;
            }

            return max;
        }
    }

    // -demo: one second after spawn, fail anyone still sitting in the bind pose.
    public class PoseCheck : MonoBehaviour
    {
        public const float BindDegrees = 2f;

        public static bool Requested()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-demo" || args[i] == "-poseshot")
                    return true;
            }

            return false;
        }

        System.Collections.IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);
            Report();
        }

        public static void Report()
        {
            PoseWatch[] watches = Object.FindObjectsByType<PoseWatch>(FindObjectsSortMode.None);
            int fail = 0;
            for (int i = 0; i < watches.Length; i++)
            {
                float degrees = watches[i].Drift();
                bool bad = degrees < BindDegrees;
                if (bad)
                    fail += 1;
                Debug.Log("FORTIDLE pose name=" + watches[i].Who
                    + " degrees=" + degrees.ToString("0.0")
                    + (bad ? " FAIL" : " pass"));
            }

            if (fail > 0)
                Debug.Log("FORTIDLE pose FAIL count=" + fail + " of=" + watches.Length);
            else
                Debug.Log("FORTIDLE pose ok count=" + watches.Length + " fail=0");
        }
    }
}
