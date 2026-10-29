using UnityEngine;

namespace FortIdle
{
    public class Flyer : MonoBehaviour
    {
        Vector3 start;
        Vector3 end;
        float duration;
        float arc;
        float age;
        int poolKey = 100;

        public static void Spawn(Vector3 from, Vector3 to, float duration, float arcHeight, Material material, Vector3 size)
        {
            int key = material == FortWorld.Look.Stone ? 101 : 100;
            GameObject go = FortPool.Take(key, () =>
            {
                string id = material == FortWorld.Look.Stone ? "RockMoss" : "WoodLog";
                GameObject made = FortArt.Box(null, id, size, Color.white, 0f, false, false);
                if (made == null)
                {
                    made = Prop.Primitive(PrimitiveType.Cube, material, null, false);
                    made.transform.localScale = size;
                }

                return made;
            });

            go.name = "FlyingPlank";
            Flyer flyer = go.GetComponent<Flyer>();
            if (flyer == null)
                flyer = go.AddComponent<Flyer>();
            flyer.start = from;
            flyer.end = to;
            flyer.duration = duration;
            flyer.arc = arcHeight;
            flyer.age = 0f;
            flyer.poolKey = material == FortWorld.Look.Stone ? 101 : 100;
            go.transform.position = from;
        }

        void Update()
        {
            float span = duration <= 0f ? 0f : duration;
            age += Time.deltaTime;
            float t = span <= 0f ? 1f : Mathf.Clamp01(age / span);
            Vector3 position = Vector3.Lerp(start, end, t);
            // 4 * t * (1-t) peaks at 1 when t is 0.5, so the apex equals arc.
            position.y += arc * (4f * t * (1f - t));
            transform.position = position;
            if (t >= 1f)
                FortPool.Give(poolKey, gameObject);
        }
    }
}
