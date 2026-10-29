using System.Collections;
using UnityEngine;

namespace FortIdle
{
    public class BuildPad : MonoBehaviour
    {
        public enum Kind
        {
            Wall,
            Turret,
            Hut,
            Spike,
            Gate,
            Watchtower,
            TurretUpgrade
        }

        public Kind PadKind { get; private set; }
        public bool IsComplete { get; private set; }
        public bool IsBlocking => IsComplete && standing && structureCollider != null && structureCollider.enabled;
        public bool IsAttackTarget => IsComplete && HitPoints > 0f && (PadKind == Kind.Watchtower || PadKind == Kind.Turret);
        public float Progress { get; private set; }
        public int Cost { get; private set; }
        public float HitPoints { get; private set; }
        public float MaxHitPoints { get; private set; }
        public int Tier { get; private set; }
        public int UnlockNight { get; private set; }
        public int Remaining => WoodRemaining;
        public int PouredWood => woodHave;
        public int PouredTierWood => tierWood;
        public int StageShown => ActiveStages == null ? 0 : ActiveStages.Shown(PadKind == Kind.TurretUpgrade ? 1 : 0);
        public StagedBuild Stages => staged;
        public bool QuietLabel;

        public float ListedDps
        {
            get
            {
                if (!IsComplete)
                    return 0f;
                if (turret != null && turret.isActiveAndEnabled)
                    return turret.DamagePerSecond;
                if (PadKind == Kind.Spike)
                    return SpikeDamage(MonsterKind.Shambler, transform.position);
                return 0f;
            }
        }

        Transform structurePivot;
        Transform structureMesh;
        Collider structureCollider;
        Renderer discRenderer;
        Renderer structureRenderer;
        Transform fillBar;
        WorldLabel label;
        Turret turret;
        bool standing = true;
        bool repairable;
        Color idleColor;
        Vector3 baseMeshScale;
        int woodNeed;
        int stoneNeed;
        int ironNeed;
        int woodHave;
        int stoneHave;
        int ironHave;
        int tierWood;
        int tierStone;
        int tierIron;
        Vector3 bornScale;
        bool haveBorn;
        float specialDwell;
        bool specialSpent;
        StagedBuild staged;
        StagedBuild upgradeTarget;
        Transform ticks;

        StagedBuild ActiveStages => upgradeTarget != null && PadKind == Kind.TurretUpgrade ? upgradeTarget : staged;

        public Bounds WorldBounds => structureCollider.bounds;

        public bool BlocksSegment(Vector3 from, Vector3 to, float clearance)
        {
            if (structureCollider == null || structurePivot == null || !structurePivot.gameObject.activeSelf)
                return false;
            if (!IsComplete)
                return false;
            Bounds bounds = structureCollider.bounds;
            Vector3 min = bounds.min - new Vector3(clearance, 0f, clearance);
            Vector3 max = bounds.max + new Vector3(clearance, 0f, clearance);
            return FortMath.SegmentIntersectsAabbXZ(from, to, min, max);
        }

        public bool IsUnlocked
        {
            get
            {
                if (FortWorld.Match == null)
                    return UnlockNight <= 1;
                return FortWorld.Match.NightIndex >= UnlockNight;
            }
        }

        public bool NeedsRepair => repairable && IsComplete && MaxHitPoints > 0f && HitPoints < MaxHitPoints;

        public int WoodRemaining
        {
            get
            {
                if (!IsUnlocked)
                    return 0;
                if (!IsComplete)
                    return Mathf.Max(0, woodNeed - woodHave);
                if (NeedsRepair)
                {
                    float per = FortWorld.Config.defenses.repairHitPointsPerWood;
                    if (per <= 0f)
                        per = 1f;
                    return Mathf.CeilToInt((MaxHitPoints - HitPoints) / per);
                }

                int wood;
                int stone;
                int iron;
                if (!TierOffer(out wood, out stone, out iron))
                    return 0;
                return Mathf.Max(0, wood - tierWood);
            }
        }

        public int StoneRemaining
        {
            get
            {
                if (!IsUnlocked || NeedsRepair)
                    return 0;
                if (!IsComplete)
                    return Mathf.Max(0, stoneNeed - stoneHave);
                int wood;
                int stone;
                int iron;
                if (!TierOffer(out wood, out stone, out iron))
                    return 0;
                return Mathf.Max(0, stone - tierStone);
            }
        }

        public int IronRemaining
        {
            get
            {
                if (!IsUnlocked || NeedsRepair)
                    return 0;
                if (!IsComplete)
                    return Mathf.Max(0, ironNeed - ironHave);
                int wood;
                int stone;
                int iron;
                if (!TierOffer(out wood, out stone, out iron))
                    return 0;
                return Mathf.Max(0, iron - tierIron);
            }
        }

        public bool WantsWood => WoodRemaining > 0;
        public bool WantsStone => StoneRemaining > 0;
        public bool WantsIron => IronRemaining > 0;

