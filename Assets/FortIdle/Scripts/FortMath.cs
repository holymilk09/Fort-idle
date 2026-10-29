using UnityEngine;

namespace FortIdle
{
    public static class FortMath
    {
        public static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        public static float EaseOutBack(float t, float overshoot)
        {
            float c1 = overshoot;
            float c3 = c1 + 1f;
            float u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }

        public static bool SegmentHitsSphere(Vector3 a, Vector3 b, Vector3 point, float radius)
        {
            Vector3 ab = b - a;
            float lengthSq = ab.sqrMagnitude;
            float t = lengthSq <= 0f ? 0f : Vector3.Dot(point - a, ab) / lengthSq;
            t = Mathf.Clamp01(t);
            Vector3 closest = a + ab * t;
            float radiusSq = radius * radius;
            return (closest - point).sqrMagnitude <= radiusSq;
        }

        public static bool SegmentIntersectsAabbXZ(Vector3 a, Vector3 b, Vector3 min, Vector3 max)
        {
            float tmin = 0f;
            float tmax = 1f;
            if (!Slab(a.x, b.x - a.x, min.x, max.x, ref tmin, ref tmax))
                return false;
            if (!Slab(a.z, b.z - a.z, min.z, max.z, ref tmin, ref tmax))
                return false;
            return true;
        }

        static bool Slab(float origin, float direction, float min, float max, ref float tmin, ref float tmax)
        {
            const float parallelEpsilon = 0.00001f;
            if (Mathf.Abs(direction) < parallelEpsilon)
                return origin >= min && origin <= max;

            float inv = 1f / direction;
            float t1 = (min - origin) * inv;
            float t2 = (max - origin) * inv;
            if (t1 > t2)
            {
                float swap = t1;
                t1 = t2;
                t2 = swap;
            }

            if (t1 > tmin)
                tmin = t1;
            if (t2 < tmax)
                tmax = t2;
            return tmin <= tmax;
        }
    }
}
