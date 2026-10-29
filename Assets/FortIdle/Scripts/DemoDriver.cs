using UnityEngine;

namespace FortIdle
{
    public enum DemoGoal
    {
        None,
        Tree,
        Pad,
        Coin,
        Rock,
        Watch
    }

    [DefaultExecutionOrder(-50)]
    public class DemoDriver : MonoBehaviour
    {
        VirtualJoystick joystick;
        Transform hero;
        float repick;
        Vector3 goal;
        DemoGoal kind;
        bool active;
        bool emptying;
        bool loggedShowcase;

        public void Init(VirtualJoystick stick, Transform heroTransform)
        {
            joystick = stick;
            hero = heroTransform;
            active = FortWorld.Config.demo.runByDefault || HasFlag("-demo");
            enabled = active;
            if (active)
                Debug.Log("FORTIDLE demo=on");
        }

        void Update()
        {
            if (StageShowcase.Requested() || PerfHarness.Requested())
                return;
            if (!active || joystick == null || hero == null)
                return;

            repick -= Time.deltaTime;
            if (repick <= 0f)
            {
                Pick();
                repick = FortWorld.Config.demo.repickInterval;
            }

            TickShowcase();
            Vector2 stick = Vector2.zero;
            Vector3 destination = goal;
            float clearance = FortWorld.Config.player.radius + FortWorld.Config.player.radius;
            bool detour = kind != DemoGoal.None && FortNav.TryDetour(hero.position, goal, clearance, out destination);
            Vector3 to = destination - hero.position;
            to.y = 0f;
            float arrive = detour ? FortWorld.Config.player.minMoveDistance : ArriveDistance();
            if (kind != DemoGoal.None && to.magnitude > arrive)
                stick = new Vector2(to.x, to.z).normalized;
            joystick.SetDemoValue(stick);
        }

        void Pick()
        {
            FortIdleConfig config = FortWorld.Config;
            HeroController body = hero.GetComponent<HeroController>();
            if (body != null && body.Down)
            {
                goal = hero.position;
                kind = DemoGoal.Watch;
                return;
            }

            if (FortWorld.Match != null && FortWorld.Match.Phase == MatchPhase.Resolved && FortWorld.Match.Victory)
            {
                if (NeedsSap())
                {
                    goal = FortWorld.TurretUpgrade.transform.position;
                    kind = DemoGoal.Pad;
                    return;
                }

                if (ResearchVisit())
                    return;
                if (CraftVisit())
                    return;
                if (TryUpgradeGoal(out Vector3 after))
                {
                    goal = after;
                    kind = DemoGoal.Coin;
                    return;
                }
            }

            bool nightFight = FortWorld.Match != null
                && FortWorld.Match.Phase == MatchPhase.Night
                && FortWorld.Match.LivingMonsters > 0;
            if (nightFight && body != null && body.HealthFraction < 0.45f && FortWorld.Progress != null && FortWorld.Progress.Meat > 0 && FortWorld.Fire != null)
            {
                goal = FortWorld.Fire.transform.position;
                kind = DemoGoal.Pad;
                return;
            }

            bool wallDone = FortWorld.Wall != null && FortWorld.Wall.IsComplete;
            bool turretDone = FortWorld.TurretPad != null && FortWorld.TurretPad.IsComplete;
            if (nightFight && wallDone && turretDone && !NeedsSap())
            {
                bool holding = FortWorld.Stack != null && FortWorld.Stack.Count > 0;
                bool torn = FortWorld.Wall.NeedsRepair && FortWorld.Wall.HitPoints / FortWorld.Wall.MaxHitPoints < 0.35f;
                goal = torn && holding ? FortWorld.Wall.transform.position : config.layout.wallPadPosition;
                kind = DemoGoal.Pad;
                return;
            }

            if (HaulRepair(0.4f))
                return;

            bool hutDone = FortWorld.Hut != null && FortWorld.Hut.IsComplete;
            BuildPad pad = null;
            if (!wallDone)
                pad = FortWorld.Wall;
            else if (!turretDone)
                pad = FortWorld.TurretPad;
            else if (MealReady())
            {
                goal = FortWorld.Fire.transform.position;
                kind = DemoGoal.Pad;
                return;
            }
            else if (ShopBasics(out Vector3 shop))
            {
                goal = shop;
                kind = DemoGoal.Coin;
                return;
            }
            else if (!hutDone)
                pad = FortWorld.Hut;

            if (pad != null)
            {
                HaulWood(pad);
                return;
            }

            if (HaulRepair(config.demo.repairBelowFraction))
                return;

            if (NeedsSap())
            {
                goal = FortWorld.TurretUpgrade.transform.position;
                kind = DemoGoal.Pad;
                return;
            }

            BuildPad project = NextProject();
            if (project != null)
            {
                WorkProject(project);
                return;
            }

            if (CraftVisit())
                return;

            if (ResearchVisit())
                return;

            if (ShouldAssignStone() && FortWorld.StoneAssign != null)
            {
                goal = FortWorld.StoneAssign.transform.position;
                kind = DemoGoal.Coin;
                return;
            }

            if (TryUpgradeGoal(out Vector3 upgrade))
            {
                goal = upgrade;
                kind = DemoGoal.Coin;
                return;
            }

            if (FortWorld.Trader != null && FortWorld.Progress != null && FortWorld.Progress.HasSurplus())
            {
                goal = FortWorld.Trader.position;
                kind = DemoGoal.Pad;
                return;
            }

            if (FortWorld.Workers.Count > 0)
            {
                FollowWorker();
                return;
            }

            goal = config.layout.watchPosition;
            kind = DemoGoal.Watch;
        }

