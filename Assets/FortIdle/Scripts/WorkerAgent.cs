using UnityEngine;

namespace FortIdle
{
    public class WorkerAgent : MonoBehaviour
    {
        public enum Duty
        {
            Wood,
            Stone
        }

        static bool loggedChop;
        static bool loggedDeliver;

        PlankStack stack;
        Transform visual;
        FortAnim anim;
        bool stepped;
        Duty job;
        float yaw;
        float chopTimer;
        float pourAccumulator;
        float swingLeft;
        bool emptying;

        public static WorkerAgent SpawnNext()
        {
            FortIdleConfig config = FortWorld.Config;
            if (config == null || FortWorld.Hut == null)
                return null;

            int index = FortWorld.Workers.Count;
            Vector3 position = FortWorld.Hut.transform.position
                + config.workers.spawnLocal
                + new Vector3(config.workers.spawnSpacing * index, 0f, 0f);
            position.y = 0f;

            GameObject root = new GameObject("Worker");
            root.transform.position = position;
            GameObject visualObject = new GameObject("Visual");
            visualObject.transform.SetParent(root.transform, false);

            float workerHeight = config.workers.headHeight + config.workers.headDiameter * 0.5f;
            if (FortArt.Stand(visualObject.transform, "Worker", workerHeight) == null)
            {
                GameObject body = Prop.Primitive(PrimitiveType.Capsule, FortWorld.Look.WorkerBody, visualObject.transform, false);
                body.name = "Body";
                body.transform.localScale = Prop.CapsuleScale(config.workers.bodyRadius, config.workers.bodyHeight);
                body.transform.localPosition = new Vector3(0f, config.workers.bodyHeight * 0.5f, 0f);

                GameObject head = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.WorkerHead, visualObject.transform, false);
                head.name = "Head";
                head.transform.localScale = Prop.SphereScale(config.workers.headDiameter);
                head.transform.localPosition = new Vector3(0f, config.workers.headHeight, 0f);
            }

            GameObject stackObject = new GameObject("Stack");
            stackObject.transform.SetParent(visualObject.transform, false);
            PlankStack carried = stackObject.AddComponent<PlankStack>();
            carried.Init(config, FortWorld.Look.Plank);
            carried.UseCapacity(config.workers.carryCapacity);

            WorkerAgent worker = root.AddComponent<WorkerAgent>();
            worker.stack = carried;
            worker.visual = visualObject.transform;
            worker.anim = visualObject.GetComponentInChildren<FortAnim>();
            FortWorld.Workers.Add(worker);
            Debug.Log("FORTIDLE worker=spawned count=" + FortWorld.Workers.Count);
            return worker;
        }

        public Duty Job => job;
        public int Carried => stack != null ? stack.Count : 0;

        public void Assign(Duty duty)
        {
            if (job == duty || stack == null)
                return;
            job = duty;
            while (stack.Count > 0)
                stack.RemoveOne();
            Material material = duty == Duty.Stone ? FortWorld.Look.Stone : FortWorld.Look.Plank;
            stack.SetMaterial(material);
            if (duty == Duty.Stone)
                stack.UseCapacity(FortWorld.Config.supplies.workerStoneCapacity);
            else
                stack.UseCapacity(FortWorld.Config.workers.carryCapacity);
            Debug.Log("FORTIDLE worker=assign " + duty);
        }

        public static bool AssignOne(Duty duty)
        {
            for (int i = 0; i < FortWorld.Workers.Count; i++)
            {
                WorkerAgent worker = FortWorld.Workers[i];
                if (worker.Job == duty)
                    continue;
                worker.Assign(duty);
                return true;
            }

            return false;
        }

        public static bool Any(Duty duty)
        {
            for (int i = 0; i < FortWorld.Workers.Count; i++)
            {
                if (FortWorld.Workers[i].Job == duty)
                    return true;
            }

            return false;
        }

