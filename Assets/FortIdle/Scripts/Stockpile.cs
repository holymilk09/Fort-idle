using System.Collections.Generic;
using UnityEngine;

namespace FortIdle
{
    public class Stockpile : MonoBehaviour
    {
        readonly List<Transform> planks = new List<Transform>();
        WorldLabel label;
        int captionCount = -1;

        public void Init(WorldLabel worldLabel)
        {
            label = worldLabel;
            Refresh();
        }

        public void Deposit()
        {
            if (FortWorld.Progress != null)
                FortWorld.Progress.AddStock(1);
            Refresh();
        }

        void Refresh()
        {
            FortIdleConfig config = FortWorld.Config;
            if (config == null)
                return;

            int count = FortWorld.Progress != null ? FortWorld.Progress.Stockpile : 0;
            int visible = count < config.stack.maxRenderedPlanks ? count : config.stack.maxRenderedPlanks;
            while (planks.Count < visible)
            {
                GameObject log = FortArt.Box(transform, "WoodLog", config.stack.plankSize, Color.white, 0f, false, false);
                if (log == null)
                {
                    log = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Plank, transform, false);
                    log.transform.localScale = config.stack.plankSize;
                }

                log.name = "StockPlank";
                planks.Add(log.transform);
            }

            for (int i = 0; i < planks.Count; i++)
            {
                bool on = i < visible;
                planks[i].gameObject.SetActive(on);
                if (on)
                    planks[i].localPosition = new Vector3(0f, (i + 0.5f) * config.stack.visualHeightStep, 0f);
            }

            if (label != null && count != captionCount)
            {
                captionCount = count;
                label.SetText(config.copy.stockpileName + "\n" + count);
            }
        }
    }
}
