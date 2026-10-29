using UnityEngine;
using UnityEngine.UI;

namespace FortIdle
{
    public class WorldLabel : MonoBehaviour
    {
        Text text;
        Canvas canvas;
        bool floating;
        float age;
        float duration;
        float rise;
        Vector3 origin;
        Color baseColor;

        public static WorldLabel Create(Transform parent, Vector3 localPosition, string value, int fontSize, float canvasScale, Vector2 size)
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject root = new GameObject("WorldLabel");
            if (parent != null)
                root.transform.SetParent(parent, false);
            root.transform.localPosition = localPosition;

            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            if (FortWorld.Camera != null)
                canvas.worldCamera = FortWorld.Camera;

            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.localScale = Vector3.one * canvasScale;

            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(root.transform, false);
            Text label = textObject.AddComponent<Text>();
            label.font = FortWorld.Look.Font;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.color = config.presentation.hudText;
            label.raycastTarget = false;
            label.text = value;
            RectTransform textRect = label.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            WorldLabel world = root.AddComponent<WorldLabel>();
            world.text = label;
            world.canvas = canvas;
            world.baseColor = label.color;
            return world;
        }

        public static void SpawnDamage(Vector3 worldPosition, float amount)
        {
            FortIdleConfig config = FortWorld.Config;
            if (config == null || FortWorld.Look == null || FortWorld.Look.Font == null)
                return;

            const int PoolKey = 330;
            GameObject go = FortPool.Take(PoolKey, () => Create(
                null,
                worldPosition,
                "0",
                config.hud.damageNumberFontSize,
                config.hud.damageNumberScale,
                config.hud.worldLabelSize).gameObject);
            WorldLabel label = go.GetComponent<WorldLabel>();
            label.Arm(worldPosition + Vector3.up * (config.monsters.height * 0.5f), Mathf.RoundToInt(amount).ToString(), config.hud.damageNumberDuration, config.hud.damageNumberRise);
        }

        public void Arm(Vector3 worldPosition, string value, float life, float riseHeight)
        {
            transform.position = worldPosition;
            floating = true;
            age = 0f;
            duration = life;
            rise = riseHeight;
            origin = worldPosition;
            baseColor = FortWorld.Config != null ? FortWorld.Config.presentation.warningText : Color.white;
            if (canvas != null)
                canvas.enabled = true;
            if (text != null)
            {
                text.text = value;
                text.color = baseColor;
            }
        }

        public void SetText(string value)
        {
            if (text == null || value == null || text.text == value)
                return;
            text.text = value;
        }

        float FadeAlpha(Camera camera)
        {
            if (camera == null || FortWorld.Config == null)
                return 1f;
            float near = FortWorld.Config.pour.labelFadeNear;
            float far = FortWorld.Config.pour.labelFadeFar;
            if (near <= 0f)
                near = 7.5f;
            if (far <= near)
                far = 14f;
            float distance = Vector3.Distance(transform.position, camera.transform.position);
            return Mathf.InverseLerp(far, near, distance);
        }

        static readonly Vector3[] boundsCorners = new Vector3[4];

        Rect ScreenBounds(Camera camera)
        {
            RectTransform rect = (RectTransform)transform;
            rect.GetWorldCorners(boundsCorners);
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                Vector3 screen = camera.WorldToScreenPoint(boundsCorners[i]);
                if (screen.z < 0f)
                    return new Rect(0f, 0f, 0f, 0f);
                minX = Mathf.Min(minX, screen.x);
                minY = Mathf.Min(minY, screen.y);
                maxX = Mathf.Max(maxX, screen.x);
                maxY = Mathf.Max(maxY, screen.y);
            }

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        public void SetColor(Color color)
        {
            baseColor = color;
            if (text != null && !floating)
                text.color = color;
        }

        void LateUpdate()
        {
            Camera camera = FortWorld.Camera;
            if (camera != null)
                transform.rotation = camera.transform.rotation;

            if (!floating)
            {
                float alpha = camera == null ? 1f : FadeAlpha(camera);
                bool blocked = camera != null && GameHud.BlocksWorldLabel(ScreenBounds(camera));
                if (blocked)
                    alpha = 0f;
                if (canvas != null)
                    canvas.enabled = alpha > 0.02f;
                if (text != null)
                {
                    Color tint = baseColor;
                    tint.a = alpha;
                    text.color = tint;
                }

                return;
            }

            age += Time.deltaTime;
            float span = duration <= 0f ? 1f : duration;
            float u = Mathf.Clamp01(age / span);
            transform.position = origin + Vector3.up * (rise * u);
            Color color = baseColor;
            color.a = 1f - u;
            text.color = color;
            if (u >= 1f)
                FortPool.Give(330, gameObject);
        }
    }
}