        void Update()
        {
            FortIdleConfig config = FortWorld.Config;
            if (config == null || stack == null)
                return;

            float delta = Time.deltaTime;
            bool stoneJob = job == Duty.Stone;
            TreeNode tree = stoneJob ? null : NearestTree();
            RockNode rock = stoneJob ? NearestRock() : null;
            BuildPad pad = NearestOpenPad();
            bool sourceAvailable = stoneJob ? rock != null : tree != null;
            bool carrying = stack.Count > 0;
            int needed = stoneJob ? (pad != null ? pad.StoneRemaining : 0) : (pad != null ? pad.WoodRemaining : 0);
            bool enough = pad != null && needed > 0 && stack.Count >= needed;
            if (!carrying)
                emptying = false;
            else if (stack.IsFull || !sourceAvailable || enough)
                emptying = true;

            Vector3 destination = IdlePoint();
            bool hauling = false;
            if (emptying && pad != null)
            {
                destination = pad.transform.position;
                hauling = true;
                if (FortMath.HorizontalDistance(transform.position, destination) <= config.workers.padArriveDistance)
                    destination = transform.position;
            }
            else if (emptying)
            {
                destination = StockPoint();
                hauling = true;
                if (FortMath.HorizontalDistance(transform.position, destination) <= config.workers.stockArriveDistance)
                    destination = transform.position;
            }
            else if (!stoneJob && tree != null && stack.HasRoom)
            {
                destination = tree.transform.position;
                float arrive = config.chop.range * config.workers.chopRangeFactor;
                if (FortMath.HorizontalDistance(transform.position, destination) <= arrive)
                    destination = transform.position;
            }
            else if (stoneJob && rock != null && stack.HasRoom)
            {
                destination = rock.transform.position;
                if (FortMath.HorizontalDistance(transform.position, destination) <= config.supplies.mineRange * config.workers.chopRangeFactor)
                    destination = transform.position;
            }

            MoveToward(destination, delta);
            Separate(delta);
            stack.Tick(Vector3.zero, delta);

            if (!hauling && stoneJob)
                Mine(rock, delta);
            else if (!hauling)
                Chop(tree, delta);
            else if (pad != null)
                Pour(pad, delta);
            else if (!stoneJob)
                Dump(delta);

            Swing(delta);
            PlayClip();
        }

        void PlayClip()
        {
            if (anim == null || !anim.Ready)
                return;
            string clip = "Idle";
            if (swingLeft > 0f)
                clip = "Punch";
            else if (stepped && stack != null && stack.Count > 0)
                clip = "Walk_Carry";
            else if (stepped)
                clip = "Walk";
            anim.Play(clip);
        }

        void MoveToward(Vector3 destination, float delta)
        {
            FortIdleConfig config = FortWorld.Config;
            float clearance = config.workers.bodyRadius + config.workers.bodyRadius;
            Vector3 via;
            bool detour = FortNav.TryDetour(transform.position, destination, clearance, out via);
            Vector3 stepTarget = detour ? via : destination;
            Vector3 to = stepTarget - transform.position;
            to.y = 0f;
            float distance = to.magnitude;
            stepped = false;
            if (distance <= config.player.minMoveDistance)
                return;
            stepped = true;

            Vector3 direction = to / distance;
            float work = FortWorld.Progress != null ? FortWorld.Progress.WorkRate : 1f;
            float step = config.workers.moveSpeed * work * delta;
            if (step > distance)
                step = distance;
            transform.position += direction * step;
            Clamp();

            float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float blend = 1f - Mathf.Exp(-config.workers.turnSharpness * delta);
            yaw = Mathf.LerpAngle(yaw, targetYaw, blend);
            if (visual != null)
                visual.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        void Clamp()
        {
            FortIdleConfig config = FortWorld.Config;
            float limit = config.layout.groundExtent * 0.5f - config.player.boundaryMargin;
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, -limit, limit);
            position.z = Mathf.Clamp(position.z, -limit, limit);
            position.y = 0f;
            transform.position = position;
        }

        void Separate(float delta)
        {
            FortIdleConfig config = FortWorld.Config;
            float radius = config.workers.separationRadius;
            float strength = config.workers.separationStrength;
            if (radius <= 0f || strength <= 0f)
                return;

            float push = 0f;
            for (int i = 0; i < FortWorld.Workers.Count; i++)
            {
                WorkerAgent other = FortWorld.Workers[i];
                if (other == this)
                    continue;
                float dx = transform.position.x - other.transform.position.x;
                float abs = Mathf.Abs(dx);
                if (abs >= radius || abs <= 0f)
                    continue;
                push += Mathf.Sign(dx) * (radius - abs);
            }

            if (push == 0f)
                return;
            Vector3 position = transform.position;
            position.x += Mathf.Clamp(push, -1f, 1f) * strength * delta;
            transform.position = position;
        }

