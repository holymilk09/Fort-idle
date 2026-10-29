using System.Globalization;
using UnityEngine;

namespace FortIdle
{
    public class FortProgress : MonoBehaviour
    {
        public int Coins { get; private set; }
        public int Iron { get; private set; }
        public int CarriedStone { get; private set; }
        public int StackLevel { get; private set; }
        public int SpeedLevel { get; private set; }
        public int WorkersHired { get; private set; }
        public int Stockpile { get; private set; }
        public int Meat { get; private set; }
        public int Bones { get; private set; }
        public int Hide { get; private set; }
        public int Cores { get; private set; }
        public int Sap { get; private set; }
        public int MeleeLevel { get; private set; }
        public int ArmorLevel { get; private set; }
        public int PackLevel { get; private set; }
        public int PatchLevel { get; private set; }
        public int SharpenLevel { get; private set; }
        public int BrothStacks { get; private set; }
        public int ResearchRank { get; private set; }
        public int CoreFocus { get; private set; }
        public int SapOil { get; private set; }
        public int WorkerWood { get; private set; }
        public int WorkerCoinSpent { get; private set; }
        public bool HeartyMeal { get; private set; }
        public bool MealActive { get; private set; }
        public float WorkRate { get; private set; } = 1f;
        public string DawnLine { get; private set; } = "";
        public int DawnSerial { get; private set; }

        const int Kinds = 9;
        const int Days = 16;
        readonly int[,] earned = new int[Kinds, Days];
        readonly int[,] spent = new int[Kinds, Days];

        void Awake()
        {
            FortWorld.Progress = this;
        }

        public int StackCapacity
        {
            get
            {
                FortIdleConfig config = FortWorld.Config;
                if (config == null)
                    return 0;
                return config.upgrades.CapacityForLevel(StackLevel) + PackLevel * config.heroCombat.packCapacity;
            }
        }

        public float MoveSpeed
        {
            get
            {
                FortIdleConfig config = FortWorld.Config;
                if (config == null)
                    return 0f;
                float bonus = config.upgrades.speedBonusPerLevel;
                float speed = config.player.moveSpeed * Mathf.Pow(1f + bonus, SpeedLevel);
                return Mathf.Min(config.upgrades.speedCap, speed);
            }
        }

        public float SwingDamage
        {
            get
            {
                FortIdleConfig config = FortWorld.Config;
                if (config == null)
                    return 0f;
                FortIdleConfig.ProgressionTuning tune = config.progression;
                float steps = 1f + tune.heroDpsPerLevel * MeleeLevel;
                int marks = tune.heroMilestoneEvery > 0 ? MeleeLevel / tune.heroMilestoneEvery : 0;
                float milestone = Mathf.Pow(tune.heroMilestoneMult, marks);
                float focus = 1f + CoreFocus * tune.coreFocusBonus;
                return config.heroCombat.swingDamage * steps * milestone * focus
                    + SharpenLevel * config.heroCombat.sharpenDamage;
            }
        }

        public bool CanUpgradeStack => FortWorld.Config != null && StackLevel < FortWorld.Config.progression.stackLevelCap;

        public bool CanUpgradeSpeed
        {
            get
            {
                FortIdleConfig config = FortWorld.Config;
                if (config == null)
                    return false;
                return SpeedLevel < config.progression.speedLevelCap && MoveSpeed < config.upgrades.speedCap;
            }
        }

        public bool CanHire => FortWorld.Config != null && WorkersHired < FortWorld.Config.upgrades.maxWorkers;

        public bool CanUpgradeMelee => FortWorld.Config != null && MeleeLevel < FortWorld.Config.progression.heroLevelCap;

        public bool CanUpgradeArmor => FortWorld.Config != null && ArmorLevel < FortWorld.Config.heroCombat.maxArmorLevel;

        public bool CanUpgradePack => FortWorld.Config != null && PackLevel < FortWorld.Config.heroCombat.maxPackLevel;

        public bool CanPatch => FortWorld.Config != null && PatchLevel < FortWorld.Config.heroCombat.maxPatches;

        public bool CanSharpen => FortWorld.Config != null && SharpenLevel < FortWorld.Config.heroCombat.maxSharpen;

        public bool CanResearch
        {
            get
            {
                int cost = NextResearchCost;
                return cost > 0 && Cores >= cost;
            }
        }

