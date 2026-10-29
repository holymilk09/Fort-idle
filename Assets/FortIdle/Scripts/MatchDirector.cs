using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace FortIdle
{
    public enum MatchPhase
    {
        Day,
        Night,
        Resolved
    }

    public class MatchDirector : MonoBehaviour
    {
        public MatchPhase Phase { get; private set; }
        public float ClockRemaining { get; private set; }
        public bool Victory { get; private set; }
        public bool SpawnFinished { get; private set; }
        public int LivingMonsters { get; private set; }
        public int NightIndex { get; private set; }
        public float Elapsed { get; private set; }
        public NightPlan Plan { get; private set; }
        public int LossStreak { get; private set; }
        public bool ClockHeld;

        int tries;
        int duskCoins;
        int elites;

        Light sun;
        Camera view;
        float statusTimer;

        public void Init(Light sunLight, Camera camera)
        {
            sun = sunLight;
            view = camera;
        }

        void Start()
        {
            NightIndex = FortSave.PendingNight > 0 ? FortSave.PendingNight : 1;
            Phase = MatchPhase.Day;
            ClockRemaining = DayLength();
            SpawnFinished = false;
            ApplyLighting(0f);
            Debug.Log("FORTIDLE phase=Day night=1");
        }

        void Update()
        {
            float delta = Time.deltaTime;
            Elapsed += delta;
            TickClock(delta);
            CountLiving();
            TickResolution();
            ApplyLighting(Nightness());
            TickStatus(delta);
        }

        void TickClock(float delta)
        {
            if (Phase == MatchPhase.Resolved || ClockHeld)
                return;

            ClockRemaining -= delta;
            if (Phase == MatchPhase.Day && ClockRemaining <= 0f)
            {
                Phase = MatchPhase.Night;
                ClockRemaining = FortWorld.Config.wave.nightDuration;
                SpawnFinished = false;
                Plan = ThreatMath.Build(NightIndex, LossStreak);
                duskCoins = FortWorld.Progress != null ? FortWorld.Progress.Coins : 0;
                Debug.Log("FORTIDLE threat night=" + Plan.Night
                    + " ps=" + Mathf.RoundToInt(Plan.Power)
                    + " threat=" + Mathf.RoundToInt(Plan.Threat)
                    + " band=" + Plan.Band.ToString("0.00")
                    + " ratio=" + Plan.Ratio.ToString("0.00")
                    + " kind=" + ThreatMath.KindOf(Plan.Night)
                    + " shamblers=" + Plan.Shamblers
                    + " brutes=" + Plan.Brutes
                    + " bosses=" + Plan.Bosses
                    + " bossHp=" + Mathf.RoundToInt(Plan.BossHp)
                    + " hpx=" + (FortWorld.Config.monsters.hitPoints <= 0f ? 1f : Plan.ShamblerHp / FortWorld.Config.monsters.hitPoints).ToString("0.00")
                    + " tier=" + Plan.Tier
                    + " bruteTier=" + Plan.BruteTier
                    + " elite=" + Plan.EliteChance.ToString("0.00"));
                StartCoroutine(SpawnWave());
                Debug.Log("FORTIDLE phase=Night night=" + NightIndex);
            }
            else if (Phase == MatchPhase.Night && ClockRemaining < 0f)
            {
                ClockRemaining = 0f;
            }
        }

        void CountLiving()
        {
            int living = 0;
            for (int i = 0; i < FortWorld.Monsters.Count; i++)
            {
                if (FortWorld.Monsters[i].Alive)
                    living += 1;
            }

            LivingMonsters = living;
        }

        void TickResolution()
        {
            if (Phase == MatchPhase.Resolved)
                return;
            if (FortWorld.Fire != null && FortWorld.Fire.HitPoints <= 0f)
            {
                FailNight();
                return;
            }

            if (Phase == MatchPhase.Night && SpawnFinished && LivingMonsters == 0)
            {
                LogNight(true);
                FortPrestige.NoteCleared(Plan != null ? Plan.Threat : 0f);
                LossStreak = 0;
                tries = 0;
                if (NightIndex >= FortWorld.Config.wave.nightCount && !FortPrestige.EndlessOn())
                    Resolve(true);
                else
                {
                    BeginNextDay();
                    FortSave.WritePlayer();
                }
            }
        }

        void FailNight()
        {
            tries += 1;
            LossStreak += 1;
            int gained = FortWorld.Progress != null ? FortWorld.Progress.Coins - duskCoins : 0;
            if (gained > 0 && FortWorld.Progress != null)
            {
                int keep = Mathf.FloorToInt(gained * FortWorld.Config.progression.lossLootKeep);
                FortWorld.Progress.ClawCoins(gained - keep);
            }

            LogNight(false);

            StopAllCoroutines();
            ClearMonsters();
            if (FortWorld.Fire != null)
                FortWorld.Fire.Heal(FortWorld.Fire.MaxHitPoints);
            Debug.Log("FORTIDLE retry night=" + NightIndex + " try=" + tries + " streak=" + LossStreak);
            if (tries >= FortWorld.Config.progression.maxTries)
            {
                Resolve(false);
                return;
            }

            Phase = MatchPhase.Day;
            ClockRemaining = DayLength();
            SpawnFinished = false;
        }

        void LogNight(bool won)
        {
            NightPlan plan = Plan;
            int kept = FortWorld.Progress != null ? FortWorld.Progress.Coins - duskCoins : 0;
            if (kept < 0)
                kept = 0;
            Debug.Log("FORTIDLE night=end n=" + NightIndex
                + " win=" + (won ? 1 : 0)
                + " ps=" + (plan == null ? 0 : Mathf.RoundToInt(plan.Power))
                + " threat=" + (plan == null ? 0 : Mathf.RoundToInt(plan.Threat))
                + " band=" + (plan == null ? 1f : plan.Band).ToString("0.00")
                + " ratio=" + (plan == null ? 0f : plan.Ratio).ToString("0.00")
                + " coins=" + kept
                + " shamblers=" + (plan == null ? 0 : plan.Shamblers)
                + " brutes=" + (plan == null ? 0 : plan.Brutes)
                + " bossHp=" + (plan == null ? 0 : Mathf.RoundToInt(plan.BossHp)));
        }

        public void ShowNight(int night)
        {
            NightIndex = night < 1 ? 1 : night;
            Phase = MatchPhase.Night;
            ClockRemaining = FortWorld.Config.wave.nightDuration;
            ClockHeld = true;
            Plan = ThreatMath.Build(NightIndex, LossStreak);
            ApplyLighting(1f);
        }

        public void SpawnShownNight()
        {
            SpawnFinished = false;
            if (Plan == null || Plan.Night != NightIndex)
                Plan = ThreatMath.Build(NightIndex, LossStreak);
            StartCoroutine(SpawnWave());
        }

        public void ClearAllMonsters()
        {
            ClearMonsters();
        }

        void ClearMonsters()
        {
            for (int i = FortWorld.Monsters.Count - 1; i >= 0; i--)
            {
                Monster monster = FortWorld.Monsters[i];
                if (monster != null)
                    FortPool.Give(400 + (int)monster.Kind, monster.gameObject);
            }

            FortWorld.Monsters.Clear();
            LivingMonsters = 0;
        }

        void BeginNextDay()
        {
            NightIndex += 1;
            Phase = MatchPhase.Day;
            ClockRemaining = DayLength();
            SpawnFinished = false;
            if (FortWorld.Fire != null)
                FortWorld.Fire.Heal(FortWorld.Config.defenses.dawnFireHeal);
            if (FortWorld.Progress != null)
                FortWorld.Progress.OnDawn();
            Debug.Log("FORTIDLE phase=Day night=" + NightIndex + Supplies());
        }

        static string Supplies()
        {
            int coins = FortWorld.Progress != null ? FortWorld.Progress.Coins : 0;
            int iron = FortWorld.Progress != null ? FortWorld.Progress.Iron : 0;
            int stone = FortWorld.Progress != null ? FortWorld.Progress.CarriedStone : 0;
            int meat = FortWorld.Progress != null ? FortWorld.Progress.Meat : 0;
            int bones = FortWorld.Progress != null ? FortWorld.Progress.Bones : 0;
            int hide = FortWorld.Progress != null ? FortWorld.Progress.Hide : 0;
            int cores = FortWorld.Progress != null ? FortWorld.Progress.Cores : 0;
            int sap = FortWorld.Progress != null ? FortWorld.Progress.Sap : 0;
            for (int i = 0; i < FortWorld.Workers.Count; i++)
            {
                if (FortWorld.Workers[i].Job == WorkerAgent.Duty.Stone)
                    stone += FortWorld.Workers[i].Carried;
            }

            int wallHp = FortWorld.Wall == null ? 0 : Mathf.CeilToInt(FortWorld.Wall.HitPoints);
            int gateHp = FortWorld.Gate == null || !FortWorld.Gate.IsComplete ? 0 : Mathf.CeilToInt(FortWorld.Gate.HitPoints);
            int fire = FortWorld.Fire == null ? 0 : Mathf.CeilToInt(FortWorld.Fire.HitPoints);
            int tier = FortWorld.Wall == null ? 0 : FortWorld.Wall.Tier;
            return " coins=" + coins
                + " stone=" + stone
                + " iron=" + iron
                + " meat=" + meat
                + " bone=" + bones
                + " hide=" + hide
                + " core=" + cores
                + " sap=" + sap
                + " wallHp=" + wallHp
                + " tier=" + tier
                + " gateHp=" + gateHp
                + " fire=" + fire;
        }

        float DayLength()
        {
            FortIdleConfig.WaveTuning wave = FortWorld.Config.wave;
            return NightIndex <= 1 ? wave.dayDuration : wave.laterDayDuration;
        }

        void Resolve(bool victory)
        {
            Phase = MatchPhase.Resolved;
            Victory = victory;
            Debug.Log((victory ? "FORTIDLE result=clear" : "FORTIDLE result=fail")
                + " night=" + NightIndex
                + Supplies());
            if (FortWorld.Progress != null)
                Debug.Log(FortWorld.Progress.LedgerLine(NightIndex));
            FortSave.ProveRoundtrip();
        }

        float Nightness()
        {
            if (Phase == MatchPhase.Day)
            {
                float warning = FortWorld.Config.wave.nightWarningSeconds;
                if (warning <= 0f || ClockRemaining >= warning)
                    return 0f;
                return 1f - ClockRemaining / warning;
            }

            return 1f;
        }

        void ApplyLighting(float nightness)
        {
            FortIdleConfig.PresentationTuning look = FortWorld.Config.presentation;
            float mid = Mathf.Clamp(look.duskMidpoint, 0.01f, 0.99f);
            Color sky = nightness <= mid
                ? Color.Lerp(look.daySky, look.duskSky, nightness / mid)
                : Color.Lerp(look.duskSky, look.nightSky, (nightness - mid) / (1f - mid));

            if (view != null)
                view.backgroundColor = sky;

            RenderSettings.ambientMode = AmbientMode.Flat;
            float ambient = Mathf.Lerp(look.ambientDay, look.ambientNight, nightness);
            RenderSettings.ambientLight = sky * ambient;
            RenderSettings.fog = false;

            if (sun == null)
                return;
            sun.color = Color.Lerp(look.daySun, look.nightSun, nightness);
            sun.intensity = Mathf.Lerp(look.daySunIntensity, look.nightSunIntensity, nightness);
            float pitch = Mathf.Lerp(look.daySunPitch, look.nightSunPitch, nightness);
            sun.transform.rotation = Quaternion.Euler(pitch, look.sunYaw, 0f);
            FortLook.TuneSun(sun, nightness);
            FortLook.Grade(nightness);
        }

        IEnumerator SpawnWave()
        {
            FortIdleConfig config = FortWorld.Config;
            if (Plan == null)
                Plan = ThreatMath.Build(NightIndex, LossStreak);
            int shamblers = Plan.Shamblers;
            int brutes = Plan.Brutes;
            int treants = Plan.Bosses;
            int total = shamblers + brutes + treants;
            if (total < 1)
                total = 1;
            int index = 0;
            elites = 0;
            yield return SpawnKind(MonsterKind.Shambler, shamblers, total, index);
            index += shamblers;
            yield return SpawnKind(MonsterKind.Brute, brutes, total, index);
            index += brutes;
            yield return SpawnKind(MonsterKind.Treant, treants, total, index);
            SpawnFinished = true;
            Debug.Log("FORTIDLE spawn=done night=" + NightIndex
                + " shamblers=" + shamblers
                + " brutes=" + brutes
                + " treants=" + treants
                + " elites=" + elites);
        }

        IEnumerator SpawnKind(MonsterKind kind, int count, int total, int index)
        {
            if (count <= 0)
                yield break;
            FortIdleConfig config = FortWorld.Config;
            bool logged = false;
            for (int i = 0; i < count; i++)
            {
                int slot = index + i;
                float t = total <= 1 ? 0.5f : (slot + 0.5f) / total;
                float x = Mathf.Lerp(-config.layout.spawnHalfWidth, config.layout.spawnHalfWidth, t);
                int poolKey = 400 + (int)kind;
                GameObject go = FortPool.Take(poolKey, () => new GameObject(kind.ToString()));
                go.transform.position = new Vector3(x, config.layout.spawnHeight, config.layout.spawnZ);
                Monster monster = go.GetComponent<Monster>();
                if (monster == null)
                    monster = go.AddComponent<Monster>();
                float hpScale = 1f;
                float size = 1f;
                if (Plan != null)
                {
                    float baseHp = kind == MonsterKind.Brute
                        ? config.monsters.bruteHitPoints
                        : kind == MonsterKind.Treant ? config.monsters.treantHitPoints : config.monsters.hitPoints;
                    float want = kind == MonsterKind.Brute ? Plan.BruteHp : kind == MonsterKind.Treant ? Plan.BossHp : Plan.ShamblerHp;
                    if (baseHp > 1f && want > 1f)
                        hpScale = want / baseHp;
                    size = Plan.Size;
                }

                monster.Init(kind, slot, total, hpScale, size);
                if (Plan != null && kind != MonsterKind.Treant)
                {
                    EliteAffix affix = MonsterAffix.Roll(Plan.EliteChance);
                    int tier = kind == MonsterKind.Brute ? Plan.BruteTier : Plan.Tier;
                    if (affix != EliteAffix.None)
                        elites += 1;
                    if (tier > 1 || affix != EliteAffix.None)
                        monster.Promote(tier, affix, false);
                }

                FortWorld.Monsters.Add(monster);
                if (!logged)
                {
                    logged = true;
                    Debug.Log("FORTIDLE spawn kind=" + kind + " count=" + count + " night=" + NightIndex);
                }

                bool more = i < count - 1 || KindRemains(kind);
                if (more && config.wave.spawnInterval > 0f)
                    yield return new WaitForSeconds(config.wave.spawnInterval);
            }
        }

        bool KindRemains(MonsterKind kind)
        {
            int brutes = Plan != null ? Plan.Brutes : 0;
            int treants = Plan != null ? Plan.Bosses : 0;
            if (kind == MonsterKind.Shambler)
                return brutes > 0 || treants > 0;
            if (kind == MonsterKind.Brute)
                return treants > 0;
            return false;
        }

        void TickStatus(float delta)
        {
            statusTimer -= delta;
            if (statusTimer > 0f)
                return;
            statusTimer = FortWorld.Config.presentation.statusLogInterval;
            int wood = FortWorld.Stack != null ? FortWorld.Stack.Count : 0;
            int cap = FortWorld.Stack != null ? FortWorld.Stack.Capacity : 0;
            string wall = FortWorld.Wall == null ? "-" : Mathf.FloorToInt(FortWorld.Wall.Progress) + "/" + FortWorld.Wall.Cost;
            int wallHp = FortWorld.Wall == null ? 0 : Mathf.CeilToInt(FortWorld.Wall.HitPoints);
            string turret = FortWorld.TurretPad == null ? "-" : Mathf.FloorToInt(FortWorld.TurretPad.Progress) + "/" + FortWorld.TurretPad.Cost;
            string hut = FortWorld.Hut == null ? "-" : Mathf.FloorToInt(FortWorld.Hut.Progress) + "/" + FortWorld.Hut.Cost;
            float fire = FortWorld.Fire != null ? FortWorld.Fire.HitPoints : 0f;
            int coins = FortWorld.Progress != null ? FortWorld.Progress.Coins : 0;
            int stock = FortWorld.Progress != null ? FortWorld.Progress.Stockpile : 0;
            float speed = FortWorld.Progress != null ? FortWorld.Progress.MoveSpeed : 0f;
            string tease = FortWorld.Rig != null && FortWorld.Rig.Teasing ? "1" : "0";
            Debug.Log("FORTIDLE status phase=" + Phase
                + " night=" + NightIndex
                + " clock=" + ClockRemaining.ToString("0.0")
                + " wood=" + wood + "/" + cap
                + " wall=" + wall
                + " wallHp=" + wallHp
                + " turret=" + turret
                + " hut=" + hut
                + " horde=" + LivingMonsters
                + " fire=" + Mathf.CeilToInt(fire)
                + " coins=" + coins
                + " speed=" + speed.ToString("0.00")
                + " crew=" + FortWorld.Workers.Count
                + " stock=" + stock
                + " tease=" + tease
                + Supplies());
        }
    }
}
