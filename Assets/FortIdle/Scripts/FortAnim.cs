using UnityEngine;

namespace FortIdle
{
    // Plays the imported takes on the hero and workers. The files ship Idle, Walk, Walk_Carry, Punch, PickUp, RecieveHit, and Defeat.
    public class FortAnim : MonoBehaviour
    {
        Animation anim;
        string current;
        bool ready;

        public bool Ready => ready;

        public static void Attach(GameObject piece)
        {
            if (piece == null || piece.GetComponent<FortAnim>() != null)
                return;
            piece.AddComponent<FortAnim>();
        }

        void Awake()
        {
            anim = GetComponent<Animation>();
            if (anim == null)
                anim = GetComponentInChildren<Animation>();
            int clips = anim == null ? 0 : anim.GetClipCount();
            Debug.Log("FORTIDLE clips name=" + gameObject.name + " count=" + clips);
            if (anim == null || clips < 1)
                return;
            ready = true;
            anim.cullingType = AnimationCullingType.AlwaysAnimate;
            foreach (AnimationState state in anim)
            {
                bool loop = state.name == "Idle" || state.name == "Walk" || state.name == "Walk_Carry";
                state.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
            }

        }

        void Start()
        {
            if (ready)
                Play("Idle");
        }

        public void Play(string name)
        {
            if (!ready || anim == null || string.IsNullOrEmpty(name) || current == name)
                return;
            if (anim.GetClip(name) == null)
                return;
            current = name;
            anim.CrossFade(name, 0.1f);
        }
    }
}