        public void Init(Kind kind, int cost, float maxHitPoints, Transform pivot, Collider blocker, Renderer disc, Transform bar, WorldLabel worldLabel, Turret weapon, Color color)
        {
            PadKind = kind;
            Cost = cost;
            woodNeed = cost;
            MaxHitPoints = maxHitPoints;
            HitPoints = maxHitPoints;
            structurePivot = pivot;
            structureCollider = blocker;
            discRenderer = disc;
            fillBar = bar;
            label = worldLabel;
            turret = weapon;
            idleColor = color;
            Progress = 0f;
            IsComplete = false;
            standing = true;
            UnlockNight = 1;
            repairable = kind == Kind.Wall || kind == Kind.Gate || kind == Kind.Watchtower || kind == Kind.Turret;
            if (structurePivot != null)
                structurePivot.gameObject.SetActive(false);
            if (structureCollider != null)
                structureCollider.enabled = false;
            if (turret != null)
                turret.enabled = false;
            if (maxHitPoints > 0f)
            {
                HpBar hpBar = HpBar.Create(transform, 2.6f, 1.3f);
                hpBar.Read = () => MaxHitPoints <= 0f ? 0f : HitPoints / MaxHitPoints;
            }

            RefreshVisuals(false);
        }

        public void SetRecipe(int wood, int stone, int iron)
        {
            woodNeed = wood;
            stoneNeed = stone;
            ironNeed = iron;
            Cost = wood + stone + iron;
        }

        public void SetUnlockNight(int night)
        {
            UnlockNight = night < 1 ? 1 : night;
        }

        public void BindStages(StagedBuild build)
        {
            staged = build;
        }

        public void BindUpgrade(StagedBuild build)
        {
            upgradeTarget = build;
        }

        public Vector3 DeliveryPoint()
        {
            Transform point = structurePivot != null ? structurePivot : transform;
            return point.position + Vector3.up * 1.15f;
        }

        public void WriteSite(FortSave.Site site)
        {
            if (site == null)
                return;
            site.wood = woodHave;
            site.stone = stoneHave;
            site.iron = ironHave;
            site.tierWood = tierWood;
            site.tierStone = tierStone;
            site.tierIron = tierIron;
        }

        public void ReadSite(FortSave.Site site)
        {
            if (site == null)
                return;
            if (!IsComplete)
            {
                if (site.wood <= 0 && site.stone <= 0 && site.iron <= 0)
                    return;
                woodHave = site.wood;
                stoneHave = site.stone;
                ironHave = site.iron;
                Progress = woodHave + stoneHave + ironHave;
                WakeStructure();
                NoteBuildStage(false);
                RefreshVisuals(false);
                return;
            }

            tierWood = site.tierWood;
            tierStone = site.tierStone;
            tierIron = site.tierIron;
            NoteUpgradeStage(false);
        }

        public void ConcealStages()
        {
            if (staged != null)
                staged.Conceal();
        }

        public void BindMesh(Transform mesh, Renderer renderer)
        {
            structureMesh = mesh;
            structureRenderer = renderer;
            if (mesh != null)
            {
                if (!haveBorn)
                {
                    bornScale = mesh.localScale;
                    haveBorn = true;
                }

                baseMeshScale = mesh.localScale;
            }
            RefreshDress();
        }

        public void RefreshDress()
        {
            if (staged != null)
                return;
            if (PadKind != Kind.Wall || structureMesh == null)
                return;
            string id = "WallWood";
            int tiles = 4;
            if (Tier == 1)
            {
                id = "WallReinforced";
                tiles = 3;
            }
            else if (Tier >= 2)
            {
                id = "WallStone";
                tiles = 3;
            }

            if (FortArt.Fill(structureMesh, id, tiles, true) && structureRenderer != null)
                structureRenderer.enabled = false;
        }

        public void AddPlank()
        {
            AddWood();
        }

        public void AddWood()
        {
            if (!IsUnlocked)
                return;
            if (!IsComplete)
            {
                if (woodHave >= woodNeed)
                    return;
                woodHave += 1;
                if (FortWorld.Progress != null)
                    FortWorld.Progress.NoteWood(1, false);
                Progress = woodHave + stoneHave + ironHave;
                WakeStructure();
                NoteBuildStage(true);
                if (RecipeFilled())
                    FinishBuild();
                RefreshVisuals(true);
                return;
            }

            if (NeedsRepair)
            {
                float per = FortWorld.Config.defenses.repairHitPointsPerWood;
                if (FortWorld.Progress != null)
                    FortWorld.Progress.NoteWood(1, false);
                HitPoints += per;
                if (HitPoints > MaxHitPoints)
                    HitPoints = MaxHitPoints;
                if (turret != null && HitPoints > 0f)
                    turret.enabled = true;
                RaiseIfHeld();
                RefreshVisuals(true);
                return;
            }

            int wood;
            int stone;
            int iron;
            if (!TierOffer(out wood, out stone, out iron) || tierWood >= wood)
                return;
            tierWood += 1;
            if (FortWorld.Progress != null)
                FortWorld.Progress.NoteWood(1, false);
            NoteUpgradeStage(true);
            if (TierFilled(wood, stone, iron))
                FinishTier();
            RefreshVisuals(true);
        }

