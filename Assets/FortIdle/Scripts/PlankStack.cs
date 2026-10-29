using System.Collections.Generic;
using UnityEngine;

namespace FortIdle
{
    public class PlankStack : MonoBehaviour
    {
        FortIdleConfig.StackTuning tuning;
        Material plankMaterial;
        string meshId = "WoodLog";
        Transform sway;
        readonly List<Transform> planks = new List<Transform>();
        int count;
        int forcedCapacity;
        float roll;
        float pitch;
        float rollVelocity;
        float pitchVelocity;

        public int Count => count;
        public int Capacity => forcedCapacity > 0 ? forcedCapacity : HeroCapacity();
        public bool HasRoom => count < Capacity;
        public bool IsFull => count >= Capacity;

        public void UseCapacity(int capacity)
        {
            forcedCapacity = capacity;
        }

        int HeroCapacity()
        {
            if (FortWorld.Progress != null)
                return FortWorld.Progress.StackCapacity;
            return tuning.capacity;
        }

        public void SetMaterial(Material material)
        {
            plankMaterial = material;
            string next = material == FortWorld.Look.Stone ? "RockMoss" : "WoodLog";
            if (next == meshId)
                return;
            meshId = next;
            for (int i = 0; i < planks.Count; i++)
            {
                if (planks[i] != null)
                    Object.Destroy(planks[i].gameObject);
            }

            planks.Clear();
            Rebuild();
        }

        public void Init(FortIdleConfig config, Material material)
        {
            tuning = config.stack;
            plankMaterial = material;
            GameObject swayObject = new GameObject("Sway");
            sway = swayObject.transform;
            sway.SetParent(transform, false);
            sway.localPosition = tuning.localOffset;
            sway.localRotation = Quaternion.identity;
            sway.localScale = Vector3.one;
        }

        public int Add(int amount)
        {
            int space = Capacity - count;
            int gain = amount < space ? amount : space;
            if (gain < 0)
                gain = 0;
            count += gain;
            Rebuild();
            return gain;
        }

        public Vector3 RemoveOne()
        {
            Vector3 top = TopWorld();
            if (count <= 0)
                return top;
            count -= 1;
            Rebuild();
            return top;
        }

        public Vector3 TopWorld()
        {
            int visible = count < tuning.maxRenderedPlanks ? count : tuning.maxRenderedPlanks;
            float y = visible * tuning.visualHeightStep;
            return sway.TransformPoint(new Vector3(0f, y, 0f));
        }

        public void Tick(Vector3 worldVelocity, float deltaTime)
        {
            float speed = FortWorld.Config.player.moveSpeed;
            if (speed <= 0f)
                speed = 1f;
            Vector3 local = transform.InverseTransformDirection(worldVelocity);
            float targetRoll = Mathf.Clamp(-local.x / speed, -1f, 1f) * tuning.swayDegrees;
            float targetPitch = Mathf.Clamp(-local.z / speed, -1f, 1f) * tuning.swayDegrees;
            if (tuning.swaySmoothTime <= 0f)
            {
                roll = targetRoll;
                pitch = targetPitch;
            }
            else
            {
                roll = Mathf.SmoothDamp(roll, targetRoll, ref rollVelocity, tuning.swaySmoothTime, Mathf.Infinity, deltaTime);
                pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVelocity, tuning.swaySmoothTime, Mathf.Infinity, deltaTime);
            }

            sway.localRotation = Quaternion.Euler(pitch, 0f, roll);
        }

        void Rebuild()
        {
            int visible = count < tuning.maxRenderedPlanks ? count : tuning.maxRenderedPlanks;
            while (planks.Count < visible)
                planks.Add(MakePlank());

            for (int i = 0; i < planks.Count; i++)
            {
                bool on = i < visible;
                planks[i].gameObject.SetActive(on);
                if (!on)
                    continue;
                planks[i].localPosition = new Vector3(0f, (i + 0.5f) * tuning.visualHeightStep, 0f);
                Renderer renderer = planks[i].GetComponent<Renderer>();
                if (renderer != null && plankMaterial != null && planks[i].GetComponentInChildren<MeshFilter>() == null)
                    renderer.sharedMaterial = plankMaterial;
            }

            int extra = count - visible;
            if (extra < 0)
                extra = 0;
            float scale = 1f + extra * tuning.extraPlankScale;
            sway.localScale = new Vector3(scale, scale, scale);
        }

        Transform MakePlank()
        {
            bool stone = meshId == "RockMoss";
            GameObject box = FortArt.Box(sway, meshId, tuning.plankSize, Color.white, stone ? 0.05f : 0f, false, false);
            if (box == null)
            {
                box = Prop.Primitive(PrimitiveType.Cube, plankMaterial, sway, false);
                box.transform.localScale = tuning.plankSize;
            }

            box.name = "Plank";
            return box.transform;
        }
    }
}