        public int NextStackCost => ThreatMath.Cost(FortWorld.Config.progression.heroBase, FortWorld.Config.progression.heroGrowth, StackLevel);

        public int NextSpeedCost => ThreatMath.Cost(FortWorld.Config.progression.heroBase, FortWorld.Config.progression.heroGrowth, SpeedLevel);

        public int NextHireCost => ThreatMath.Cost(FortWorld.Config.progression.workerBase, FortWorld.Config.progression.workerGrowth, WorkersHired);

        public int NextMeleeCost => ThreatMath.Cost(FortWorld.Config.progression.heroBase, FortWorld.Config.progression.heroGrowth, MeleeLevel);

        public int NextArmorHide => FortWorld.Config.heroCombat.armorHideCost + ArmorLevel * FortWorld.Config.heroCombat.armorHideStep;

        public int NextPackHide => FortWorld.Config.heroCombat.packHideCost + PackLevel * FortWorld.Config.heroCombat.packHideStep;

        public int NextResearchCost
        {
            get
            {
                FortIdleConfig.DropTuning drops = FortWorld.Config.drops;
                if (ResearchRank <= 0)
                    return drops.researchCost0;
                if (ResearchRank == 1)
                    return drops.researchCost1;
                if (ResearchRank == 2)
                    return drops.researchCost2;
                return 0;
            }
        }

        public bool StoneHasRoom
        {
            get
            {
                FortIdleConfig config = FortWorld.Config;
                if (config == null)
                    return false;
                return CarriedStone < config.supplies.heroStoneCapacity;
            }
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0)
                return;
            Coins += amount;
            Gain(6, amount);
        }

        public int AddStone(int amount)
        {
            FortIdleConfig config = FortWorld.Config;
            if (amount <= 0 || config == null)
                return 0;
            int space = config.supplies.heroStoneCapacity - CarriedStone;
            int gain = amount < space ? amount : space;
            if (gain < 0)
                gain = 0;
            CarriedStone += gain;
            Gain(8, gain);
            return gain;
        }

        public bool RemoveStone()
        {
            if (CarriedStone <= 0)
                return false;
            CarriedStone -= 1;
            Pay(8, 1);
            return true;
        }

        public void AddIron(int amount)
        {
            if (amount <= 0)
                return;
            Iron += amount;
            Gain(5, amount);
        }

        public bool SpendIron(int amount)
        {
            if (amount <= 0 || Iron < amount)
                return false;
            Iron -= amount;
            Pay(5, amount);
            return true;
        }

        public void AddMeat(int amount)
        {
            if (amount <= 0)
                return;
            Meat += amount;
            Gain(0, amount);
        }

        public void AddBones(int amount)
        {
            if (amount <= 0)
                return;
            Bones += amount;
            Gain(1, amount);
        }

        public void AddHide(int amount)
        {
            if (amount <= 0)
                return;
            Hide += amount;
            Gain(2, amount);
        }

        public void AddCores(int amount)
        {
            if (amount <= 0)
                return;
            Cores += amount;
            Gain(3, amount);
        }

        public void AddSap(int amount)
        {
            if (amount <= 0)
                return;
            Sap += amount;
            Gain(4, amount);
        }

        public bool SpendMeat(int amount)
        {
            if (amount <= 0 || Meat < amount)
                return false;
            Meat -= amount;
            Pay(0, amount);
            return true;
        }

        public bool SpendBones(int amount)
        {
            if (amount <= 0 || Bones < amount)
                return false;
            Bones -= amount;
            Pay(1, amount);
            return true;
        }

        public bool SpendHide(int amount)
        {
            if (amount <= 0 || Hide < amount)
                return false;
            Hide -= amount;
            Pay(2, amount);
            return true;
        }

        public bool SpendCores(int amount)
        {
            if (amount <= 0 || Cores < amount)
                return false;
            Cores -= amount;
            Pay(3, amount);
            return true;
        }

        public bool SpendSap(int amount)
        {
            if (amount <= 0 || Sap < amount)
                return false;
            Sap -= amount;
            Pay(4, amount);
            return true;
        }

        public void NoteWood(int amount, bool earnedWood)
        {
            if (amount <= 0)
                return;
            if (earnedWood)
                Gain(7, amount);
            else
                Pay(7, amount);
        }

        public void NoteStoneFound(int amount)
        {
            Gain(8, amount);
        }

