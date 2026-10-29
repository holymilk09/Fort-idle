using UnityEngine;

namespace FortIdle
{
    public class NightPlan
    {
        public int Night;
        public float Power;
        public float BaseBudget;
        public float Threat;
        public float Band;
        public float Ratio;
        public int Shamblers;
        public int Brutes;
        public int Bosses;
        public float ShamblerHp;
        public float BruteHp;
        public float BossHp;
        public float Size = 1f;
        public bool BossNight;
        public float Hpx = 1f;
        public float EliteChance;
        public int Tier = 1;
        public int BruteTier = 1;
    }

    public static class ThreatMath
    {
        public static float FortPower()
        {
            FortIdleConfig config = FortWorld.Config;
            if (config == null)
                return 0f;
            FortIdleConfig.ProgressionTuning tune = config.progression;
            float dps = HeroDps() + DefenseDps();
            float hp = FortHp();
            float root = Mathf.Sqrt(1f + hp / Mathf.Max(1f, tune.fortHpRef));
            return tune.powerEta * dps * tune.nightSeconds * root;
        }

        public static float HeroDps()
        {
            FortIdleConfig config = FortWorld.Config;
            if (config == null || FortWorld.Progress == null)
                return 0f;
            float interval = config.heroCombat.swingInterval;
            if (interval < 0.05f)
                interval = 0.5f;
            return FortWorld.Progress.SwingDamage / interval;
        }

        public static float DefenseDps()
        {
            float sum = 0f;
            sum += PadDps(FortWorld.TurretPad);
            sum += PadDps(FortWorld.Tower);
            sum += PadDps(FortWorld.Spike);
            return sum;
        }

        static float PadDps(BuildPad pad)
        {
            return pad == null ? 0f : pad.ListedDps;
        }

        public static float FortHp()
        {
            float hp = 0f;
            if (FortWorld.Wall != null && FortWorld.Wall.IsComplete)
                hp += FortWorld.Wall.HitPoints;
            if (FortWorld.Gate != null && FortWorld.Gate.IsComplete)
                hp += FortWorld.Gate.HitPoints;
            if (FortWorld.Fire != null)
                hp += FortWorld.Fire.HitPoints;
            return hp;
        }

        public static NightPlan Build(int night, int lossStreak)
        {
            FortIdleConfig.ProgressionTuning tune = FortWorld.Config.progression;
            NightPlan plan = new NightPlan();
            plan.Night = night < 1 ? 1 : night;
            plan.Power = FortPower();
            plan.BossNight = plan.Night % tune.bossEvery == 0;
            plan.BaseBudget = BaseBudget(plan.Night);
            float expected = plan.BaseBudget / TargetRatio(plan.Night);
            plan.Band = 1f;
            if (plan.Night > tune.rubberOffUntil && expected > 1f)
            {
                float raw = Mathf.Pow(plan.Power / expected, tune.rubberAlpha);
                plan.Band = Mathf.Clamp(raw, tune.rubberMin, tune.rubberMax);
            }

            float valve = lossStreak >= 2 ? tune.lossValve : 1f;
            plan.Threat = plan.BaseBudget * plan.Band * valve;
            plan.Ratio = plan.Power <= 1f ? 99f : plan.Threat / plan.Power;
            FillRoster(plan);
            FillElites(plan);
            return plan;
        }

        public static float BaseBudget(int night)
        {
            FortIdleConfig.ProgressionTuning tune = FortWorld.Config.progression;
            FortIdleConfig.EndlessTuning endless = FortPrestige.Endless;
            int index = (night - 1) % 5;
            if (index < 0)
                index = 0;
            if (night > endless.authoredNights && endless.authoredNights > 0)
            {
                int maps = Mathf.Max(1, endless.authoredNights / 5);
                float top = tune.MapBudget(4) * Mathf.Pow(tune.mapGrowth, maps - 1);
                float budget = top * Mathf.Pow(endless.endlessGrowth, night - endless.authoredNights);
                NightKind kind = KindOf(night);
                if (kind == NightKind.BloodMoon)
                    budget *= endless.bloodMoonMult;
                else if (kind == NightKind.Rest)
                    budget *= endless.restMult;
                return budget;
            }

            int map = Mathf.Min(5, (night - 1) / 5 + 1);
            float seed = tune.MapBudget(index);
            float value = seed * Mathf.Pow(tune.mapGrowth, map - 1);
            if (map > 1 && endless.carryFloor > 0f)
            {
                float lastBoss = tune.MapBudget(4) * Mathf.Pow(tune.mapGrowth, map - 2);
                value = Mathf.Max(value, endless.carryFloor * lastBoss * Mathf.Pow(endless.carryStep, index));
            }

            return value;
        }

