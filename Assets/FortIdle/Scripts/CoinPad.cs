using UnityEngine;

namespace FortIdle
{
    public class CoinPad : MonoBehaviour
    {
        public enum Kind
        {
            Stack,
            Speed,
            Hire,
            Melee
        }

        public Kind PadKind { get; private set; }

        Renderer discRenderer;
        WorldLabel label;
        Color idleColor;
        float dwell;
        bool spentThisVisit;

        public void Init(Kind kind, Renderer disc, WorldLabel worldLabel, Color color)
        {
            PadKind = kind;
            discRenderer = disc;
            label = worldLabel;
            idleColor = color;
        }

        void Update()
        {
            FortIdleConfig config = FortWorld.Config;
            FortProgress progress = FortWorld.Progress;
            if (config == null || progress == null)
                return;

            bool unlocked = PadKind != Kind.Hire || (FortWorld.Hut != null && FortWorld.Hut.IsComplete);
            bool occupied = false;
            if (unlocked && FortWorld.Hero != null)
            {
                float distance = FortMath.HorizontalDistance(transform.position, FortWorld.Hero.position);
                occupied = distance <= config.pour.padRadius;
            }

            if (!occupied)
            {
                dwell = 0f;
                spentThisVisit = false;
            }
            else
            {
                dwell += Time.deltaTime;
                float delay = config.upgrades.payDelay;
                if (spentThisVisit && dwell > delay + 0.85f)
                {
                    spentThisVisit = false;
                    dwell = 0f;
                }
                else if (!spentThisVisit && dwell >= delay && TryPurchase(progress))
                    spentThisVisit = true;
            }

            if (discRenderer != null)
            {
                if (discMaterial == null)
                    discMaterial = discRenderer.material;
                Color color = occupied ? config.presentation.padActive : idleColor;
                discMaterial.SetColor("_BaseColor", color);
            }

            if (label != null)
            {
                int signature = (unlocked ? 1 : 0)
                    + (progress.CanUpgradeStack ? 2 : 0)
                    + (progress.CanUpgradeSpeed ? 4 : 0)
                    + (progress.CanHire ? 8 : 0)
                    + (progress.CanUpgradeMelee ? 16 : 0)
                    + (progress.CanSharpen ? 32 : 0)
                    + (progress.CanUpgradePack ? 64 : 0)
                    + (progress.CanUpgradeArmor ? 128 : 0);
                int amount = progress.NextStackCost + progress.NextSpeedCost * 3 + progress.NextHireCost * 5 + progress.NextMeleeCost * 7;
                if (signature != captionSignature || amount != captionAmount)
                {
                    captionSignature = signature;
                    captionAmount = amount;
                    label.SetText(Caption(config, progress, unlocked));
                }
            }
        }

        Material discMaterial;
        int captionSignature = int.MinValue;
        int captionAmount = int.MinValue;

        bool TryPurchase(FortProgress progress)
        {
            if (PadKind == Kind.Stack)
            {
                if (progress.CanUpgradePack && progress.Hide >= progress.NextPackHide)
                    return progress.TryBuyPack();
                if (progress.CanUpgradeArmor && progress.Hide >= progress.NextArmorHide)
                    return progress.TryBuyArmor();
                if (progress.CanPatch && progress.Hide >= FortWorld.Config.heroCombat.patchHideCost)
                    return progress.TryPatch();
                return progress.TryBuyStack();
            }

            if (PadKind == Kind.Melee)
            {
                if (progress.CanSharpen && progress.Bones >= FortWorld.Config.heroCombat.sharpenBoneCost)
                    return progress.TrySharpen();
                return progress.TryBuyMelee();
            }

            if (PadKind == Kind.Speed)
                return progress.TryBuySpeed();
            if (FortWorld.Hut == null || !FortWorld.Hut.IsComplete)
                return false;
            if (!progress.TryHire())
                return false;
            WorkerAgent.SpawnNext();
            return true;
        }

        string Caption(FortIdleConfig config, FortProgress progress, bool unlocked)
        {
            if (!unlocked)
                return config.copy.lockedLabel + "\n" + config.copy.hireName;

            if (PadKind == Kind.Melee)
            {
                if (progress.CanSharpen && progress.Bones >= FortWorld.Config.heroCombat.sharpenBoneCost)
                    return config.copy.boneLabel + "\n" + FortWorld.Config.heroCombat.sharpenBoneCost;
                if (!progress.CanUpgradeMelee)
                    return config.copy.meleeName + "\n" + config.copy.maxLabel;
                return config.copy.meleeName + "\n" + progress.NextMeleeCost;
            }

            if (PadKind == Kind.Stack)
            {
                if (progress.CanUpgradePack && progress.Hide >= progress.NextPackHide)
                    return config.copy.packName + "\n" + progress.NextPackHide;
                if (progress.CanUpgradeArmor && progress.Hide >= progress.NextArmorHide)
                    return config.copy.armorName + "\n" + progress.NextArmorHide;
                if (!progress.CanUpgradeStack)
                    return config.copy.bagName + "\n" + config.copy.maxLabel;
                return config.copy.bagName + "\n" + progress.NextStackCost;
            }

            if (PadKind == Kind.Speed)
            {
                if (!progress.CanUpgradeSpeed)
                    return config.copy.speedLabel + "\n" + config.copy.maxLabel;
                return config.copy.speedLabel + "\n" + progress.NextSpeedCost;
            }

            if (!progress.CanHire)
                return config.copy.hireName + "\n" + config.copy.maxLabel;
            return config.copy.hireName + "\n" + progress.NextHireCost;
        }
    }
}