        public void NoteStoneSpent(int amount)
        {
            Pay(8, amount);
        }

        public void AddStock(int amount)
        {
            if (amount <= 0)
                return;
            Stockpile += amount;
        }

        public bool TryBuyStack()
        {
            if (!CanUpgradeStack)
                return false;
            int cost = NextStackCost;
            if (!Spend(cost))
                return false;
            StackLevel += 1;
            NoteReward("stack-" + StackLevel);
            Debug.Log("FORTIDLE upgrade=stack level=" + StackLevel + " cap=" + StackCapacity + " cost=" + cost + " coins=" + Coins);
            return true;
        }

        public bool TryBuySpeed()
        {
            if (!CanUpgradeSpeed)
                return false;
            int cost = NextSpeedCost;
            if (!Spend(cost))
                return false;
            SpeedLevel += 1;
            NoteReward("speed-" + SpeedLevel);
            Debug.Log("FORTIDLE upgrade=speed level=" + SpeedLevel + " speed=" + MoveSpeed.ToString("0.00") + " cost=" + cost + " coins=" + Coins);
            return true;
        }

        public bool TryBuyMelee()
        {
            if (!CanUpgradeMelee)
                return false;
            int cost = NextMeleeCost;
            if (!Spend(cost))
                return false;
            MeleeLevel += 1;
            NoteReward("melee-" + MeleeLevel);
            Debug.Log("FORTIDLE upgrade=melee level=" + MeleeLevel + " dmg=" + SwingDamage.ToString("0") + " cost=" + cost);
            return true;
        }

        public int BuyMelee(int count)
        {
            return BuyCoinLevels(count, CanUpgradeMelee, FortWorld.Config.progression.heroBase, FortWorld.Config.progression.heroGrowth, MeleeLevel, FortWorld.Config.progression.heroLevelCap - MeleeLevel, TryBuyMelee);
        }

        public int BuyStack(int count)
        {
            return BuyCoinLevels(count, CanUpgradeStack, FortWorld.Config.progression.heroBase, FortWorld.Config.progression.heroGrowth, StackLevel, FortWorld.Config.progression.stackLevelCap - StackLevel, TryBuyStack);
        }

        public int BuySpeed(int count)
        {
            return BuyCoinLevels(count, CanUpgradeSpeed, FortWorld.Config.progression.heroBase, FortWorld.Config.progression.heroGrowth, SpeedLevel, FortWorld.Config.progression.speedLevelCap - SpeedLevel, TryBuySpeed);
        }

        public int BuyHire(int count)
        {
            int room = FortWorld.Config.upgrades.maxWorkers - WorkersHired;
            return BuyCoinLevels(count, CanHire, FortWorld.Config.progression.workerBase, FortWorld.Config.progression.workerGrowth, WorkersHired, room, TryHire);
        }

        int BuyCoinLevels(int count, bool can, float baseCost, float growth, int level, int room, System.Func<bool> buy)
        {
            if (!can || room <= 0)
                return 0;
            int qty = count < 0 ? ThreatMath.MaxBuy(baseCost, growth, level, Coins, room) : Mathf.Min(count, room);
            int bought = 0;
            for (int i = 0; i < qty; i++)
            {
                if (!buy())
                    break;
                bought += 1;
            }

            return bought;
        }

        public void ClawCoins(int amount)
        {
            if (amount <= 0)
                return;
            if (amount > Coins)
                amount = Coins;
            Coins -= amount;
            int day = Day();
            earned[6, day] -= amount;
            if (earned[6, day] < 0)
                earned[6, day] = 0;
        }

        public void NoteWorkerWood(int amount)
        {
            if (amount > 0)
                WorkerWood += amount;
        }

        public bool HasSurplus()
        {
            FortIdleConfig.ProgressionTuning tune = FortWorld.Config.progression;
            return Meat > tune.meatReserve || Bones > tune.boneReserve || Hide > tune.hideReserve;
        }

        public bool TrySellSurplus()
        {
            FortIdleConfig.ProgressionTuning tune = FortWorld.Config.progression;
            if (Meat > tune.meatReserve && SpendMeat(1))
            {
                AddCoins(tune.sellMeat);
                NoteReward("sell-meat");
                return true;
            }

            if (Bones > tune.boneReserve && SpendBones(1))
            {
                AddCoins(tune.sellBone);
                NoteReward("sell-bone");
                return true;
            }

            if (Hide > tune.hideReserve && SpendHide(1))
            {
                AddCoins(tune.sellHide);
                NoteReward("sell-hide");
                return true;
            }

            return false;
        }

