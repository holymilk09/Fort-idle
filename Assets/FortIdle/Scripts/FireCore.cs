using UnityEngine;

namespace FortIdle
{
    public class FireCore : MonoBehaviour
    {
        public float HitPoints { get; private set; }
        public float MaxHitPoints { get; private set; }
        public int Level { get; private set; } = 1;

        Transform flame;
        Vector3 baseScale;
        StagedBuild staged;
        int woodGot;
        int stoneGot;
        int ironGot;
        float pull;
        float dwell;
        bool spent;
        bool dressed2;
        bool dressed3;
        HpBar bar;

        public bool WantsWood => WoodRemaining > 0;
        public bool WantsStone => StoneRemaining > 0;
        public bool WantsIron => IronRemaining > 0;

        public int WoodRemaining
        {
            get
            {
                int need = NextWood();
                return need <= 0 ? 0 : Mathf.Max(0, need - (Level == 1 ? woodGot : woodGot));
            }
        }

        public int StoneRemaining => Level == 1 && Offers(2) ? Mathf.Max(0, FortWorld.Config.defenses.fireLv2Stone - stoneGot) : 0;

        public int IronRemaining => Level == 2 && Offers(3) ? Mathf.Max(0, FortWorld.Config.defenses.fireLv3Iron - ironGot) : 0;

        public bool WantsVisit
        {
            get
            {
                FortProgress progress = FortWorld.Progress;
                if (progress == null)
                    return false;
                HeroController hero = FortWorld.Hero != null ? FortWorld.Hero.GetComponent<HeroController>() : null;
                if (hero != null && hero.HealthFraction < 0.9f && progress.Meat > 0)
                    return true;
                if (!progress.HeartyMeal && progress.Meat >= FortWorld.Config.drops.dawnMealMeat)
                    return true;
                if (WantsWood || WantsStone || WantsIron)
                    return true;
                if (progress.Bones >= FortWorld.Config.drops.brothBones && progress.BrothStacks < FortWorld.Config.drops.brothMax)
                    return true;
                if (progress.CanSapWick || progress.HasSpareCore)
                    return true;
                return false;
            }
        }

        public void BindStages(StagedBuild build)
        {
            staged = build;
            if (staged != null && FortWorld.Config != null)
                staged.Sync(0, 1, 1, FortWorld.Config.stages.fire, false);
        }

        public void WriteSite(FortSave.Site site)
        {
            if (site == null)
                return;
            site.wood = woodGot;
            site.stone = stoneGot;
            site.iron = ironGot;
        }

        public void ReadSite(FortSave.Site site)
        {
            if (site == null || staged == null)
                return;
            if (site.wood <= 0 && site.stone <= 0 && site.iron <= 0)
                return;
            woodGot = site.wood;
            stoneGot = site.stone;
            ironGot = site.iron;
            NoteFireStage(false);
        }

        public void Init(Transform flameTransform, float hitPoints)
        {
            flame = flameTransform;
            baseScale = flame.localScale;
            MaxHitPoints = hitPoints;
            HitPoints = hitPoints;
            Level = 1;
            bar = HpBar.Create(transform, 1.6f, 1.1f);
            bar.Read = () => MaxHitPoints <= 0f ? 0f : HitPoints / MaxHitPoints;
        }

        public void Restore(int level, float hp)
        {
            int steps = level - 1;
            for (int i = 0; i < steps; i++)
                Promote();
            if (hp >= 0f)
            {
                HitPoints = Mathf.Min(hp, MaxHitPoints);
                Paint();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || MaxHitPoints <= 0f)
                return;
            HitPoints += amount;
            if (HitPoints > MaxHitPoints)
                HitPoints = MaxHitPoints;
            Paint();
        }

        public void ApplyDamage(float amount)
        {
            if (amount <= 0f || HitPoints <= 0f)
                return;
            HitPoints -= amount;
            if (HitPoints < 0f)
                HitPoints = 0f;
            Paint();
        }

        void Update()
        {
            FortIdleConfig config = FortWorld.Config;
            FortProgress progress = FortWorld.Progress;
            if (config == null || progress == null || FortWorld.Hero == null)
                return;
            float distance = FortMath.HorizontalDistance(transform.position, FortWorld.Hero.position);
            bool near = distance <= config.drops.useRadius;
            if (near)
                Pull(Time.deltaTime);
            TickUse(near, progress);
            if (Level >= 3)
                Warm(Time.deltaTime);
        }

        void Pull(float delta)
        {
            if (!WantsWood && !WantsStone && !WantsIron)
                return;
            FortIdleConfig config = FortWorld.Config;
            pull += config.pour.planksPerSecond * delta;
            while (pull >= 1f)
            {
                if (WantsWood && FortWorld.Stack != null && FortWorld.Stack.Count > 0)
                {
                    pull -= 1f;
                    Vector3 from = FortWorld.Stack.RemoveOne();
                    woodGot += 1;
                    if (FortWorld.Progress != null)
                        FortWorld.Progress.NoteWood(1, false);
                    Flyer.Spawn(from, transform.position + Vector3.up * 0.6f, FortWorld.Config.pour.flyDuration, FortWorld.Config.pour.flyArcHeight, FortWorld.Look.Plank, FortWorld.Config.stack.plankSize);
                    NoteFireStage(true);
                    continue;
                }

                if (WantsStone && FortWorld.Progress.RemoveStone())
                {
                    pull -= 1f;
                    stoneGot += 1;
                    NoteFireStage(true);
                    continue;
                }

                if (WantsIron && FortWorld.Progress.SpendIron(1))
                {
                    pull -= 1f;
                    ironGot += 1;
                    NoteFireStage(true);
                    continue;
                }

                break;
            }

            if (Level == 1 && Offers(2) && woodGot >= config.defenses.fireLv2Wood && stoneGot >= config.defenses.fireLv2Stone)
                Promote();
            else if (Level == 2 && Offers(3) && woodGot >= config.defenses.fireLv3Wood && ironGot >= config.defenses.fireLv3Iron)
                Promote();
        }