        public static NightKind KindOf(int night)
        {
            FortIdleConfig.EndlessTuning endless = FortPrestige.Endless;
            int every = endless.bloodMoonEvery;
            int past = night - endless.authoredNights;
            if (every > 0 && past > 0)
            {
                if (past % every == 0)
                    return NightKind.BloodMoon;
                if (past > 1 && (past - 1) % every == 0)
                    return NightKind.Rest;
            }

            int boss = FortWorld.Config.progression.bossEvery;
            return boss > 0 && night % boss == 0 ? NightKind.Boss : NightKind.Normal;
        }

        static float TargetRatio(int night)
        {
            FortIdleConfig.ProgressionTuning tune = FortWorld.Config.progression;
            FortIdleConfig.EndlessTuning endless = FortPrestige.Endless;
            NightKind kind = KindOf(night);
            if (kind == NightKind.BloodMoon)
                return endless.targetBlood;
            if (kind == NightKind.Rest)
                return endless.targetRest;
            return kind == NightKind.Boss ? tune.targetBoss : tune.targetNormal;
        }

        public static float KillCoinScale()
        {
            MatchDirector match = FortWorld.Match;
            int night = match != null ? match.NightIndex : 1;
            FortIdleConfig.EndlessTuning endless = FortPrestige.Endless;
            float scale = 1f;
            if (endless.decayStart > 0 && night >= endless.decayStart)
            {
                int step = (night - endless.decayStart) / Mathf.Max(1, endless.decaySpan);
                scale = step <= 0 ? endless.decayFirst : step == 1 ? endless.decaySecond : endless.decayLast;
            }

            if (FortWorld.Config != null && KindOf(night) == NightKind.BloodMoon)
                scale *= endless.bloodMoonLoot;
            return scale * FortPrestige.IncomeMult();
        }

        static void FillRoster(NightPlan plan)
        {
            FortIdleConfig config = FortWorld.Config;
            FortIdleConfig.ProgressionTuning tune = config.progression;
            float shamblerBase = config.monsters.hitPoints;
            float bruteBase = config.monsters.bruteHitPoints;
            float bossBase = config.monsters.treantHitPoints;
            if (shamblerBase < 1f)
                shamblerBase = 60f;
            if (bruteBase < 1f)
                bruteBase = 600f;
            if (bossBase < 1f)
                bossBase = 3000f;

            if (plan.Night <= 5)
            {
                Authored(plan.Night, out plan.Shamblers, out plan.Brutes, out plan.Bosses);
                float authored = plan.Shamblers * shamblerBase + plan.Brutes * bruteBase + plan.Bosses * bossBase;
                float scale = authored <= 1f ? 1f : plan.Threat / authored;
                plan.ShamblerHp = shamblerBase * scale;
                plan.BruteHp = bruteBase * scale;
                plan.BossHp = plan.Bosses > 0 ? bossBase * scale : 0f;
                plan.Size = Mathf.Sqrt(Mathf.Max(1f, scale));
                return;
            }

            float bossBudget = plan.BossNight ? plan.Threat * tune.bossShare : 0f;
            float rest = plan.Threat - bossBudget;
            float share = 0f;
            if (plan.Night >= 3)
                share = Mathf.Min(tune.bruteShareMax, tune.bruteShareBase + tune.bruteSharePerNight * (plan.Night - 3));
            plan.Brutes = Mathf.FloorToInt(rest * share / bruteBase);
            if (plan.Brutes < 0)
                plan.Brutes = 0;
            float fodder = rest - plan.Brutes * bruteBase;
            if (fodder < 0f)
                fodder = 0f;
            plan.Shamblers = Mathf.FloorToInt(fodder / shamblerBase);
            plan.Bosses = plan.BossNight ? 1 : 0;
            float hpMult = 1f;
            int bodies = plan.Shamblers + plan.Brutes + plan.Bosses;
            if (bodies > tune.countCap)
            {
                int room = tune.countCap - plan.Brutes - plan.Bosses;
                if (room < 1)
                    room = 1;
                plan.Shamblers = room;
                hpMult = fodder / (plan.Shamblers * shamblerBase);
                if (hpMult < 1f)
                    hpMult = 1f;
            }

            plan.ShamblerHp = shamblerBase * hpMult;
            plan.BruteHp = bruteBase;
            plan.BossHp = plan.Bosses > 0 ? bossBudget : 0f;
            plan.Size = Mathf.Sqrt(Mathf.Max(1f, hpMult));
        }