        public void AddStone()
        {
            if (!IsUnlocked || NeedsRepair)
                return;
            if (!IsComplete)
            {
                if (stoneHave >= stoneNeed)
                    return;
                stoneHave += 1;
                Progress = woodHave + stoneHave + ironHave;
                WakeStructure();
                NoteBuildStage(true);
                if (RecipeFilled())
                    FinishBuild();
                RefreshVisuals(true);
                return;
            }

            int wood;
            int stone;
            int iron;
            if (!TierOffer(out wood, out stone, out iron) || tierStone >= stone)
                return;
            tierStone += 1;
            NoteUpgradeStage(true);
            if (TierFilled(wood, stone, iron))
                FinishTier();
            RefreshVisuals(true);
        }

        public void AddIron()
        {
            if (!IsUnlocked || NeedsRepair)
                return;
            if (!IsComplete)
            {
                if (ironHave >= ironNeed)
                    return;
                ironHave += 1;
                Progress = woodHave + stoneHave + ironHave;
                WakeStructure();
                NoteBuildStage(true);
                if (RecipeFilled())
                    FinishBuild();
                RefreshVisuals(true);
                return;
            }

            int wood;
            int stone;
            int iron;
            if (!TierOffer(out wood, out stone, out iron) || tierIron >= iron)
                return;
            tierIron += 1;
            NoteUpgradeStage(true);
            if (TierFilled(wood, stone, iron))
                FinishTier();
            RefreshVisuals(true);
        }

        public float SpikeDamage(MonsterKind monsterKind, Vector3 point)
        {
            if (PadKind != Kind.Spike || !IsComplete || !IsUnlocked)
                return 0f;
            if (monsterKind == MonsterKind.Treant || (monsterKind == MonsterKind.Brute && Tier < 1))
                return 0f;
            if (monsterKind != MonsterKind.Shambler && monsterKind != MonsterKind.Brute)
                return 0f;
            FortIdleConfig.DefenseTuning defenses = FortWorld.Config.defenses;
            if (FortMath.HorizontalDistance(point, transform.position) > defenses.spikeRadius)
                return 0f;
            if (Tier >= 2)
                return defenses.spikeLv3Damage * FortPrestige.DpsMult();
            if (Tier >= 1)
                return defenses.spikeLv2Damage * FortPrestige.DpsMult();
            return defenses.spikeDamagePerSecond * FortPrestige.DpsMult();
        }

        public bool WantsSpecial
        {
            get
            {
                FortProgress progress = FortWorld.Progress;
                if (!IsComplete || !IsUnlocked || progress == null)
                    return false;
                FortIdleConfig.DefenseTuning defenses = FortWorld.Config.defenses;
                if (PadKind == Kind.Spike && Tier <= 0)
                    return progress.Bones >= defenses.spikeLv2Bones;
                if (PadKind == Kind.Spike && Tier == 1)
                    return progress.Bones >= defenses.spikeLv3Bones && progress.Iron >= defenses.spikeLv3Iron;
                if (PadKind == Kind.TurretUpgrade && FortWorld.TurretWeapon != null && FortWorld.TurretWeapon.Level < 3)
                    return progress.Sap >= defenses.turretLv3Sap;
                return false;
            }
        }

        public void ApplyDamage(float amount)
        {
            if (amount <= 0f || !IsComplete || MaxHitPoints <= 0f || HitPoints <= 0f)
                return;
            HitPoints -= amount;
            if (HitPoints <= 0f)
            {
                HitPoints = 0f;
                standing = false;
                if (structureCollider != null)
                    structureCollider.enabled = false;
                if (turret != null)
                    turret.enabled = false;
                if (structureMesh != null)
                    structureMesh.localScale = new Vector3(baseMeshScale.x, 0f, baseMeshScale.z);
                if (staged != null)
                    staged.SetStrain(0.05f);
            }

            RefreshVisuals(false);
        }

        bool RecipeFilled()
        {
            return woodHave >= woodNeed && stoneHave >= stoneNeed && ironHave >= ironNeed;
        }

