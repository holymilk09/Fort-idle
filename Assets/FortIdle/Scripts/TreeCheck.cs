using System.IO;
using System.Text;
using UnityEngine;

namespace FortIdle
{
    public static class TreeCheck
    {
        public const float MaxDegrees = 5f;

        public static void Report()
        {
            int fail = 0;
            int count = FortWorld.Plants.Count;
            float maxAngle = 0f;
            StringBuilder file = new StringBuilder(4096);
            file.Append("thresholdDegrees=5\n");
            file.Append("angle=angle(transform.up, Vector3.up)\n");
            for (int i = 0; i < count; i++)
            {
                Transform plant = FortWorld.Plants[i];
                if (plant == null)
                {
                    fail += 1;
                    Debug.Log("FORTIDLE tree-check missing index=" + i);
                    continue;
                }

                float rootDeg = Vector3.Angle(plant.up, Vector3.up);
                if (rootDeg > maxAngle)
                    maxAngle = rootDeg;
                float trunkDeg = 90f;
                bool anyMesh = false;
                string meshName = "-";
                Bounds bounds = new Bounds();
                bool anyBounds = false;
                MeshFilter[] filters = plant.GetComponentsInChildren<MeshFilter>(true);
                for (int f = 0; f < filters.Length; f++)
                {
                    MeshFilter filter = filters[f];
                    if (filter == null)
                        continue;
                    Renderer rend = filter.GetComponent<Renderer>();
                    if (rend != null && !rend.enabled)
                        continue;
                    float deg = FortArt.TrunkDegrees(filter.gameObject);
                    if (!anyMesh || deg > trunkDeg)
                    {
                        trunkDeg = deg;
                        meshName = filter.name;
                        anyMesh = true;
                    }

                    if (rend == null)
                        continue;
                    if (!anyBounds)
                    {
                        bounds = rend.bounds;
                        anyBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(rend.bounds);
                    }
                }

                if (!anyMesh)
                    trunkDeg = 90f;
                float wide = Mathf.Max(bounds.size.x, bounds.size.z);
                bool ok = rootDeg <= MaxDegrees && trunkDeg <= MaxDegrees;
                if (!ok)
                    fail += 1;
                string verdict = ok ? "pass" : "FAIL";
                string line = "FORTIDLE tree-check name=" + plant.name
                    + " mesh=" + meshName
                    + " up=" + plant.up.x.ToString("0.00") + "," + plant.up.y.ToString("0.00") + "," + plant.up.z.ToString("0.00")
                    + " upAngle=" + rootDeg.ToString("0.00")
                    + " trunkDeg=" + trunkDeg.ToString("0.0")
                    + " height=" + bounds.size.y.ToString("0.00")
                    + " wide=" + wide.ToString("0.00")
                    + " " + verdict;
                Debug.Log(line);
                file.Append(line).Append('\n');
            }

            string summary = fail > 0
                ? "FORTIDLE trees FAIL count=" + fail + " of=" + count + " maxUpAngle=" + maxAngle.ToString("0.00")
                : "FORTIDLE trees ok count=" + count + " fail=0 maxUpAngle=" + maxAngle.ToString("0.00");
            Debug.Log(summary);
            file.Append(summary).Append('\n');
            try
            {
                File.WriteAllText(Path.Combine(PoseShot.ShotDir(), "tree-up-assertion.txt"), file.ToString());
            }
            catch (System.Exception error) // UnauthorizedAccessException is not an IOException; never break boot
            {
                Debug.Log("FORTIDLE tree-check file FAIL " + error.Message);
            }
        }
    }
}