        void Chop(TreeNode tree, float delta)
        {
            FortIdleConfig config = FortWorld.Config;
            if (tree == null || !stack.HasRoom)
                return;
            if (FortMath.HorizontalDistance(transform.position, tree.transform.position) > config.chop.range)
                return;

            chopTimer += delta;
            if (chopTimer < config.chop.interval)
                return;

            chopTimer = 0f;
            swingLeft = config.chop.swingDuration;
            int planks = config.chop.planksPerHit + config.progression.workerBonusPlanks;
            int gain = stack.Add(planks);
            if (gain > 0 && FortWorld.Progress != null)
            {
                FortWorld.Progress.NoteWood(gain, true);
                FortWorld.Progress.NoteWorkerWood(gain);
            }
            tree.ApplyHit();
            if (!loggedChop)
            {
                loggedChop = true;
                Debug.Log("FORTIDLE worker=chop");
            }

            Vector3 from = tree.transform.position + Vector3.up * config.layout.canopyHeight;
            for (int i = 0; i < gain; i++)
            {
                float offset = (i - (gain - 1) * 0.5f) * config.stack.plankSize.x;
                Flyer.Spawn(
                    from + new Vector3(offset, 0f, 0f),
                    stack.TopWorld(),
                    config.chop.flyDuration,
                    config.chop.flyArcHeight,
                    FortWorld.Look.Plank,
                    config.stack.plankSize);
            }
        }

        void Mine(RockNode rock, float delta)
        {
            FortIdleConfig config = FortWorld.Config;
            if (rock == null || !stack.HasRoom)
                return;
            if (FortMath.HorizontalDistance(transform.position, rock.transform.position) > config.supplies.mineRange)
                return;

            chopTimer += delta;
            if (chopTimer < config.chop.interval)
                return;
            chopTimer = 0f;
            swingLeft = config.chop.swingDuration;
            int gain = stack.Add(config.supplies.stonePerHit);
            if (gain > 0 && FortWorld.Progress != null)
                FortWorld.Progress.NoteStoneFound(gain);
            rock.ApplyHit();
            Vector3 from = rock.transform.position + Vector3.up * config.supplies.rockHeight;
            for (int i = 0; i < gain; i++)
            {
                Flyer.Spawn(
                    from,
                    stack.TopWorld(),
                    config.chop.flyDuration,
                    config.chop.flyArcHeight,
                    FortWorld.Look.Stone,
                    config.supplies.stoneChunkSize);
            }
        }

        void Pour(BuildPad pad, float delta)
        {
            FortIdleConfig config = FortWorld.Config;
            bool stoneJob = job == Duty.Stone;
            int remaining = stoneJob ? pad.StoneRemaining : pad.WoodRemaining;
            if (pad == null || stack.Count <= 0 || remaining <= 0)
                return;
            if (FortMath.HorizontalDistance(transform.position, pad.transform.position) > config.pour.padRadius)
                return;

            Material material = stoneJob ? FortWorld.Look.Stone : FortWorld.Look.Plank;
            Vector3 size = stoneJob ? config.supplies.stoneChunkSize : config.stack.plankSize;
            pourAccumulator += config.workers.deliverPlanksPerSecond * delta;
            while (pourAccumulator >= 1f && stack.Count > 0)
            {
                int left = stoneJob ? pad.StoneRemaining : pad.WoodRemaining;
                if (left <= 0)
                    break;
                pourAccumulator -= 1f;
                Vector3 from = stack.RemoveOne();
                if (stoneJob)
                {
                    pad.AddStone();
                    if (FortWorld.Progress != null)
                        FortWorld.Progress.NoteStoneSpent(1);
                }
                else
                    pad.AddWood();
                NoteDeliver();
                Flyer.Spawn(
                    from,
                    pad.DeliveryPoint(),
                    config.pour.flyDuration,
                    config.pour.flyArcHeight,
                    material,
                    size);
            }
        }