        bool HaulRepair(float fraction)
        {
            BuildPad pad = LowestStructure(fraction);
            if (pad == null)
                return false;
            HaulWood(pad);
            return true;
        }

        void HaulWood(BuildPad pad)
        {
            bool treeAvailable = TryNearestTree(out TreeNode tree);
            bool carrying = FortWorld.Stack != null && FortWorld.Stack.Count > 0;
            bool full = FortWorld.Stack != null && FortWorld.Stack.IsFull;
            bool enough = pad != null && FortWorld.Stack != null && pad.WoodRemaining > 0 && FortWorld.Stack.Count >= pad.WoodRemaining;
            if (!carrying || pad == null)
                emptying = false;
            else if (full || !treeAvailable || enough)
                emptying = true;

            if (emptying && pad != null && carrying)
            {
                goal = pad.transform.position;
                kind = DemoGoal.Pad;
                return;
            }

            if (pad != null && FortWorld.Stack != null && FortWorld.Stack.HasRoom && treeAvailable)
            {
                goal = tree.transform.position;
                kind = DemoGoal.Tree;
                return;
            }

            goal = pad != null ? pad.transform.position : hero.position;
            kind = pad != null ? DemoGoal.Pad : DemoGoal.Watch;
        }

        void WorkProject(BuildPad pad)
        {
            int iron = FortWorld.Progress != null ? FortWorld.Progress.Iron : 0;
            int stone = FortWorld.Progress != null ? FortWorld.Progress.CarriedStone : 0;
            if (pad.WantsSpecial && !pad.WantsWood && !(pad.WantsStone && (FortWorld.Progress == null || FortWorld.Progress.CarriedStone <= 0)))
            {
                goal = pad.transform.position;
                kind = DemoGoal.Pad;
                return;
            }

            if (pad.WantsWood)
            {
                HaulWood(pad);
                return;
            }

            if (pad.WantsStone && stone <= 0 && TryNearestRock(out RockNode rock))
            {
                goal = rock.transform.position;
                kind = DemoGoal.Rock;
                return;
            }

            if ((pad.WantsStone && stone > 0) || (pad.WantsIron && iron > 0))
            {
                goal = pad.transform.position;
                kind = DemoGoal.Pad;
                return;
            }

            goal = FortWorld.Config.layout.watchPosition;
            kind = DemoGoal.Watch;
        }

        BuildPad NextProject()
        {
            BuildPad spike = FortWorld.Spike;
            BuildPad gate = FortWorld.Gate;
            BuildPad tower = FortWorld.Tower;
            BuildPad upgrade = FortWorld.TurretUpgrade;
            BuildPad wall = FortWorld.Wall;
            if (WantsBuild(spike))
                return spike;
            if (wall != null && wall.IsComplete && (wall.WantsStone || (wall.WantsWood && !wall.NeedsRepair) || wall.WantsIron))
                return wall;
            if (WantsBuild(gate))
                return gate;
            if (WantsBuild(tower))
                return tower;
            if (wall != null && wall.WantsIron)
                return wall;
            if (WantsBuild(upgrade))
                return upgrade;
            return null;
        }

        static bool WantsBuild(BuildPad pad)
        {
            return pad != null && (pad.WantsWood || pad.WantsStone || pad.WantsIron || pad.WantsSpecial);
        }