        static void FillElites(NightPlan plan)
        {
            float shamblerBase = FortWorld.Config.monsters.hitPoints;
            if (shamblerBase < 1f)
                shamblerBase = 60f;
            plan.Hpx = plan.ShamblerHp > 0f ? plan.ShamblerHp / shamblerBase : 1f;
            plan.EliteChance = MonsterAffix.EliteChance(plan.Night);
            plan.Tier = MonsterAffix.FodderTier(plan.Hpx, plan.Threat);
            plan.BruteTier = MonsterAffix.ThreatTier(plan.Threat);
        }

        public static void Authored(int night, out int shamblers, out int brutes, out int bosses)
        {
            shamblers = 12;
            brutes = 0;
            bosses = 0;
            if (night == 2)
                shamblers = 20;
            else if (night == 3)
            {
                shamblers = 30;
                brutes = 1;
            }
            else if (night == 4)
            {
                shamblers = 40;
                brutes = 2;
            }
            else if (night >= 5)
            {
                shamblers = 25;
                bosses = 1;
            }
        }

        public static int Cost(float baseCost, float growth, int level)
        {
            if (level < 0)
                level = 0;
            float value = baseCost * Mathf.Pow(growth, level);
            int cost = Mathf.RoundToInt(value);
            if (cost < 1)
                cost = 1;
            return cost;
        }

        public static int Bulk(float baseCost, float growth, int level, int count)
        {
            if (count <= 0)
                return 0;
            if (Mathf.Abs(growth - 1f) < 0.0001f)
                return Cost(baseCost, growth, level) * count;
            float sum = baseCost * Mathf.Pow(growth, level) * (Mathf.Pow(growth, count) - 1f) / (growth - 1f);
            int cost = Mathf.RoundToInt(sum);
            if (cost < 1)
                cost = 1;
            return cost;
        }

        public static int MaxBuy(float baseCost, float growth, int level, int money, int room)
        {
            int bought = 0;
            int guard = room < 0 ? 0 : room;
            while (bought < guard)
            {
                int next = Bulk(baseCost, growth, level, bought + 1);
                if (next > money)
                    break;
                bought += 1;
            }

            return bought;
        }

        public static string Scarce()
        {
            FortProgress progress = FortWorld.Progress;
            if (FortWorld.Wall != null && !FortWorld.Wall.IsComplete)
                return "Wood";
            if (FortWorld.TurretPad != null && !FortWorld.TurretPad.IsComplete)
                return "Wood";
            if (progress != null && !progress.HeartyMeal && progress.Meat < FortWorld.Config.drops.dawnMealMeat)
                return "Meat";
            if (FortWorld.Wall != null && FortWorld.Wall.WantsStone)
                return "Stone";
            if ((FortWorld.Wall != null && FortWorld.Wall.WantsIron)
                || (FortWorld.Gate != null && FortWorld.Gate.WantsIron)
                || (FortWorld.Fire != null && FortWorld.Fire.WantsIron))
                return "Iron";
            if (FortWorld.Spike != null && FortWorld.Spike.WantsSpecial && progress != null && progress.Bones < 8)
                return "Bones";
            if (progress != null && progress.NextResearchCost > progress.Cores && progress.NextResearchCost > 0)
                return "Cores";
            if (FortWorld.TurretWeapon != null && FortWorld.TurretWeapon.Level < 3 && progress != null && progress.Sap < FortWorld.Config.defenses.turretLv3Sap)
                return "Sap";
            if (progress != null && progress.Coins < progress.NextMeleeCost)
                return "Coins";
            if (progress != null && progress.Hide < progress.NextArmorHide && progress.CanUpgradeArmor)
                return "Hide";
            return "Wood";
        }
    }
}