        void Dump(float delta)
        {
            FortIdleConfig config = FortWorld.Config;
            if (FortWorld.Stock == null || stack.Count <= 0)
                return;
            if (FortMath.HorizontalDistance(transform.position, StockPoint()) > config.workers.stockArriveDistance)
                return;

            pourAccumulator += config.workers.deliverPlanksPerSecond * delta;
            while (pourAccumulator >= 1f && stack.Count > 0)
            {
                pourAccumulator -= 1f;
                Vector3 from = stack.RemoveOne();
                FortWorld.Stock.Deposit();
                NoteDeliver();
                Flyer.Spawn(
                    from,
                    FortWorld.Stock.transform.position + Vector3.up * config.stack.visualHeightStep,
                    config.pour.flyDuration,
                    config.pour.flyArcHeight,
                    FortWorld.Look.Plank,
                    config.stack.plankSize);
            }
        }

        static void NoteDeliver()
        {
            if (loggedDeliver)
                return;
            loggedDeliver = true;
            int stock = FortWorld.Progress != null ? FortWorld.Progress.Stockpile : 0;
            Debug.Log("FORTIDLE worker=deliver stock=" + stock);
        }

        void Swing(float delta)
        {
            if (swingLeft > 0f)
                swingLeft -= delta;
        }

        TreeNode NearestTree()
        {
            TreeNode found = null;
            float best = float.PositiveInfinity;
            for (int i = 0; i < FortWorld.Trees.Count; i++)
            {
                TreeNode tree = FortWorld.Trees[i];
                if (!tree.Standing || !Reachable(tree))
                    continue;
                float distance = FortMath.HorizontalDistance(transform.position, tree.transform.position);
                if (distance < best)
                {
                    best = distance;
                    found = tree;
                }
            }

            return found;
        }

        bool Reachable(TreeNode tree)
        {
            for (int i = 0; i < FortWorld.Pads.Count; i++)
            {
                if (FortWorld.Pads[i].BlocksSegment(transform.position, tree.transform.position, 0f))
                    return false;
            }

            return true;
        }

        RockNode NearestRock()
        {
            RockNode found = null;
            float best = float.PositiveInfinity;
            for (int i = 0; i < FortWorld.Rocks.Count; i++)
            {
                RockNode rock = FortWorld.Rocks[i];
                if (!rock.Available)
                    continue;
                float distance = FortMath.HorizontalDistance(transform.position, rock.transform.position);
                if (distance < best)
                {
                    best = distance;
                    found = rock;
                }
            }

            return found;
        }

        BuildPad NearestOpenPad()
        {
            bool stoneJob = job == Duty.Stone;
            BuildPad urgent = null;
            float worst = 0.55f;
            if (!stoneJob)
            {
                for (int i = 0; i < FortWorld.Pads.Count; i++)
                {
                    BuildPad pad = FortWorld.Pads[i];
                    if (!pad.NeedsRepair || pad.MaxHitPoints <= 0f)
                        continue;
                    float fraction = pad.HitPoints / pad.MaxHitPoints;
                    if (fraction < worst)
                    {
                        worst = fraction;
                        urgent = pad;
                    }
                }
            }

            if (urgent != null)
                return urgent;

            BuildPad found = null;
            float best = float.PositiveInfinity;
            for (int i = 0; i < FortWorld.Pads.Count; i++)
            {
                BuildPad pad = FortWorld.Pads[i];
                bool wants = stoneJob ? pad.WantsStone : pad.WantsWood;
                if (!wants)
                    continue;
                float distance = FortMath.HorizontalDistance(transform.position, pad.transform.position);
                if (distance < best)
                {
                    best = distance;
                    found = pad;
                }
            }

            return found;
        }

        Vector3 IdlePoint()
        {
            if (FortWorld.Hut != null)
                return FortWorld.Hut.transform.position + FortWorld.Config.workers.spawnLocal;
            return transform.position;
        }

        static Vector3 StockPoint()
        {
            if (FortWorld.Stock != null)
                return FortWorld.Stock.transform.position;
            if (FortWorld.Hut != null)
                return FortWorld.Hut.transform.position + FortWorld.Config.layout.stockpileOffset;
            return Vector3.zero;
        }
    }
}