        bool TierOffer(out int wood, out int stone, out int iron)
        {
            wood = 0;
            stone = 0;
            iron = 0;
            if (!IsComplete || NeedsRepair)
                return false;
            FortIdleConfig.DefenseTuning defenses = FortWorld.Config.defenses;
            int night = FortWorld.Match == null ? 1 : FortWorld.Match.NightIndex;
            if (PadKind == Kind.Wall)
            {
                if (Tier == 0 && night >= defenses.reinforcedUnlockNight)
                {
                    wood = defenses.reinforcedWood;
                    stone = defenses.reinforcedStone;
                    return true;
                }

                if (Tier == 1 && night >= defenses.stoneUnlockNight)
                {
                    wood = defenses.stoneWood;
                    stone = defenses.stoneStone;
                    iron = defenses.stoneIron;
                    return true;
                }
            }

            if (PadKind == Kind.Gate)
            {
                if (Tier == 0 && night >= defenses.gateLv2UnlockNight)
                {
                    wood = defenses.gateLv2Wood;
                    iron = defenses.gateLv2Iron;
                    return true;
                }

                if (Tier == 1 && night >= defenses.gateLv3UnlockNight)
                {
                    wood = defenses.gateLv3Wood;
                    stone = defenses.gateLv3Stone;
                    iron = defenses.gateLv3Iron;
                    return true;
                }
            }

            if (PadKind == Kind.Watchtower)
            {
                if (Tier == 0 && night >= defenses.towerLv2UnlockNight)
                {
                    wood = defenses.towerLv2Wood;
                    stone = defenses.towerLv2Stone;
                    return true;
                }

                if (Tier == 1 && night >= defenses.towerLv3UnlockNight)
                {
                    wood = defenses.towerLv3Wood;
                    iron = defenses.towerLv3Iron;
                    return true;
                }
            }

            return false;
        }

        bool TierFilled(int wood, int stone, int iron)
        {
            return tierWood >= wood && tierStone >= stone && tierIron >= iron;
        }

        void FinishBuild()
        {
            IsComplete = true;
            HitPoints = MaxHitPoints;
            standing = true;
            StartCoroutine(BecomeActive());
        }

        void FinishTier()
        {
            FortIdleConfig.DefenseTuning defenses = FortWorld.Config.defenses;
            Tier += 1;
            tierWood = 0;
            tierStone = 0;
            tierIron = 0;
            if (PadKind == Kind.Wall)
            {
                MaxHitPoints = Tier == 1 ? defenses.reinforcedHitPoints : defenses.stoneHitPoints;
                HitPoints = MaxHitPoints;
                standing = true;
                ApplyTierLook();
            }
            else if (PadKind == Kind.Gate)
            {
                MaxHitPoints = Tier == 1 ? defenses.gateLv2HitPoints : defenses.gateLv3HitPoints;
                HitPoints = MaxHitPoints;
                standing = true;
                DressGate();
            }
            else if (PadKind == Kind.Watchtower)
            {
                standing = true;
                if (turret != null && Tier == 1)
                    turret.RankUp(defenses.towerRange, defenses.towerLv2RateScale, 1f, false);
                else if (turret != null)
                    turret.RankUp(defenses.towerLv3Range, 1f, defenses.towerLv3DamageScale, true);
                DressTower();
            }

            if (structureCollider != null)
            {
                ShoveClear();
                structureCollider.enabled = true;
            }

            StagedBuild target = upgradeTarget != null ? upgradeTarget : staged;
            if (target != null)
            {
                target.Sync(Tier, 1, 1, UpgradeStageCount(), false);
                if (!FortSave.Restoring)
                    StartCoroutine(target.PlayBuilt());
            }
            if (FortWorld.Progress != null)
                FortWorld.Progress.NoteReward(PadKind.ToString().ToLowerInvariant() + "-l" + (Tier + 1));
            Debug.Log("FORTIDLE tier=" + PadKind + " level=" + (Tier + 1) + " hp=" + Mathf.CeilToInt(MaxHitPoints));
            RefreshVisuals(true);
        }