        void TickUse(bool near, FortProgress progress)
        {
            if (!near)
            {
                dwell = 0f;
                spent = false;
                return;
            }

            dwell += Time.deltaTime;
            float delay = FortWorld.Config.upgrades.payDelay;
            if (spent)
            {
                if (dwell > delay + 0.7f)
                {
                    spent = false;
                    dwell = 0f;
                }

                return;
            }

            if (dwell < delay)
                return;
            HeroController hero = FortWorld.Hero.GetComponent<HeroController>();
            bool used = false;
            if (hero != null && hero.HealthFraction < 0.92f)
                used = progress.TryCook();
            if (!used)
                used = progress.TryArmMeal();
            if (!used)
                used = progress.TryBroth();
            if (!used)
                used = progress.TrySapWick();
            if (!used)
                used = progress.TryCoreFocus();
            if (used)
                spent = true;
        }

        void Warm(float delta)
        {
            HeroController hero = FortWorld.Hero != null ? FortWorld.Hero.GetComponent<HeroController>() : null;
            if (hero == null)
                return;
            float distance = FortMath.HorizontalDistance(transform.position, hero.transform.position);
            if (distance > FortWorld.Config.defenses.warmthRadius)
                return;
            hero.Heal(FortWorld.Config.defenses.warmthHealPerSecond * delta);
        }

        int NextWood()
        {
            FortIdleConfig.DefenseTuning defenses = FortWorld.Config.defenses;
            if (Level == 1 && Offers(2))
                return defenses.fireLv2Wood;
            if (Level == 2 && Offers(3))
                return defenses.fireLv3Wood;
            return 0;
        }

        bool Offers(int level)
        {
            int night = FortWorld.Match == null ? 1 : FortWorld.Match.NightIndex;
            FortIdleConfig.DefenseTuning defenses = FortWorld.Config.defenses;
            if (level == 2)
                return night >= defenses.fireLv2UnlockNight;
            return night >= defenses.fireLv3UnlockNight;
        }

        void NoteFireStage(bool animate)
        {
            if (staged == null || FortWorld.Config == null)
                return;
            FortIdleConfig.DefenseTuning defenses = FortWorld.Config.defenses;
            int cost = NextWood();
            if (Level == 1)
                cost += defenses.fireLv2Stone;
            else if (Level == 2)
                cost += defenses.fireLv3Iron;
            if (cost < 1)
                return;
            int layer = Level;
            int before = staged.Shown(layer);
            staged.Sync(layer, woodGot + stoneGot + ironGot, cost, FortWorld.Config.stages.fireUpgrade, animate);
            if (animate && staged.Shown(layer) > before)
                Debug.Log("FORTIDLE stage=Fire n=" + staged.Shown(layer) + "/" + FortWorld.Config.stages.fireUpgrade);
        }

        void Promote()
        {
            FortIdleConfig.DefenseTuning defenses = FortWorld.Config.defenses;
            Level += 1;
            woodGot = 0;
            stoneGot = 0;
            ironGot = 0;
            MaxHitPoints = Level >= 3 ? defenses.fireLv3HitPoints : defenses.fireLv2HitPoints;
            HitPoints = MaxHitPoints;
            Paint();
            Dress();
            if (FortWorld.Progress != null)
                FortWorld.Progress.NoteReward("fire-l" + Level);
            Debug.Log("FORTIDLE fire=l" + Level);
        }

        void Dress()
        {
            if (staged != null)
            {
                dressed2 = Level >= 2;
                dressed3 = Level >= 3;
                return;
            }

            if (Level >= 2 && !dressed2)
            {
                dressed2 = true;
                for (int i = 0; i < 6; i++)
                {
                    float angle = i / 6f * Mathf.PI * 2f;
                    GameObject rock = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Stone, transform, false);
                    rock.name = "Ring";
                    rock.transform.localScale = new Vector3(0.28f, 0.22f, 0.28f);
                    rock.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.7f, 0.12f, Mathf.Sin(angle) * 0.7f);
                }
            }

            if (Level >= 3 && !dressed3 && flame != null)
            {
                dressed3 = true;
                flame.localScale = baseScale * 1.45f;
                baseScale = flame.localScale;
                GameObject extra = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.Fire, transform, false);
                extra.name = "Bonfire";
                extra.transform.localScale = baseScale * 0.7f;
                extra.transform.localPosition = flame.localPosition + Vector3.up * 0.35f;
            }
        }

        void Paint()
        {
            if (flame == null || MaxHitPoints <= 0f)
                return;
            float fraction = HitPoints / MaxHitPoints;
            float scale = Mathf.Lerp(FortWorld.Config.fire.minVisualScale, 1f, fraction);
            flame.localScale = baseScale * scale;
        }
    }
}
