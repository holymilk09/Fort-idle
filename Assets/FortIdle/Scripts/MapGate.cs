using UnityEngine;

namespace FortIdle
{
    public class MapGate : MonoBehaviour
    {
        WorldLabel label;
        float dwell;
        bool spent;
        bool opened;
        int captionSignature = int.MinValue;
        int captionAmount = int.MinValue;

        public bool WantsVisit => FortWorld.Progress != null && FortWorld.Progress.CanResearch;

        public void Init(WorldLabel worldLabel)
        {
            label = worldLabel;
        }

        void Update()
        {
            FortProgress progress = FortWorld.Progress;
            FortIdleConfig config = FortWorld.Config;
            if (progress == null || config == null)
                return;
            if (progress.ResearchRank > 0)
                opened = true;
            bool near = FortWorld.Hero != null
                && FortMath.HorizontalDistance(transform.position, FortWorld.Hero.position) <= config.drops.useRadius;
            if (!near)
            {
                dwell = 0f;
                spent = false;
            }
            else
            {
                dwell += Time.deltaTime;
                float delay = config.upgrades.payDelay;
                if (spent && dwell > delay + 0.8f)
                {
                    spent = false;
                    dwell = 0f;
                }
                else if (!spent && dwell >= delay && progress.TryResearch())
                {
                    spent = true;
                    opened = true;
                }
            }

            if (label == null)
                return;
            int signature = progress.ResearchRank + (opened ? 16 : 0) + (progress.CanResearch ? 32 : 0);
            int amount = progress.NextResearchCost;
            if (signature == captionSignature && amount == captionAmount)
                return;
            captionSignature = signature;
            captionAmount = amount;
            if (progress.ResearchRank >= 3)
                label.SetText(config.copy.map2Label + "\n" + config.copy.maxLabel);
            else if (opened)
                label.SetText(config.copy.map2Label + "\n" + progress.ResearchRank);
            else if (progress.CanResearch)
                label.SetText(config.copy.researchName + "\n" + progress.NextResearchCost);
            else
                label.SetText(config.copy.lockedLabel);
        }
    }
}