        void DressGate()
        {
            if (structureMesh != null && haveBorn)
            {
                float mul = Tier >= 2 ? 1.2f : 1.08f;
                structureMesh.localScale = new Vector3(bornScale.x, bornScale.y * mul, bornScale.z);
                baseMeshScale = structureMesh.localScale;
            }

            if (staged != null)
                return;
            if (structurePivot == null || structurePivot.Find("Band" + Tier) != null)
                return;
            FortIdleConfig.DefenseTuning defenses = FortWorld.Config.defenses;
            GameObject band = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Iron, structurePivot, false);
            band.name = "Band" + Tier;
            band.transform.localScale = new Vector3(defenses.gateSize.x * 0.94f, 0.16f, 0.28f);
            band.transform.localPosition = new Vector3(0f, 0.7f + Tier * 0.45f, 0f);
            if (Tier < 2)
                return;
            for (int i = 0; i < 4; i++)
            {
                GameObject spike = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Spike, structurePivot, false);
                spike.name = "Portcullis";
                spike.transform.localScale = new Vector3(0.12f, 0.42f, 0.12f);
                spike.transform.localPosition = new Vector3(-1.2f + i * 0.8f, 0.55f, 0.2f);
            }
        }

        void DressTower()
        {
            if (staged != null || structurePivot == null)
                return;
            float scale = Tier >= 2 ? 1.22f : 1.1f;
            structurePivot.localScale = new Vector3(scale, scale, scale);
            if (Tier < 2 || structurePivot.Find("Banner") != null)
                return;
            GameObject banner = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Fire, structurePivot, false);
            banner.name = "Banner";
            banner.transform.localScale = new Vector3(0.7f, 0.12f, 0.7f);
            banner.transform.localPosition = new Vector3(0f, FortWorld.Config.defenses.towerHeight + 0.35f, 0f);
        }

        public void RestoreSpike(int tier)
        {
            if (!IsComplete)
            {
                IsComplete = true;
                woodHave = woodNeed;
                Progress = Cost;
                if (structurePivot != null)
                {
                    structurePivot.gameObject.SetActive(true);
                    structurePivot.localScale = Vector3.one;
                }

                NoteBuildStage(false);
            }

            while (Tier < tier)
                PromoteSpike();
        }

        public void Restore(int tier, float hp, bool built)
        {
            if (!built)
                return;
            IsComplete = true;
            woodHave = woodNeed;
            stoneHave = stoneNeed;
            ironHave = ironNeed;
            Progress = Cost;
            standing = true;
            if (structurePivot != null)
            {
                structurePivot.gameObject.SetActive(true);
                structurePivot.localScale = Vector3.one;
            }

            NoteBuildStage(false);
            if (structureCollider != null)
            {
                ShoveClear();
                structureCollider.enabled = true;
            }

            if (turret != null)
                turret.enabled = true;
            int steps = tier;
            for (int i = 0; i < steps; i++)
                FinishTier();
            if (MaxHitPoints > 0f && hp >= 0f)
            {
                HitPoints = Mathf.Min(hp, MaxHitPoints);
                if (HitPoints <= 0f)
                {
                    standing = false;
                    if (structureCollider != null)
                        structureCollider.enabled = false;
                    if (turret != null)
                        turret.enabled = false;
                }
            }

            RefreshVisuals(false);
        }

        void PromoteSpike()
        {
            Tier += 1;
            if (staged != null)
                staged.Sync(Tier, 1, 1, 1, true);
            else if (structurePivot != null)
            {
                float scale = Tier >= 2 ? 1.28f : 1.12f;
                structurePivot.localScale = new Vector3(scale, scale, scale);
                Renderer[] renderers = structurePivot.GetComponentsInChildren<Renderer>();
                Material material = Tier >= 2 ? FortWorld.Look.Iron : FortWorld.Look.Bone;
                for (int i = 0; i < renderers.Length; i++)
                    renderers[i].sharedMaterial = material;
            }

            if (FortWorld.Progress != null)
                FortWorld.Progress.NoteReward("spike-l" + (Tier + 1));
            Debug.Log("FORTIDLE spike=l" + (Tier + 1));
        }

        bool TrySpecial()
        {
            FortProgress progress = FortWorld.Progress;
            FortIdleConfig.DefenseTuning defenses = FortWorld.Config.defenses;
            if (progress == null)
                return false;
            if (PadKind == Kind.Spike && Tier <= 0 && progress.SpendBones(defenses.spikeLv2Bones))
            {
                PromoteSpike();
                return true;
            }

            if (PadKind == Kind.Spike && Tier == 1
                && progress.Bones >= defenses.spikeLv3Bones
                && progress.Iron >= defenses.spikeLv3Iron
                && progress.SpendBones(defenses.spikeLv3Bones)
                && progress.SpendIron(defenses.spikeLv3Iron))
            {
                PromoteSpike();
                return true;
            }

            if (PadKind == Kind.TurretUpgrade && FortWorld.TurretWeapon != null && FortWorld.TurretWeapon.Level < 3
                && progress.SpendSap(defenses.turretLv3Sap))
            {
                FortWorld.TurretWeapon.ApplyLevel(3);
                if (upgradeTarget != null)
                    upgradeTarget.Sync(2, 1, 1, 1, true);
                progress.NoteReward("turret-l3");
                Debug.Log("FORTIDLE turret=fire");
                return true;
            }

            return false;
        }

        void TickSpecial(bool near)
        {
            if (!near || !WantsSpecial)
            {
                specialDwell = 0f;
                specialSpent = false;
                return;
            }

            specialDwell += Time.deltaTime;
            float delay = FortWorld.Config.upgrades.payDelay;
            if (specialSpent)
            {
                if (specialDwell > delay + 0.9f)
                {
                    specialSpent = false;
                    specialDwell = 0f;
                }

                return;
            }

            if (specialDwell >= delay && TrySpecial())
                specialSpent = true;
        }

        void ApplyTierLook()
        {
            if (structureRenderer == null || structureMesh == null || PadKind != Kind.Wall)
                return;
            Material material = FortWorld.Look.Wall;
            float height = 1f;
            if (Tier == 1 && FortWorld.Look.WallReinforced != null)
            {
                material = FortWorld.Look.WallReinforced;
                height = 1.08f;
            }
            else if (Tier >= 2 && FortWorld.Look.WallStone != null)
            {
                material = FortWorld.Look.WallStone;
                height = 1.22f;
            }

            structureMesh.localScale = new Vector3(baseMeshScale.x, baseMeshScale.y * height, baseMeshScale.z);
            baseMeshScale = structureMesh.localScale;
            if (staged != null)
                return;
            structureRenderer.sharedMaterial = material;
            RefreshDress();
        }

        void ShoveClear()
        {
            if (structureCollider == null)
                return;
            ShoveBody(FortWorld.Hero, FortWorld.Config.player.radius);
            for (int i = 0; i < FortWorld.Workers.Count; i++)
                ShoveBody(FortWorld.Workers[i].transform, FortWorld.Config.workers.bodyRadius);
        }

        void ShoveBody(Transform body, float radius)
        {
            if (body == null)
                return;
            Bounds bounds = structureCollider.bounds;
            float pad = radius + 0.15f;
            Vector3 point = body.position;
            if (point.x < bounds.min.x - pad || point.x > bounds.max.x + pad || point.z < bounds.min.z - pad || point.z > bounds.max.z + pad)
                return;

            float left = point.x - bounds.min.x;
            float right = bounds.max.x - point.x;
            float south = point.z - bounds.min.z;
            float north = bounds.max.z - point.z;
            float best = left;
            Vector3 parked = point;
            parked.x = bounds.min.x - pad;
            if (right < best)
            {
                best = right;
                parked = point;
                parked.x = bounds.max.x + pad;
            }

            if (south < best)
            {
                best = south;
                parked = point;
                parked.z = bounds.min.z - pad;
            }

            if (north < best)
            {
                parked = point;
                parked.z = bounds.max.z + pad;
            }

            CharacterController controller = body.GetComponent<CharacterController>();
            if (controller != null)
                controller.enabled = false;
            body.position = parked;
            if (controller != null)
                controller.enabled = true;
        }

        void RaiseIfHeld()
        {
            if (HitPoints <= 0f || structureMesh == null)
                return;
            float fraction = MaxHitPoints <= 0f ? 1f : HitPoints / MaxHitPoints;
            standing = fraction >= 0.2f;
            structureMesh.localScale = new Vector3(baseMeshScale.x, baseMeshScale.y * Mathf.Max(fraction, 0.05f), baseMeshScale.z);
            if (staged != null)
                staged.SetStrain(Mathf.Max(fraction, 0.05f));
            if (structureCollider != null)
            {
                if (standing)
                    ShoveClear();
                structureCollider.enabled = standing;
            }
            if (structurePivot != null)
                structurePivot.gameObject.SetActive(true);
        }

        void Update()
        {
            bool near = false;
            if (IsUnlocked && FortWorld.Hero != null)
            {
                float distance = FortMath.HorizontalDistance(transform.position, FortWorld.Hero.position);
                near = distance <= FortWorld.Config.pour.padRadius;
            }

            bool occupied = near && (WantsWood || WantsStone || WantsIron || WantsSpecial);
            TickSpecial(near);
            RefreshVisuals(occupied);
        }

        void RefreshVisuals(bool occupied)
        {
            bool open = IsUnlocked;
            if (discRenderer != null)
                discRenderer.enabled = open;
            if (label != null)
                label.gameObject.SetActive(open && !QuietLabel);
            if (!open)
                return;

            if (discRenderer != null)
            {
                if (discMaterial == null)
                    discMaterial = discRenderer.material;
                Color color = idleColor;
                if (NeedsRepair)
                {
                    float pulse = 0.55f + 0.45f * Mathf.Sin(Time.time * 6f);
                    color = Color.Lerp(idleColor, FortWorld.Config.presentation.padRepair, pulse);
                }
                else if (!IsComplete)
                    color = occupied ? FortWorld.Config.presentation.padActive : idleColor;
                else if (WantsWood || WantsStone || WantsIron)
                    color = occupied ? FortWorld.Config.presentation.padActive : idleColor;
                else
                    color = FortWorld.Config.presentation.padComplete;
                discMaterial.SetColor("_BaseColor", color);
            }

            if (fillBar != null)
            {
                float fraction;
                if (!IsComplete)
                    fraction = Cost <= 0 ? 0f : Progress / Cost;
                else if (MaxHitPoints > 0f && (repairable || PadKind == Kind.Wall))
                    fraction = HitPoints / MaxHitPoints;
                else if (WantsWood || WantsStone || WantsIron)
                    fraction = StageFraction();
                else
                    fraction = 1f;

                Vector3 size = FortWorld.Config.presentation.fillBarSize;
                float height = FortWorld.Config.presentation.fillBarHeight * Mathf.Clamp01(fraction);
                fillBar.localScale = new Vector3(size.x, Mathf.Max(height, 0.01f), size.z);
                fillBar.localPosition = new Vector3(fillBar.localPosition.x, height * 0.5f, fillBar.localPosition.z);
            }

            if (label == null)
                return;
            int signature = (IsComplete ? 1 : 0)
                + (NeedsRepair ? 2 : 0)
                + (standing ? 4 : 0)
                + (WantsSpecial ? 8 : 0)
                + (WantsWood ? 16 : 0)
                + (WantsStone ? 32 : 0)
                + (WantsIron ? 64 : 0)
                + (Tier << 8)
                + (StageShown << 12);
            int amount = !IsComplete ? Mathf.FloorToInt(Progress) : Mathf.CeilToInt(HitPoints);
            int extra = Cost + WoodRemaining + StoneRemaining + IronRemaining;
            if (signature == captionSignature && amount == captionAmount && extra == captionExtra)
                return;
            captionSignature = signature;
            captionAmount = amount;
            captionExtra = extra;
            label.SetText(Caption());
        }

        Material discMaterial;
        int captionSignature = int.MinValue;
        int captionAmount = int.MinValue;
        int captionExtra = int.MinValue;

        float StageFraction()
        {
            int wood;
            int stone;
            int iron;
            if (!IsComplete)
            {
                int need = woodNeed + stoneNeed + ironNeed;
                if (need <= 0)
                    return 0f;
                return (woodHave + stoneHave + ironHave) / (float)need;
            }

            if (!TierOffer(out wood, out stone, out iron))
                return 1f;
            int tierNeed = wood + stone + iron;
            if (tierNeed <= 0)
                return 1f;
            return (tierWood + tierStone + tierIron) / (float)tierNeed;
        }

        string Caption()
        {
            if (!IsComplete)
            {
                string line = Name() + "\n" + Mathf.FloorToInt(Progress) + "/" + Cost;
                int total = PadKind == Kind.TurretUpgrade ? UpgradeStageCount() : BaseStageCount();
                if (total > 0)
                    line += "\n" + StageShown + "/" + total;
                return line;
            }
            if (NeedsRepair)
                return Name() + "\n" + Mathf.CeilToInt(HitPoints) + "/" + Mathf.CeilToInt(MaxHitPoints);
            if (repairable && !standing)
                return Name() + "\nDOWN";
            if (WantsSpecial)
                return Name() + "\n" + (PadKind == Kind.TurretUpgrade ? FortWorld.Config.copy.sapLabel : FortWorld.Config.copy.boneLabel);
            if (WantsWood || WantsStone || WantsIron)
                return Name() + "\n" + (WoodRemaining + StoneRemaining + IronRemaining);
            if (repairable)
                return Name() + "\n" + Mathf.CeilToInt(HitPoints) + "/" + Mathf.CeilToInt(MaxHitPoints);
            return Name() + "\nREADY";
        }

        string Name()
        {
            FortIdleConfig.CopyTuning copy = FortWorld.Config.copy;
            if (PadKind == Kind.Wall)
            {
                if (Tier >= 2)
                    return copy.stoneWallName;
                if (Tier == 1)
                    return copy.reinforcedName;
                return copy.wallName;
            }

            if (PadKind == Kind.Hut)
                return copy.hutName;
            if (PadKind == Kind.Spike)
                return copy.spikeName;
            if (PadKind == Kind.Gate)
                return copy.gateName;
            if (PadKind == Kind.Watchtower)
                return copy.towerName;
            if (PadKind == Kind.TurretUpgrade)
                return copy.upgradeName;
            return copy.turretName;
        }

        void WakeStructure()
        {
            if (structurePivot == null)
                return;
            if (!structurePivot.gameObject.activeSelf)
            {
                structurePivot.gameObject.SetActive(true);
                structurePivot.localScale = Vector3.one;
            }
        }

        void NoteBuildStage(bool animate)
        {
            if (PadKind == Kind.TurretUpgrade && upgradeTarget != null)
            {
                int count = UpgradeStageCount();
                int before = upgradeTarget.Shown(1);
                upgradeTarget.Sync(1, woodHave + stoneHave + ironHave, Cost, count, animate);
                int after = upgradeTarget.Shown(1);
                if (animate && after > before)
                    Debug.Log("FORTIDLE stage=" + PadKind + " n=" + after + "/" + count);
                PaintTicks(after, count);
                return;
            }

            if (staged == null)
                return;
            WakeStructure();
            int stages = BaseStageCount();
            int had = staged.Shown(0);
            staged.Sync(0, woodHave + stoneHave + ironHave, Cost, stages, animate);
            int got = staged.Shown(0);
            if (animate && got > had)
                Debug.Log("FORTIDLE stage=" + PadKind + " n=" + got + "/" + stages);
            PaintTicks(got, stages);
        }

        void NoteUpgradeStage(bool animate)
        {
            StagedBuild target = upgradeTarget != null ? upgradeTarget : staged;
            if (target == null)
                return;
            int wood;
            int stone;
            int iron;
            if (!TierOffer(out wood, out stone, out iron))
                return;
            int cost = wood + stone + iron;
            if (cost < 1)
                return;
            int layer = Tier + 1;
            int count = UpgradeStageCount();
            int before = target.Shown(layer);
            target.Sync(layer, tierWood + tierStone + tierIron, cost, count, animate);
            int after = target.Shown(layer);
            if (animate && after > before)
                Debug.Log("FORTIDLE stage=" + PadKind + " up=" + after + "/" + count);
            PaintTicks(after, count);
        }

        int BaseStageCount()
        {
            FortIdleConfig.StageTuning tune = FortWorld.Config.stages;
            if (PadKind == Kind.Gate)
                return tune.gate;
            if (PadKind == Kind.Watchtower)
                return tune.tower;
            if (PadKind == Kind.Turret)
                return tune.turret;
            if (PadKind == Kind.Spike)
                return tune.spike;
            if (PadKind == Kind.Hut)
                return tune.hut;
            return tune.wall;
        }

        int UpgradeStageCount()
        {
            FortIdleConfig.StageTuning tune = FortWorld.Config.stages;
            if (PadKind == Kind.Gate)
                return tune.gateUpgrade;
            if (PadKind == Kind.Watchtower)
                return tune.towerUpgrade;
            if (PadKind == Kind.Turret || PadKind == Kind.TurretUpgrade)
                return tune.turretUpgrade;
            if (PadKind == Kind.Spike)
                return 1;
            return tune.wallUpgrade;
        }

        void PaintTicks(int shownCount, int total)
        {
            if (total < 1)
                return;
            if (ticks == null || ticks.childCount != total)
            {
                if (ticks != null)
                    Destroy(ticks.gameObject);
                GameObject row = new GameObject("StageTicks");
                row.transform.SetParent(transform, false);
                ticks = row.transform;
                for (int i = 0; i < total; i++)
                {
                    GameObject tick = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Dirt, ticks, false);
                    tick.name = "Tick";
                    float x = (i - (total - 1) * 0.5f) * 0.28f;
                    tick.transform.localPosition = new Vector3(x, 0.22f, -1.35f);
                    tick.transform.localScale = new Vector3(0.24f, 0.1f, 0.24f);
                }
            }

            for (int i = 0; i < ticks.childCount; i++)
            {
                Renderer renderer = ticks.GetChild(i).GetComponent<Renderer>();
                if (renderer == null)
                    continue;
                Color color = i < shownCount ? new Color(0.96f, 0.82f, 0.28f, 1f) : new Color(0.1f, 0.1f, 0.1f, 1f);
                renderer.material.SetColor("_BaseColor", color);
            }
        }

        IEnumerator BecomeActive()
        {
            if (PadKind == Kind.Wall && Tier == 0 && FortWorld.Rig != null && !StageShowcase.Requested())
                FortWorld.Rig.BeginTease(FortWorld.Config.layout.lockedAreaPosition);
            WakeStructure();
            StagedBuild pop = PadKind == Kind.TurretUpgrade ? upgradeTarget : staged;
            if (pop != null)
                yield return pop.PlayBuilt();
            if (structurePivot != null)
                structurePivot.localScale = Vector3.one;
            if (structureCollider != null && standing)
            {
                ShoveClear();
                structureCollider.enabled = true;
            }
            if (turret != null)
                turret.enabled = true;
            if (PadKind == Kind.TurretUpgrade && FortWorld.TurretWeapon != null)
            {
                FortWorld.TurretWeapon.ApplyLevel(2);
                if (FortWorld.Progress != null)
                    FortWorld.Progress.NoteReward("turret-l2");
                Debug.Log("FORTIDLE turret=upgraded");
            }

            if (FortWorld.Progress != null && PadKind != Kind.TurretUpgrade)
                FortWorld.Progress.NoteReward(PadKind.ToString().ToLowerInvariant() + "-l1");

            if (PadKind == Kind.Hut)
                Debug.Log("FORTIDLE hut=ready");
            if (PadKind == Kind.Spike)
                Debug.Log("FORTIDLE spike=ready");
            if (PadKind == Kind.Gate)
                Debug.Log("FORTIDLE gate=ready");
            if (PadKind == Kind.Watchtower)
                Debug.Log("FORTIDLE tower=ready");

            float hitStop = FortWorld.Config.presentation.hitStopSeconds;
            if (hitStop > 0f)
            {
                float previous = Time.timeScale;
                Time.timeScale = FortWorld.Config.presentation.hitStopScale;
                yield return new WaitForSecondsRealtime(hitStop);
                Time.timeScale = previous;
            }
        }
    }
}