        public bool CanSapWick => FortWorld.Config != null && Sap >= FortWorld.Config.progression.sapWickCost && SapOil < FortWorld.Config.progression.sapOilMax;

        public bool HasSpareCore => CanCoreFocusNow && (NextResearchCost == 0 || Cores > NextResearchCost);

        public bool TrySapWick()
        {
            if (!CanSapWick || !SpendSap(FortWorld.Config.progression.sapWickCost))
                return false;
            SapOil += 1;
            if (FortWorld.Fire != null)
                FortWorld.Fire.Heal(FortWorld.Config.progression.sapWickHeal);
            NoteReward("sap-wick");
            return true;
        }

        public bool CanCoreFocusNow => FortWorld.Config != null && Cores >= FortWorld.Config.progression.coreFocusCost && CoreFocus < FortWorld.Config.progression.coreFocusMax;

        public bool TryCoreFocus()
        {
            if (!CanCoreFocusNow || !SpendCores(FortWorld.Config.progression.coreFocusCost))
                return false;
            CoreFocus += 1;
            NoteReward("core-focus");
            return true;
        }

        public bool TryBuyArmor()
        {
            if (!CanUpgradeArmor)
                return false;
            int cost = NextArmorHide;
            if (!SpendHide(cost))
                return false;
            ArmorLevel += 1;
            NoteReward("armor-" + ArmorLevel);
            HeroController hero = FortWorld.Hero != null ? FortWorld.Hero.GetComponent<HeroController>() : null;
            if (hero != null)
                hero.RecalcVitals();
            return true;
        }

        public bool TryBuyPack()
        {
            if (!CanUpgradePack)
                return false;
            int cost = NextPackHide;
            if (!SpendHide(cost))
                return false;
            PackLevel += 1;
            NoteReward("pack-" + PackLevel);
            return true;
        }

        public bool TryPatch()
        {
            if (!CanPatch)
                return false;
            int cost = FortWorld.Config.heroCombat.patchHideCost;
            if (!SpendHide(cost))
                return false;
            PatchLevel += 1;
            NoteReward("patch-" + PatchLevel);
            HeroController hero = FortWorld.Hero != null ? FortWorld.Hero.GetComponent<HeroController>() : null;
            if (hero != null)
                hero.RecalcVitals();
            return true;
        }

        public bool TrySharpen()
        {
            if (!CanSharpen)
                return false;
            int cost = FortWorld.Config.heroCombat.sharpenBoneCost;
            if (!SpendBones(cost))
                return false;
            SharpenLevel += 1;
            NoteReward("sharpen-" + SharpenLevel);
            return true;
        }

        public bool TryCook()
        {
            HeroController hero = FortWorld.Hero != null ? FortWorld.Hero.GetComponent<HeroController>() : null;
            if (hero == null || hero.HealthFraction >= 0.98f)
                return false;
            if (!SpendMeat(1))
                return false;
            hero.Heal(FortWorld.Config.drops.cookHeal);
            NoteReward("cook");
            return true;
        }

        public bool TryArmMeal()
        {
            if (HeartyMeal)
                return false;
            int cost = FortWorld.Config.drops.dawnMealMeat;
            if (!SpendMeat(cost))
                return false;
            HeartyMeal = true;
            NoteReward("hearty-meal");
            return true;
        }

        public bool TryBroth()
        {
            FortIdleConfig.DropTuning drops = FortWorld.Config.drops;
            if (BrothStacks >= drops.brothMax)
                return false;
            if (!SpendBones(drops.brothBones))
                return false;
            BrothStacks += 1;
            RefreshWork();
            NoteReward("broth-" + BrothStacks);
            return true;
        }

        public bool TryResearch()
        {
            int cost = NextResearchCost;
            if (cost <= 0 || !SpendCores(cost))
                return false;
            ResearchRank += 1;
            NoteReward("research-" + ResearchRank);
            Debug.Log("FORTIDLE research=" + ResearchRank);
            return true;
        }

