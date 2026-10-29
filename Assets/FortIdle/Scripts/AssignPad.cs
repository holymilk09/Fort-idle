using UnityEngine;

namespace FortIdle
{
    public class AssignPad : MonoBehaviour
    {
        public WorkerAgent.Duty Duty { get; private set; }

        Renderer discRenderer;
        Material discMaterial;
        WorldLabel label;
        Color idleColor;
        float dwell;
        bool spentThisVisit;

        public void Init(WorkerAgent.Duty duty, Renderer disc, WorldLabel worldLabel, Color color)
        {
            Duty = duty;
            discRenderer = disc;
            label = worldLabel;
            idleColor = color;
        }

        void Update()
        {
            FortIdleConfig config = FortWorld.Config;
            if (config == null)
                return;

            int night = FortWorld.Match == null ? 1 : FortWorld.Match.NightIndex;
            bool unlocked = night >= config.supplies.assignUnlockNight;
            bool occupied = false;
            if (unlocked && FortWorld.Hero != null)
            {
                float distance = FortMath.HorizontalDistance(transform.position, FortWorld.Hero.position);
                occupied = distance <= config.pour.padRadius;
            }

            if (discRenderer != null)
                discRenderer.enabled = unlocked;
            if (label != null)
                label.gameObject.SetActive(unlocked);
            if (!unlocked)
                return;

            if (!occupied)
            {
                dwell = 0f;
                spentThisVisit = false;
            }
            else
            {
                dwell += Time.deltaTime;
                if (!spentThisVisit && dwell >= config.upgrades.payDelay && WorkerAgent.AssignOne(Duty))
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
                string caption = Duty == WorkerAgent.Duty.Stone ? config.copy.assignStone : config.copy.assignWood;
                label.SetText(caption);
            }
        }
    }
}
