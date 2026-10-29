using UnityEngine;

namespace FortIdle
{
    public class TreeNode : MonoBehaviour
    {
        public bool Standing { get; private set; }

        int hitsRemaining;
        float respawnTimer;
        Transform canopy;
        Transform trunk;
        Transform stump;
        Collider trunkCollider;

        public void Init(Transform trunkTransform, Transform canopyTransform, Transform stumpTransform, Collider collider, int hits)
        {
            trunk = trunkTransform;
            canopy = canopyTransform;
            stump = stumpTransform;
            trunkCollider = collider;
            hitsRemaining = hits;
            Standing = true;
            stump.gameObject.SetActive(false);
        }

        public void ApplyHit()
        {
            if (!Standing)
                return;
            hitsRemaining -= 1;
            if (hitsRemaining > 0)
                return;

            Standing = false;
            trunk.gameObject.SetActive(false);
            canopy.gameObject.SetActive(false);
            stump.gameObject.SetActive(true);
            if (trunkCollider != null)
                trunkCollider.enabled = false;
            respawnTimer = FortWorld.Config.chop.stumpRespawnSeconds;
        }

        void Update()
        {
            if (Standing)
                return;
            respawnTimer -= Time.deltaTime;
            if (respawnTimer > 0f)
                return;

            Standing = true;
            hitsRemaining = FortWorld.Config.chop.hitsToFellTree;
            trunk.gameObject.SetActive(true);
            canopy.gameObject.SetActive(true);
            stump.gameObject.SetActive(false);
            if (trunkCollider != null)
                trunkCollider.enabled = true;
        }
    }
}