        public bool TryHire()
        {
            if (!CanHire)
                return false;
            int cost = NextHireCost;
            if (!Spend(cost))
                return false;
            WorkersHired += 1;
            WorkerCoinSpent += cost;
            NoteReward("hire-" + WorkersHired);
            Debug.Log("FORTIDLE hire=" + WorkersHired + " cost=" + cost + " coins=" + Coins);
            return true;
        }

        public void OnDawn()
        {
            int ended = FortWorld.Match != null ? FortWorld.Match.NightIndex - 1 : 1;
            DawnLine = LedgerLine(ended);
            DawnSerial += 1;
            Debug.Log(DawnLine);
            MealActive = HeartyMeal;
            HeartyMeal = false;
            BrothStacks = 0;
            RefreshWork();
            float roi = WorkerCoinSpent <= 0 ? 0f : (float)WorkerWood / WorkerCoinSpent;
            Debug.Log("FORTIDLE workers hired=" + WorkersHired
                + " wood=" + WorkerWood
                + " coinSpent=" + WorkerCoinSpent
                + " roi=" + roi.ToString("0.00", CultureInfo.InvariantCulture));
        }

        public void RefreshWork()
        {
            FortIdleConfig config = FortWorld.Config;
            float meal = MealActive && config != null ? config.drops.mealSpeed : 1f;
            float broth = config != null ? BrothStacks * config.drops.brothSpeed : 0f;
            WorkRate = meal * (1f + broth);
        }

        public string LedgerLine(int day)
        {
            if (day < 1)
                day = 1;
            if (day >= Days)
                day = Days - 1;
            return "FORTIDLE ledger day=" + day
                + " meat=" + earned[0, day] + "/" + spent[0, day]
                + " bone=" + earned[1, day] + "/" + spent[1, day]
                + " hide=" + earned[2, day] + "/" + spent[2, day]
                + " core=" + earned[3, day] + "/" + spent[3, day]
                + " sap=" + earned[4, day] + "/" + spent[4, day]
                + " iron=" + earned[5, day] + "/" + spent[5, day]
                + " coin=" + earned[6, day] + "/" + spent[6, day]
                + " wood=" + earned[7, day] + "/" + spent[7, day]
                + " stone=" + earned[8, day] + "/" + spent[8, day];
        }

        public void NoteReward(string id)
        {
            float time = FortWorld.Match != null ? FortWorld.Match.Elapsed : 0f;
            Debug.Log("FORTIDLE reward=" + id + " t=" + time.ToString("0.0", CultureInfo.InvariantCulture));
            if (!FortSave.Restoring)
                FortSave.WritePlayer();
        }

        public void ApplyBlob(FortSave.Blob blob)
        {
            if (blob == null)
                return;
            Coins = blob.coins;
            Iron = blob.iron;
            Meat = blob.meat;
            Bones = blob.bones;
            Hide = blob.hide;
            Cores = blob.cores;
            Sap = blob.sap;
            StackLevel = blob.stack;
            SpeedLevel = blob.speed;
            MeleeLevel = blob.melee;
            ArmorLevel = blob.armor;
            PackLevel = blob.pack;
            PatchLevel = blob.patch;
            SharpenLevel = blob.sharpen;
            WorkersHired = blob.workers;
            BrothStacks = blob.broth;
            ResearchRank = blob.research;
            CoreFocus = blob.coreFocus;
            SapOil = blob.sapOil;
            HeartyMeal = blob.meal != 0;
            MealActive = blob.mealActive != 0;
            RefreshWork();
        }

        bool Spend(int cost)
        {
            if (cost < 0 || Coins < cost)
                return false;
            Coins -= cost;
            Pay(6, cost);
            return true;
        }

        void Gain(int kind, int amount)
        {
            int day = Day();
            earned[kind, day] += amount;
        }

        void Pay(int kind, int amount)
        {
            int day = Day();
            spent[kind, day] += amount;
        }

        static int Day()
        {
            int day = FortWorld.Match != null ? FortWorld.Match.NightIndex : 1;
            if (day < 1)
                day = 1;
            if (day >= Days)
                day = Days - 1;
            return day;
        }

        public static int ScaledCost(int baseCost, float multiplier, int level)
        {
            if (level < 0)
                level = 0;
            float value = baseCost * Mathf.Pow(multiplier, level);
            int cost = Mathf.CeilToInt(value);
            if (cost < 1)
                cost = 1;
            return cost;
        }
    }
}
