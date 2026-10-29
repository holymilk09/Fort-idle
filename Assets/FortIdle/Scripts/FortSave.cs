using System;
using UnityEngine;

namespace FortIdle
{
    public static class FortSave
    {
        public const string PlayerKey = "FortIdle.Map1";
        public const string DemoKey = "FortIdle.DemoCheck";

        public static bool Restoring;
        public static int PendingNight;
        public static long SavedTicks;
        static Blob pending;

        [Serializable]
        public class Blob
        {
            public int coins;
            public int iron;
            public int meat;
            public int bones;
            public int hide;
            public int cores;
            public int sap;
            public int stack;
            public int speed;
            public int melee;
            public int armor;
            public int pack;
            public int patch;
            public int sharpen;
            public int workers;
            public int broth;
            public int research;
            public int coreFocus;
            public int sapOil;
            public int meal;
            public int mealActive;
            public int spike;
            public int wallTier;
            public int gateTier;
            public int towerTier;
            public int turretLevel;
            public int fireLevel;
            public int wallBuilt;
            public int gateBuilt;
            public int towerBuilt;
            public int turretBuilt;
            public int turretUp;
            public float wallHp;
            public float gateHp;
            public float towerHp;
            public float turretHp;
            public float fireHp;
            public int night;
            public long savedTicks;
            public Site wall = new Site();
            public Site gate = new Site();
            public Site tower = new Site();
            public Site turret = new Site();
            public Site spikeSite = new Site();
            public Site hut = new Site();
            public Site fire = new Site();
            public Site turretUpSite = new Site();
        }

        [Serializable]
        public class Site
        {
            public int wood;
            public int stone;
            public int iron;
            public int tierWood;
            public int tierStone;
            public int tierIron;
        }

        public static bool DemoSession()
        {
            return HasFlag("-demo") || (FortWorld.Config != null && FortWorld.Config.demo.runByDefault);
        }

        public static void LoadIntoWorld()
        {
            PendingNight = 0;
            SavedTicks = 0;
            pending = null;
            if (DemoSession() || HasFlag("-elements") || StageShowcase.Requested() || LookReel.Requested() || PerfHarness.Requested() || TreeShots.Requested() || QaShot.Requested() || PoseShot.Requested())
                return;
            Blob blob = Read(PlayerKey);
            if (blob == null || FortWorld.Progress == null)
                return;
            Restoring = true;
            FortWorld.Progress.ApplyBlob(blob);
            Restoring = false;
            pending = blob;
            SavedTicks = blob.savedTicks;
            PendingNight = blob.night < 1 ? 1 : blob.night;
            Debug.Log("FORTIDLE save=loaded night=" + PendingNight + " coins=" + blob.coins);
        }

        public static void ApplyStructures()
        {
            if (pending == null)
                return;
            Restoring = true;
            if (FortWorld.Wall != null)
            {
                FortWorld.Wall.Restore(pending.wallTier, pending.wallHp, pending.wallBuilt != 0);
                FortWorld.Wall.ReadSite(pending.wall);
            }

            if (FortWorld.Gate != null)
            {
                FortWorld.Gate.Restore(pending.gateTier, pending.gateHp, pending.gateBuilt != 0);
                FortWorld.Gate.ReadSite(pending.gate);
            }

            if (FortWorld.Tower != null)
            {
                FortWorld.Tower.Restore(pending.towerTier, pending.towerHp, pending.towerBuilt != 0);
                FortWorld.Tower.ReadSite(pending.tower);
            }

            if (FortWorld.TurretPad != null)
            {
                FortWorld.TurretPad.Restore(0, pending.turretHp, pending.turretBuilt != 0);
                FortWorld.TurretPad.ReadSite(pending.turret);
            }

            if (FortWorld.TurretUpgrade != null && pending.turretUp != 0)
                FortWorld.TurretUpgrade.Restore(0, -1f, true);
            if (FortWorld.TurretUpgrade != null)
                FortWorld.TurretUpgrade.ReadSite(pending.turretUpSite);
            if (FortWorld.TurretWeapon != null && pending.turretLevel > 1)
                FortWorld.TurretWeapon.ApplyLevel(pending.turretLevel);
            if (FortWorld.Spike != null)
            {
                bool partialSpike = pending.spike <= 0
                    && pending.spikeSite != null
                    && pending.spikeSite.wood > 0
                    && pending.spikeSite.wood < (FortWorld.Config != null ? FortWorld.Config.defenses.spikeWood : 18);
                if (partialSpike)
                    FortWorld.Spike.ReadSite(pending.spikeSite);
                else
                    FortWorld.Spike.RestoreSpike(pending.spike);
            }

            if (FortWorld.Fire != null)
            {
                FortWorld.Fire.Restore(pending.fireLevel, pending.fireHp);
                FortWorld.Fire.ReadSite(pending.fire);
            }

            if (FortWorld.Hut != null)
                FortWorld.Hut.ReadSite(pending.hut);
            Restoring = false;
            pending = null;
        }

        public static void WritePlayer()
        {
            if (Restoring || DemoSession() || FortWorld.Progress == null)
                return;
            Write(PlayerKey);
        }

