using UnityEngine;

namespace FortIdle
{
    public static class BuildThud
    {
        static AudioSource source;
        static AudioClip clip;

        public static void Play(float pitch)
        {
            if (source == null)
            {
                GameObject host = new GameObject("BuildThud");
                Object.DontDestroyOnLoad(host);
                source = host.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.volume = 0.55f;
                clip = Make();
                source.clip = clip;
            }

            source.pitch = pitch < 0.2f ? 0.2f : pitch;
            source.Play();
        }

        static AudioClip Make()
        {
            const int rate = 22050;
            const float seconds = 0.09f;
            int length = (int)(rate * seconds);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float u = i / (float)length;
                float env = (1f - u) * (1f - u);
                float noise = (Mathf.PerlinNoise(i * 0.08f, 0.4f) * 2f - 1f);
                float body = Mathf.Sin(u * 38f) * 0.65f;
                data[i] = (body + noise * 0.55f) * env * 0.8f;
            }

            AudioClip made = AudioClip.Create("HammerThud", length, 1, rate, false);
            made.SetData(data, 0);
            return made;
        }
    }
}
