using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FortIdle
{
    // One structure, several named part groups. Each group is a stage.
    // Art can replace a group later without changing the pour math.
    public class StagedBuild : MonoBehaviour
    {
        public struct Part
        {
            public int Layer;
            public int Index;
            public Transform Root;
            public string Motion;
            public Transform Actor;
            public Vector3 RestPos;
            public Vector3 RestScale;
            public Quaternion RestRot;
            public Vector3 ActorRestPos;
            public Quaternion ActorRestRot;
        }

        readonly List<Part> parts = new List<Part>();
        readonly List<int> shown = new List<int>();
        int snapping;
        int puffLive;
        Vector3 restScale = Vector3.one;
        public Transform Aim;

        public int Shown(int layer)
        {
            if (layer < 0 || layer >= shown.Count)
                return 0;
            return shown[layer];
        }

        public static int Reached(int delivered, int cost, int stages)
        {
            if (stages < 1)
                stages = 1;
            if (delivered <= 0)
                return 0;
            if (cost < 1)
                cost = 1;
            if (delivered >= cost)
                return stages;
            float threshold = cost / (float)stages;
            if (threshold < 0.001f)
                threshold = 0.001f;
            int reached = Mathf.FloorToInt(delivered / threshold);
            if (reached < 0)
                reached = 0;
            if (reached > stages)
                reached = stages;
            return reached;
        }

        public void Add(int layer, int index, Transform root, string motion, Transform actor)
        {
            if (root == null)
                return;
            while (shown.Count <= layer)
                shown.Add(0);
            Part part = new Part();
            part.Layer = layer;
            part.Index = index;
            part.Root = root;
            part.Motion = motion == null ? "drop" : motion;
            part.Actor = actor;
            part.RestPos = root.localPosition;
            part.RestScale = root.localScale;
            part.RestRot = root.localRotation;
            if (actor != null)
            {
                part.ActorRestPos = actor.localPosition;
                part.ActorRestRot = actor.localRotation;
            }

            parts.Add(part);
            root.gameObject.SetActive(false);
        }

        public int Count(int layer)
        {
            int count = 0;
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].Layer == layer)
                    count += 1;
            }

            return count;
        }

        public void Sync(int layer, int delivered, int cost, int stages, bool animate)
        {
            int available = Count(layer);
            if (available < 1)
                return;
            if (stages < 1)
                stages = available;
            if (stages > available)
                stages = available;
            int want = Reached(delivered, cost, stages);
            int have = Shown(layer);
            if (want <= have)
                return;
            for (int index = have; index < want; index++)
            {
                int slot = Find(layer, index);
                if (slot < 0)
                    continue;
                bool last = index == stages - 1;
                StartCoroutine(Snap(parts[slot], animate, last));
            }

            while (shown.Count <= layer)
                shown.Add(0);
            shown[layer] = want;
        }

        public void Conceal()
        {
            StopAllCoroutines();
            snapping = 0;
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].Root != null)
                    parts[i].Root.gameObject.SetActive(false);
            }

            for (int i = 0; i < shown.Count; i++)
                shown[i] = 0;
            transform.localScale = restScale;
        }

        public void SetStrain(float fraction)
        {
            if (snapping > 0)
                return;
            float y = Mathf.Clamp(fraction, 0.05f, 1f);
            transform.localScale = new Vector3(restScale.x, restScale.y * y, restScale.z);
        }

        public IEnumerator PlayBuilt()
        {
            while (snapping > 0)
                yield return null;
            BuildThud.Play(1.4f);
            Sparkle();
            FortIdleConfig.StageTuning tune = Tune();
            float duration = tune.builtSeconds;
            float punch = tune.builtPunch;
            float age = 0f;
            while (age < duration)
            {
                age += Time.deltaTime;
                float u = duration <= 0f ? 1f : Mathf.Clamp01(age / duration);
                float bell = Mathf.Sin(u * Mathf.PI);
                float scale = 1f + (punch - 1f) * bell;
                transform.localScale = restScale * scale;
                yield return null;
            }

            transform.localScale = restScale;
        }

        int Find(int layer, int index)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].Layer == layer && parts[i].Index == index)
                    return i;
            }

            return -1;
        }

        IEnumerator Snap(Part part, bool animate, bool last)
        {
            snapping += 1;
            Transform root = part.Root;
            if (root == null)
            {
                snapping -= 1;
                yield break;
            }

            root.gameObject.SetActive(true);
            if (!animate || part.Motion == "climb")
            {
                root.localPosition = part.RestPos;
                root.localScale = part.RestScale;
                root.localRotation = part.RestRot;
                if (!animate)
                {
                    SettleActor(part, true);
                    snapping -= 1;
                    yield break;
                }
            }

            FortIdleConfig.StageTuning tune = Tune();
            float duration = tune.snapSeconds <= 0f ? 0.16f : tune.snapSeconds;
            float drop = part.Motion == "rise" ? -tune.dropHeight : tune.dropHeight;
            if (part.Motion == "climb")
                drop = 0f;
            Vector3 from = part.RestPos + new Vector3(0f, drop, 0f);
            bool thud = part.Motion == "climb";
            float age = 0f;
            if (part.Motion != "climb")
            {
                while (age < duration)
                {
                    age += Time.deltaTime;
                    float u = duration <= 0f ? 1f : Mathf.Clamp01(age / duration);
                    float eased = 1f - (1f - u) * (1f - u);
                    root.localPosition = Vector3.Lerp(from, part.RestPos, eased);
                    float squash = 1f;
                    if (u > 0.7f)
                    {
                        float s = (u - 0.7f) / 0.3f;
                        float bell = Mathf.Sin(s * Mathf.PI);
                        squash = 1f + (tune.squash - 1f) * bell;
                        if (!thud)
                        {
                            thud = true;
                            BuildThud.Play(0.82f);
                            Puff(root.position);
                        }
                    }

                    float wide = 2f - squash;
                    Vector3 scale = part.RestScale;
                    root.localScale = new Vector3(scale.x * wide, scale.y * squash, scale.z * wide);
                    yield return null;
                }
            }

            root.localPosition = part.RestPos;
            root.localScale = part.RestScale;
            root.localRotation = part.RestRot;
            if (part.Motion == "open")
                root.localRotation = Quaternion.Euler(0f, 72f, 0f);
            yield return MotionAfter(part);
            if (last && FortWorld.Rig != null)
                FortWorld.Rig.Nudge(root.position);
            snapping -= 1;
        }

        IEnumerator MotionAfter(Part part)
        {
            Transform actor = part.Actor != null ? part.Actor : part.Root;
            if (actor == null)
                yield break;
            if (part.Motion == "swing")
            {
                Quaternion open = Quaternion.Euler(0f, 72f, 0f);
                Quaternion shut = part.Actor != null ? part.ActorRestRot : part.RestRot;
                float age = 0f;
                const float duration = 0.48f;
                while (age < duration)
                {
                    age += Time.deltaTime;
                    actor.localRotation = Quaternion.Slerp(open, shut, Mathf.Clamp01(age / duration));
                    yield return null;
                }

                actor.localRotation = shut;
            }
            else if (part.Motion == "spin")
            {
                float age = 0f;
                const float duration = 0.55f;
                while (age < duration)
                {
                    age += Time.deltaTime;
                    float turns = Mathf.Clamp01(age / duration);
                    actor.localRotation = Quaternion.Euler(0f, 360f * turns, 0f) * (part.Actor != null ? part.ActorRestRot : part.RestRot);
                    yield return null;
                }

                actor.localRotation = part.Actor != null ? part.ActorRestRot : part.RestRot;
                BuildThud.Play(1.15f);
            }
            else if (part.Motion == "climb")
            {
                Vector3 high = part.Actor != null ? part.ActorRestPos : part.RestPos;
                Vector3 low = high + Vector3.down * 2.2f;
                float age = 0f;
                const float duration = 0.5f;
                BuildThud.Play(0.9f);
                Puff(actor.position);
                while (age < duration)
                {
                    age += Time.deltaTime;
                    float u = Mathf.Clamp01(age / duration);
                    float smooth = u * u * (3f - 2f * u);
                    actor.localPosition = Vector3.Lerp(low, high, smooth);
                    yield return null;
                }

                actor.localPosition = high;
            }
        }

        static void SettleActor(Part part, bool instant)
        {
            if (!instant || part.Actor == null)
                return;
            part.Actor.localPosition = part.ActorRestPos;
            part.Actor.localRotation = part.ActorRestRot;
        }

        void Puff(Vector3 position)
        {
            FortIdleConfig.StageTuning tune = Tune();
            int cap = tune.puffCap < 1 ? 28 : tune.puffCap;
            int chips = tune.puffChips < 1 ? 6 : tune.puffChips;
            for (int i = 0; i < chips; i++)
            {
                if (puffLive >= cap)
                    return;
                puffLive += 1;
                StartCoroutine(Chip(position));
            }
        }

        IEnumerator Chip(Vector3 position)
        {
            Material material = FortWorld.Look != null ? FortWorld.Look.Plank : null;
            GameObject chip = FortPool.Take(320, () =>
            {
                GameObject made = Prop.Primitive(PrimitiveType.Cube, material, null, false);
                made.name = "Chip";
                return made;
            });
            chip.transform.position = position + Vector3.up * 0.15f;
            chip.transform.localScale = Vector3.one * 0.07f;
            Vector3 velocity = new Vector3(Random.Range(-1.4f, 1.4f), Random.Range(1.1f, 2.6f), Random.Range(-1.4f, 1.4f));
            float age = 0f;
            const float life = 0.32f;
            while (age < life)
            {
                age += Time.deltaTime;
                velocity.y -= 8f * Time.deltaTime;
                chip.transform.position += velocity * Time.deltaTime;
                float scale = Mathf.Lerp(0.07f, 0.01f, age / life);
                chip.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            FortPool.Give(320, chip);
            puffLive -= 1;
        }

        void Sparkle()
        {
            StartCoroutine(Spark());
        }

        IEnumerator Spark()
        {
            Material material = FortWorld.Look != null ? FortWorld.Look.Fire : null;
            for (int i = 0; i < 8; i++)
            {
                if (puffLive >= 40)
                    break;
                puffLive += 1;
                GameObject bit = Prop.Primitive(PrimitiveType.Sphere, material, null, false);
                bit.name = "Spark";
                bit.transform.position = transform.position + Vector3.up * 1.1f;
                bit.transform.localScale = Prop.SphereScale(0.16f);
                Vector3 velocity = Random.onUnitSphere * 1.8f;
                velocity.y = Mathf.Abs(velocity.y) + 1.2f;
                float age = 0f;
                while (age < 0.4f)
                {
                    age += Time.deltaTime;
                    velocity.y -= 4f * Time.deltaTime;
                    bit.transform.position += velocity * Time.deltaTime;
                    yield return null;
                }

                Destroy(bit);
                puffLive -= 1;
            }
        }

        static FortIdleConfig.StageTuning Tune()
        {
            if (FortWorld.Config != null && FortWorld.Config.stages != null)
                return FortWorld.Config.stages;
            return new FortIdleConfig.StageTuning();
        }
    }
}
