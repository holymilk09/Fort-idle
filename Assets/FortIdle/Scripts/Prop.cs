using UnityEngine;
using UnityEngine.Rendering;

namespace FortIdle
{
    public static class Prop
    {
        public static GameObject Primitive(PrimitiveType type, Material material, Transform parent, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            if (!collider)
            {
                Collider existing = go.GetComponent<Collider>();
                if (existing != null)
                    Object.Destroy(existing);
            }

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (material != null)
                    renderer.sharedMaterial = material;
                ApplyShadows(renderer);
            }

            if (parent != null)
                go.transform.SetParent(parent, false);
            return go;
        }

        public static void ApplyShadows(Renderer renderer)
        {
            bool shadows = FortLook.SoftShadows;
            renderer.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = shadows;
        }

        public static Vector3 CylinderScale(float diameter, float height)
        {
            float width = diameter / (BuiltinPrimitives.CylinderRadius * 2f);
            float y = height / BuiltinPrimitives.CylinderHeight;
            return new Vector3(width, y, width);
        }

        public static Vector3 CapsuleScale(float radius, float height)
        {
            float width = radius / BuiltinPrimitives.CapsuleRadius;
            float y = height / BuiltinPrimitives.CapsuleHeight;
            return new Vector3(width, y, width);
        }

        public static Vector3 SphereScale(float diameter)
        {
            float s = diameter / BuiltinPrimitives.SphereDiameter;
            return new Vector3(s, s, s);
        }
    }
}
