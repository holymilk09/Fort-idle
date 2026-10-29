using UnityEngine;

namespace FortIdle
{
    public class TraderStall : MonoBehaviour
    {
        float dwell;
        bool spent;

        public void Init(Vector3 position)
        {
            transform.position = position;
            GameObject crate = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.HutBody, transform, false);
            crate.name = "Stall";
            crate.transform.localScale = new Vector3(1.4f, 0.9f, 0.9f);
            crate.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            GameObject awning = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Coin, transform, false);
            awning.name = "Awning";
            awning.transform.localScale = new Vector3(1.7f, 0.12f, 1.1f);
            awning.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            FortIdleConfig config = FortWorld.Config;
            WorldLabel.Create(transform, new Vector3(0f, 1.5f, 0f), "TRADER", config.hud.worldLabelFontSize, config.hud.worldCanvasScale, config.hud.worldLabelSize);
        }

        void Update()
        {
            FortProgress progress = FortWorld.Progress;
            if (progress == null || FortWorld.Hero == null || FortWorld.Config == null)
                return;
            float distance = FortMath.HorizontalDistance(transform.position, FortWorld.Hero.position);
            bool near = distance <= FortWorld.Config.drops.useRadius;
            if (!near || !progress.HasSurplus())
            {
                dwell = 0f;
                spent = false;
                return;
            }

            dwell += Time.deltaTime;
            float delay = FortWorld.Config.upgrades.payDelay;
            if (spent)
            {
                if (dwell > delay + 0.7f)
                {
                    spent = false;
                    dwell = 0f;
                }

                return;
            }

            if (dwell < delay)
                return;
            if (progress.TrySellSurplus())
                spent = true;
        }
    }
}
