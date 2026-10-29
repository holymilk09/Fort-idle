using UnityEngine;

namespace FortIdle
{
    public class RockNode : MonoBehaviour
    {
        public bool Standing { get; private set; }
        public bool Available => shown && Standing;

        int hitsRemaining;
        float respawnTimer;
        Transform body;
        bool shown = true;

        public void Init(Transform bodyTransform, int hits)
        {
            body = bodyTransform;
            hitsRemaining = hits;
            Standing = true;
            ApplyLock();
        }

        public void ApplyHit()
        {
            if (!Standing || !shown)
                return;
            hitsRemaining -= 1;
            if (hitsRemaining > 0)
                return;

            Standing = false;
            if (body != null)
                body.gameObject.SetActive(false);
            respawnTimer = FortWorld.Config.supplies.rockRespawnSeconds;
        }

        void Update()
        {
            ApplyLock();
            if (!shown || Standing)
                return;
            respawnTimer -= Time.deltaTime;
            if (respawnTimer > 0f)
                return;

            Standing = true;
            hitsRemaining = FortWorld.Config.supplies.hitsToBreakRock;
            if (body != null)
                body.gameObject.SetActive(true);
        }

        void ApplyLock()
        {
            int night = FortWorld.Match == null ? 1 : FortWorld.Match.NightIndex;
            bool open = night >= FortWorld.Config.supplies.rockUnlockNight;
            if (open == shown)
                return;
            shown = open;
            if (body != null)
                body.gameObject.SetActive(open && Standing);
        }
    }
}