        public static void ProveRoundtrip()
        {
            Write(DemoKey);
            Blob blob = Read(DemoKey);
            FortProgress progress = FortWorld.Progress;
            bool ok = blob != null && progress != null
                && blob.coins == progress.Coins
                && blob.meat == progress.Meat
                && blob.bones == progress.Bones
                && blob.hide == progress.Hide
                && blob.cores == progress.Cores
                && blob.sap == progress.Sap
                && blob.melee == progress.MeleeLevel
                && blob.research == progress.ResearchRank
                && blob.night == (FortWorld.Match != null ? FortWorld.Match.NightIndex : blob.night)
                && blob.wall != null
                && FortWorld.Wall != null
                && blob.wall.wood == FortWorld.Wall.PouredWood
                && blob.wall.tierWood == FortWorld.Wall.PouredTierWood;
            Debug.Log(ok ? "FORTIDLE save=roundtrip ok" : "FORTIDLE save=roundtrip fail");
        }

        public static Blob Capture()
        {
            FortProgress progress = FortWorld.Progress;
            Blob blob = new Blob();
            if (progress == null)
                return blob;
            blob.coins = progress.Coins;
            blob.iron = progress.Iron;
            blob.meat = progress.Meat;
            blob.bones = progress.Bones;
            blob.hide = progress.Hide;
            blob.cores = progress.Cores;
            blob.sap = progress.Sap;
            blob.stack = progress.StackLevel;
            blob.speed = progress.SpeedLevel;
            blob.melee = progress.MeleeLevel;
            blob.armor = progress.ArmorLevel;
            blob.pack = progress.PackLevel;
            blob.patch = progress.PatchLevel;
            blob.sharpen = progress.SharpenLevel;
            blob.workers = progress.WorkersHired;
            blob.broth = progress.BrothStacks;
            blob.research = progress.ResearchRank;
            blob.coreFocus = progress.CoreFocus;
            blob.sapOil = progress.SapOil;
            blob.meal = progress.HeartyMeal ? 1 : 0;
            blob.mealActive = progress.MealActive ? 1 : 0;
            blob.spike = FortWorld.Spike != null ? FortWorld.Spike.Tier : 0;
            blob.wallTier = FortWorld.Wall != null ? FortWorld.Wall.Tier : 0;
            blob.gateTier = FortWorld.Gate != null ? FortWorld.Gate.Tier : 0;
            blob.towerTier = FortWorld.Tower != null ? FortWorld.Tower.Tier : 0;
            blob.turretLevel = FortWorld.TurretWeapon != null ? FortWorld.TurretWeapon.Level : 1;
            blob.fireLevel = FortWorld.Fire != null ? FortWorld.Fire.Level : 1;
            blob.wallBuilt = FortWorld.Wall != null && FortWorld.Wall.IsComplete ? 1 : 0;
            blob.gateBuilt = FortWorld.Gate != null && FortWorld.Gate.IsComplete ? 1 : 0;
            blob.towerBuilt = FortWorld.Tower != null && FortWorld.Tower.IsComplete ? 1 : 0;
            blob.turretBuilt = FortWorld.TurretPad != null && FortWorld.TurretPad.IsComplete ? 1 : 0;
            blob.turretUp = FortWorld.TurretUpgrade != null && FortWorld.TurretUpgrade.IsComplete ? 1 : 0;
            blob.wallHp = FortWorld.Wall != null ? FortWorld.Wall.HitPoints : 0f;
            blob.gateHp = FortWorld.Gate != null ? FortWorld.Gate.HitPoints : 0f;
            blob.towerHp = FortWorld.Tower != null ? FortWorld.Tower.HitPoints : 0f;
            blob.turretHp = FortWorld.TurretPad != null ? FortWorld.TurretPad.HitPoints : 0f;
            blob.fireHp = FortWorld.Fire != null ? FortWorld.Fire.HitPoints : 0f;
            blob.night = FortWorld.Match != null ? FortWorld.Match.NightIndex : 1;
            blob.savedTicks = DateTime.UtcNow.Ticks;
            WritePad(blob.wall, FortWorld.Wall);
            WritePad(blob.gate, FortWorld.Gate);
            WritePad(blob.tower, FortWorld.Tower);
            WritePad(blob.turret, FortWorld.TurretPad);
            WritePad(blob.spikeSite, FortWorld.Spike);
            WritePad(blob.hut, FortWorld.Hut);
            WritePad(blob.turretUpSite, FortWorld.TurretUpgrade);
            if (FortWorld.Fire != null && blob.fire != null)
                FortWorld.Fire.WriteSite(blob.fire);
            return blob;
        }

        static void WritePad(Site site, BuildPad pad)
        {
            if (site == null || pad == null)
                return;
            pad.WriteSite(site);
        }

        static void Write(string key)
        {
            string json = JsonUtility.ToJson(Capture());
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        static Blob Read(string key)
        {
            if (!PlayerPrefs.HasKey(key))
                return null;
            string json = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(json))
                return null;
            return JsonUtility.FromJson<Blob>(json);
        }

        static bool HasFlag(string flag)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == flag)
                    return true;
            }

            return false;
        }
    }
}
