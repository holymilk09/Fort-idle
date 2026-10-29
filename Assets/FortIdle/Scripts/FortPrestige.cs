using UnityEngine;
using UnityEngine.SceneManagement;

namespace FortIdle
{
    public enum NightKind
    {
        Normal,
        Boss,
        BloodMoon,
        Rest
    }

    public static class FortPrestige
    {
        public static int EmbersClaimed { get; private set; }
        public static float LifetimeThreat { get; private set; }

        static readonly FortIdleConfig.EndlessTuning fallbackEndless = new FortIdleConfig.EndlessTuning();
        static readonly FortIdleConfig.PrestigeTuning fallbackPrestige = new FortIdleConfig.PrestigeTuning();

        public static FortIdleConfig.EndlessTuning Endless
        {
            get
            {
                FortIdleConfig config = FortWorld.Config;
                return config != null && config.endless != null ? config.endless : fallbackEndless;
            }
        }

        public static FortIdleConfig.PrestigeTuning Tune
        {
            get
            {
                FortIdleConfig config = FortWorld.Config;
                return config != null && config.prestige != null ? config.prestige : fallbackPrestige;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics()
        {
            EmbersClaimed = 0;
            LifetimeThreat = 0f;
        }

        public static void Restore(FortSave.Blob blob)
        {
            EmbersClaimed = blob == null || blob.embersClaimed < 0 ? 0 : blob.embersClaimed;
            LifetimeThreat = blob == null || blob.lifetimeThreat < 0f ? 0f : blob.lifetimeThreat;
        }

        public static void NoteCleared(float threat)
        {
            if (threat > 0f)
                LifetimeThreat += threat;
        }

        public static int EmbersTotal()
        {
            FortIdleConfig.PrestigeTuning tune = Tune;
            float div = Mathf.Max(1f, tune.emberDiv);
            return Mathf.FloorToInt(tune.emberK * Mathf.Sqrt(LifetimeThreat / div));
        }

        public static int Gain()
        {
            int gain = EmbersTotal() - EmbersClaimed;
            return gain < 0 ? 0 : gain;
        }

        public static float DpsMult()
        {
            return 1f + Soft(EmbersClaimed * Tune.emberDps);
        }

        public static float IncomeMult()
        {
            return 1f + Soft(EmbersClaimed * Tune.emberIncome);
        }

        static float Soft(float raw)
        {
            if (raw <= 0f)
                return 0f;
            float cap = Tune.softCap;
            if (cap <= 0f)
                return raw;
            return raw / (1f + raw / cap);
        }

        public static bool EndlessOn()
        {
            return Endless.enabled && !FortSave.DemoSession();
        }

        public static bool Pushed(MatchDirector match)
        {
            if (match == null)
                return false;
            return match.LossStreak >= Tune.offerLosses || (match.Phase == MatchPhase.Resolved && !match.Victory);
        }

        public static bool Offered(MatchDirector match)
        {
            if (match == null || Gain() <= 0)
                return false;
            return Pushed(match) || match.NightIndex >= Tune.demandNight;
        }

        public static void Rebuild()
        {
            int gain = Gain();
            if (gain <= 0)
                return;
            EmbersClaimed += gain;
            Debug.Log("FORTIDLE rebuild gain=" + gain
                + " embers=" + EmbersClaimed
                + " lifetime=" + Mathf.RoundToInt(LifetimeThreat)
                + " night=" + (FortWorld.Match != null ? FortWorld.Match.NightIndex : 0));
            FortSave.WriteRebuild();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