        bool ShopBasics(out Vector3 point)
        {
            point = Vector3.zero;
            FortProgress progress = FortWorld.Progress;
            if (progress == null || ! (FortWorld.TurretPad != null && FortWorld.TurretPad.IsComplete))
                return false;
            FortIdleConfig config = FortWorld.Config;
            if (FortWorld.MeleePad != null && progress.CanSharpen && progress.Bones >= config.heroCombat.sharpenBoneCost)
            {
                point = FortWorld.MeleePad.transform.position;
                return true;
            }

            if (FortWorld.MeleePad != null
                && progress.MeleeLevel < config.demo.meleeLevelsToBuy
                && progress.CanUpgradeMelee
                && progress.Coins >= progress.NextMeleeCost)
            {
                point = FortWorld.MeleePad.transform.position;
                return true;
            }

            if (FortWorld.StackPad != null && ((progress.PackLevel < config.demo.packLevelsToBuy && progress.CanUpgradePack && progress.Hide >= progress.NextPackHide)
                || (progress.ArmorLevel < config.demo.armorLevelsToBuy && progress.CanUpgradeArmor && progress.Hide >= progress.NextArmorHide)))
            {
                point = FortWorld.StackPad.transform.position;
                return true;
            }

            bool hutReady = FortWorld.Hut != null && FortWorld.Hut.IsComplete;
            if (hutReady && FortWorld.HirePad != null && progress.WorkersHired < config.demo.workersToHire && progress.CanHire && progress.Coins >= progress.NextHireCost)
            {
                point = FortWorld.HirePad.transform.position;
                return true;
            }

            return false;
        }

        bool MealReady()
        {
            FortProgress progress = FortWorld.Progress;
            return FortWorld.Fire != null
                && progress != null
                && !progress.HeartyMeal
                && progress.Meat >= FortWorld.Config.drops.dawnMealMeat;
        }

        bool NeedsSap()
        {
            return FortWorld.TurretUpgrade != null && FortWorld.TurretUpgrade.IsComplete && FortWorld.TurretUpgrade.WantsSpecial;
        }

        bool CraftVisit()
        {
            if (FortWorld.Fire == null || FortWorld.Progress == null)
                return false;
            FortProgress progress = FortWorld.Progress;
            bool broth = !progress.CanSharpen
                && progress.Bones >= FortWorld.Config.drops.brothBones
                && progress.BrothStacks < FortWorld.Config.drops.brothMax;
            bool upgrade = FortWorld.Fire.WantsWood || FortWorld.Fire.WantsStone || FortWorld.Fire.WantsIron;
            bool extra = progress.CanSapWick || progress.HasSpareCore;
            if (!broth && !upgrade && !extra && progress.HeartyMeal)
                return false;
            if (!broth && !upgrade && !extra)
                return false;
            if (upgrade && FortWorld.Fire.WantsStone && progress.CarriedStone <= 0 && TryNearestRock(out RockNode rock))
            {
                goal = rock.transform.position;
                kind = DemoGoal.Rock;
                return true;
            }

            if (upgrade && FortWorld.Fire.WantsWood && (FortWorld.Stack == null || FortWorld.Stack.Count == 0) && TryNearestTree(out TreeNode tree))
            {
                goal = tree.transform.position;
                kind = DemoGoal.Tree;
                return true;
            }

            goal = FortWorld.Fire.transform.position;
            kind = DemoGoal.Pad;
            return true;
        }

        bool ResearchVisit()
        {
            if (FortWorld.Progress == null || !FortWorld.Progress.CanResearch)
                return false;
            goal = FortWorld.Config.layout.lockedAreaPosition;
            kind = DemoGoal.Pad;
            return true;
        }

        BuildPad LowestStructure(float fraction)
        {
            BuildPad found = null;
            float worst = fraction;
            Consider(FortWorld.Wall, ref found, ref worst);
            Consider(FortWorld.Gate, ref found, ref worst);
            Consider(FortWorld.Tower, ref found, ref worst);
            Consider(FortWorld.TurretPad, ref found, ref worst);
            return found;
        }

        static void Consider(BuildPad pad, ref BuildPad found, ref float worst)
        {
            if (pad == null || !pad.NeedsRepair || pad.MaxHitPoints <= 0f)
                return;
            float health = pad.HitPoints / pad.MaxHitPoints;
            if (health >= worst)
                return;
            worst = health;
            found = pad;
        }

        bool ShouldAssignStone()
        {
            if (FortWorld.Workers.Count < 2 || WorkerAgent.Any(WorkerAgent.Duty.Stone))
                return false;
            int night = FortWorld.Match == null ? 1 : FortWorld.Match.NightIndex;
            if (night < FortWorld.Config.supplies.rockUnlockNight)
                return false;
            return WantsBuild(FortWorld.Wall) || WantsBuild(FortWorld.Tower);
        }

        bool TryNearestRock(out RockNode best)
        {
            best = null;
            float bestDistance = float.PositiveInfinity;
            for (int i = 0; i < FortWorld.Rocks.Count; i++)
            {
                RockNode rock = FortWorld.Rocks[i];
                if (!rock.Available)
                    continue;
                float distance = FortMath.HorizontalDistance(hero.position, rock.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = rock;
                }
            }

            return best != null;
        }

