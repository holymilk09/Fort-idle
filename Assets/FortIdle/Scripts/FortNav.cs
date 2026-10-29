using UnityEngine;

namespace FortIdle
{
    public static class FortNav
    {
        public static bool TryDetour(Vector3 from, Vector3 target, float clearance, out Vector3 via)
        {
            via = target;
            BuildPad blocker = null;
            for (int i = 0; i < FortWorld.Pads.Count; i++)
            {
                BuildPad pad = FortWorld.Pads[i];
                if (!pad.BlocksSegment(from, target, clearance))
                    continue;
                blocker = pad;
                break;
            }

            if (blocker == null)
                return false;

            Bounds bounds = blocker.WorldBounds;
            Vector3 min = bounds.min - new Vector3(clearance, 0f, clearance);
            Vector3 max = bounds.max + new Vector3(clearance, 0f, clearance);

            bool xClear = from.x <= min.x || from.x >= max.x;
            bool zClear = from.z <= min.z || from.z >= max.z;
            if (!xClear)
            {
                // Leave toward the side the body is already nearer, so a corner slide does not pull back through the slab.
                float side = Mathf.Abs(from.x - max.x) <= Mathf.Abs(from.x - min.x) ? max.x : min.x;
                via = new Vector3(side, from.y, from.z);
                return true;
            }

            if (!zClear)
            {
                float side = target.z < from.z ? min.z : max.z;
                via = new Vector3(from.x, from.y, side);
                return true;
            }

            // Outside both axes, but the straight line still clips a corner.
            Vector3 center = (min + max) * 0.5f;
            Vector3 corner = new Vector3(from.x < center.x ? min.x : max.x, from.y, from.z < center.z ? min.z : max.z);
            Vector3 away = corner - new Vector3(center.x, from.y, center.z);
            if (away.sqrMagnitude > 0.0001f)
                corner += away.normalized * 0.35f;
            via = corner;
            return true;
        }
    }
}
