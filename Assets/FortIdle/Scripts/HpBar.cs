using UnityEngine;

namespace FortIdle
{
    public class HpBar : MonoBehaviour
    {
        public System.Func<float> Read;

        Transform fill;
        Renderer fillRenderer;
        Material fillMaterial;
        float width;

        public static HpBar Create(Transform parent, float height, float barWidth)
        {
            GameObject root = new GameObject("HpBar");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0f, height, 0f);
            root.transform.localScale = new Vector3(barWidth, 0.09f, 0.04f);

            GameObject back = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Monster, root.transform, false);
            back.name = "Back";
            back.transform.localScale = Vector3.one;
            back.transform.localPosition = Vector3.zero;

            GameObject front = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Canopy, root.transform, false);
            front.name = "Fill";
            front.transform.localScale = Vector3.one;
            front.transform.localPosition = Vector3.zero;

            HpBar bar = root.AddComponent<HpBar>();
            bar.fill = front.transform;
            bar.fillRenderer = front.GetComponent<Renderer>();
            bar.width = barWidth;
            return bar;
        }

        void LateUpdate()
        {
            if (Read == null || fill == null)
                return;
            float fraction = Mathf.Clamp01(Read());
            fill.localScale = new Vector3(Mathf.Max(fraction, 0.001f), 1f, 1f);
            fill.localPosition = new Vector3((fraction - 1f) * 0.5f, 0f, -0.05f);
            if (fillRenderer != null && FortWorld.Config != null)
            {
                if (fillMaterial == null)
                    fillMaterial = fillRenderer.material;
                Color color = fraction < 0.35f ? FortWorld.Config.presentation.healthLow : FortWorld.Config.presentation.health;
                fillMaterial.SetColor("_BaseColor", color);
            }

            if (FortWorld.Camera != null)
                transform.rotation = FortWorld.Camera.transform.rotation;
            gameObject.SetActive(fraction > 0.001f && fraction < 0.995f);
        }
    }
}