        bool TryUpgradeGoal(out Vector3 point)
        {
            point = Vector3.zero;
            FortProgress progress = FortWorld.Progress;
            if (progress == null)
                return false;

            FortIdleConfig config = FortWorld.Config;
            if (progress.MeleeLevel < config.demo.meleeLevelsToBuy
                && progress.CanUpgradeMelee
                && progress.Coins >= progress.NextMeleeCost
                && FortWorld.MeleePad != null)
            {
                point = FortWorld.MeleePad.transform.position;
                return true;
            }

            if (progress.CanSharpen && progress.Bones >= config.heroCombat.sharpenBoneCost && FortWorld.MeleePad != null)
            {
                point = FortWorld.MeleePad.transform.position;
                return true;
            }

            if (FortWorld.StackPad != null && ((progress.PackLevel < config.demo.packLevelsToBuy && progress.CanUpgradePack && progress.Hide >= progress.NextPackHide)
                || (progress.ArmorLevel < config.demo.armorLevelsToBuy && progress.CanUpgradeArmor && progress.Hide >= progress.NextArmorHide)
                || (progress.CanPatch && progress.Hide >= config.heroCombat.patchHideCost)))
            {
                point = FortWorld.StackPad.transform.position;
                return true;
            }

            bool hutReady = FortWorld.Hut != null && FortWorld.Hut.IsComplete;
            if (hutReady
                && progress.WorkersHired < config.demo.workersToHire
                && progress.CanHire
                && progress.Coins >= progress.NextHireCost
                && FortWorld.HirePad != null)
            {
                point = FortWorld.HirePad.transform.position;
                return true;
            }

            if (progress.StackLevel < config.demo.stackLevelsToBuy
                && progress.CanUpgradeStack
                && progress.Coins >= progress.NextStackCost
                && FortWorld.StackPad != null)
            {
                point = FortWorld.StackPad.transform.position;
                return true;
            }

            if (progress.SpeedLevel < config.demo.speedLevelsToBuy
                && progress.CanUpgradeSpeed
                && progress.Coins >= progress.NextSpeedCost
                && FortWorld.SpeedPad != null)
            {
                point = FortWorld.SpeedPad.transform.position;
                return true;
            }

            return false;
        }

        void FollowWorker()
        {
            WorkerAgent worker = FortWorld.Workers[0];
            Vector3 focus = worker.transform.position;
            Vector3 from = focus - hero.position;
            from.y = 0f;
            float gap = FortWorld.Config.demo.workerFollowGap;
            if (from.magnitude <= gap)
            {
                goal = hero.position;
                kind = DemoGoal.Watch;
                return;
            }

            goal = focus - from.normalized * gap;
            kind = DemoGoal.Watch;
        }

        void TickShowcase()
        {
            if (loggedShowcase || FortWorld.Progress == null)
                return;
            FortIdleConfig config = FortWorld.Config;
            FortProgress progress = FortWorld.Progress;
            if (progress.WorkersHired < config.demo.workersToHire)
                return;
            if (progress.Stockpile <= 0)
                return;
            loggedShowcase = true;
            Debug.Log("FORTIDLE showcase=done coins=" + progress.Coins
                + " cap=" + progress.StackCapacity
                + " speed=" + progress.MoveSpeed.ToString("0.00")
                + " crew=" + FortWorld.Workers.Count
                + " stock=" + progress.Stockpile);
        }

        float ArriveDistance()
        {
            FortIdleConfig config = FortWorld.Config;
            if (kind == DemoGoal.Tree)
                return config.chop.range * config.demo.chopRangeFactor;
            if (kind == DemoGoal.Rock)
                return config.supplies.mineRange * config.demo.chopRangeFactor;
            if (kind == DemoGoal.Pad || kind == DemoGoal.Coin)
                return config.pour.padRadius * config.demo.padRangeFactor;
            return config.demo.watchArrive;
        }

        bool TryNearestTree(out TreeNode best)
        {
            best = null;
            float bestDistance = float.PositiveInfinity;
            for (int i = 0; i < FortWorld.Trees.Count; i++)
            {
                TreeNode tree = FortWorld.Trees[i];
                if (!tree.Standing || !Reachable(tree))
                    continue;
                float distance = FortMath.HorizontalDistance(hero.position, tree.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = tree;
                }
            }

            return best != null;
        }

        bool Reachable(TreeNode tree)
        {
            for (int i = 0; i < FortWorld.Pads.Count; i++)
            {
                if (FortWorld.Pads[i].BlocksSegment(hero.position, tree.transform.position, 0f))
                    return false;
            }

            return true;
        }

        static bool HasFlag(string flag)
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
