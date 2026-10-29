using System.Collections.Generic;
using UnityEngine;

namespace FortIdle
{
    public enum EliteAffix
    {
        None,
        Armored,
        Swift,
        Burning,
        Shielded,
        Splitter,
        Regen
    }

    public struct AffixState
    {
        public int Tier;
        public EliteAffix Affix;
        public bool Splitling;
        public int ShieldLeft;
        public float LastHurt;
        public float Scale;
        public bool Painted;
    }

    // Elite affixes and visual tiers (MONSTER_ROSTER, PROGRESSION_ECONOMY 2.5). Numbers live in FortIdleConfig.elites.
    public static class MonsterAffix
    {
        static readonly List<Renderer> renderers = new List<Renderer>(32);
        static readonly System.Func<GameObject> createShambler = CreateShambler;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");
        static MaterialPropertyBlock block;

        public static float EliteChance(int night)
        {
            FortIdleConfig.EliteTuning tune = FortWorld.Config.elites;
            if (night <= tune.eliteStartNight)
                return 0f;
            return Mathf.Clamp(tune.elitePerNight * (night - tune.eliteStartNight), 0f, tune.eliteMax);
        }

        public static int FodderTier(float hpx, float threat)
        {
            FortIdleConfig.EliteTuning tune = FortWorld.Config.elites;
            int tier = 1;
            if (tune.championAtHpx > 0f && hpx >= tune.championAtHpx)
                tier = 3;
            else if (tune.tierPromoteAtHpx > 0f && hpx >= tune.tierPromoteAtHpx)
                tier = 2;
            return Mathf.Max(tier, ThreatTier(threat));
        }

        public static int ThreatTier(float threat)
        {
            FortIdleConfig.EliteTuning tune = FortWorld.Config.elites;
            return tune.veteranAtThreat > 0f && threat >= tune.veteranAtThreat ? 2 : 1;
        }

        public static EliteAffix Roll(float chance)
        {
            if (chance <= 0f || Random.value >= chance)
                return EliteAffix.None;
            FortIdleConfig.EliteTuning tune = FortWorld.Config.elites;
            float armored = Mathf.Max(0f, tune.armoredWeight);
            float swift = Mathf.Max(0f, tune.swiftWeight);
            float burning = Mathf.Max(0f, tune.burningWeight);
            float shielded = Mathf.Max(0f, tune.shieldedWeight);
            float splitter = Mathf.Max(0f, tune.splitterWeight);
            float regen = Mathf.Max(0f, tune.regenWeight);
            float total = armored + swift + burning + shielded + splitter + regen;
            if (total <= 0f)
                return EliteAffix.None;
            float pick = Random.value * total;
            if ((pick -= armored) < 0f)
                return EliteAffix.Armored;
            if ((pick -= swift) < 0f)
                return EliteAffix.Swift;
            if ((pick -= burning) < 0f)
                return EliteAffix.Burning;
            if ((pick -= shielded) < 0f)
                return EliteAffix.Shielded;
            if ((pick -= splitter) < 0f)
                return EliteAffix.Splitter;
            if (regen > 0f)
                return EliteAffix.Regen;
            if (splitter > 0f)
                return EliteAffix.Splitter;
            if (shielded > 0f)
                return EliteAffix.Shielded;
            if (burning > 0f)
                return EliteAffix.Burning;
            return swift > 0f ? EliteAffix.Swift : EliteAffix.Armored;
        }

        public static void Reset(ref AffixState state)
        {
            state.Tier = 1;
            state.Affix = EliteAffix.None;
            state.Splitling = false;
            state.ShieldLeft = 0;
            state.LastHurt = 0f;
            state.Scale = 1f;
        }

        public static void Begin(ref AffixState state, int tier, EliteAffix affix, bool splitling)
        {
            FortIdleConfig.EliteTuning tune = FortWorld.Config.elites;
            state.Tier = Mathf.Clamp(tier, 1, 3);
            state.Affix = affix;
            state.Splitling = splitling;
            if (affix != EliteAffix.None && state.Tier < tune.eliteMinTier)
                state.Tier = Mathf.Clamp(tune.eliteMinTier, 1, 3);
            state.ShieldLeft = affix == EliteAffix.Shielded ? tune.shieldHits : 0;
            state.LastHurt = Time.time;
            float scale = 1f + tune.tierScaleStep * (state.Tier - 1);
            if (affix != EliteAffix.None)
                scale += tune.eliteScale;
            if (splitling)
                scale *= tune.splitScale;
            state.Scale = scale <= 0f ? 1f : scale;
        }

        public static float HpMult(EliteAffix affix)
        {
            return affix == EliteAffix.Armored ? FortWorld.Config.elites.armoredHp : 1f;
        }

        public static float SpeedMult(EliteAffix affix)
        {
            return affix == EliteAffix.Swift ? FortWorld.Config.elites.swiftSpeed : 1f;
        }

        public static bool BurnImmune(AffixState state)
        {
            return state.Affix == EliteAffix.Burning;
        }

        // True when a Shielded elite blocks the hit. Ticks (burn, spikes) pass through and do not use up the shield.
        public static bool Absorb(ref AffixState state, bool tick)
        {
            state.LastHurt = Time.time;
            if (tick || state.ShieldLeft <= 0)
                return false;
            state.ShieldLeft -= 1;
            return true;
        }

        public static float Regen(ref AffixState state, float hitPoints, float maxHitPoints, float delta)
        {
            if (state.Affix != EliteAffix.Regen || hitPoints >= maxHitPoints)
                return hitPoints;
            FortIdleConfig.EliteTuning tune = FortWorld.Config.elites;
            if (Time.time - state.LastHurt < tune.regenCalmSeconds)
                return hitPoints;
            float healed = hitPoints + maxHitPoints * tune.regenPerSecond * delta;
            return healed > maxHitPoints ? maxHitPoints : healed;
        }

        public static void Split(AffixState state, Vector3 origin, float parentMaxHp, int slot, int slotCount)
        {
            if (state.Affix != EliteAffix.Splitter)
                return;
            FortIdleConfig config = FortWorld.Config;
            FortIdleConfig.EliteTuning tune = config.elites;
            int live = 0;
            int kids = 0;
            for (int i = 0; i < FortWorld.Monsters.Count; i++)
            {
                Monster other = FortWorld.Monsters[i];
                if (other == null || !other.Alive)
                    continue;
                live += 1;
                if (other.IsSplitling)
                    kids += 1;
            }

            float baseHp = config.monsters.hitPoints < 1f ? 60f : config.monsters.hitPoints;
            float hpScale = Mathf.Max(0.01f, parentMaxHp * tune.splitHpShare / baseHp);
            int poolKey = 400 + (int)MonsterKind.Shambler;
            for (int c = 0; c < tune.splitCount; c++)
            {
                if (live >= config.progression.countCap || kids >= tune.splitMaxLive)
                    return;
                float side = tune.splitCount <= 1 ? 0f : (c / (float)(tune.splitCount - 1) - 0.5f) * 2f;
                GameObject go = FortPool.Take(poolKey, createShambler);
                go.transform.position = origin + new Vector3(side * tune.splitSpread, 0f, 0f);
                Monster monster = go.GetComponent<Monster>();
                if (monster == null)
                    monster = go.AddComponent<Monster>();
                monster.Init(MonsterKind.Shambler, slot, slotCount, hpScale, 1f);
                monster.Promote(1, EliteAffix.None, true);
                FortWorld.Monsters.Add(monster);
                live += 1;
                kids += 1;
            }
        }

        static GameObject CreateShambler()
        {
            return new GameObject(MonsterKind.Shambler.ToString());
        }

        public static Color Tint(AffixState state)
        {
            FortIdleConfig.EliteTuning tune = FortWorld.Config.elites;
            Color tint = Color.white;
            if (state.Tier == 2)
                tint = tune.tier2Tint;
            else if (state.Tier >= 3)
                tint = tune.tier3Tint;
            if (state.Affix == EliteAffix.Armored)
                tint *= tune.armoredTint;
            else if (state.Affix == EliteAffix.Swift)
                tint *= tune.swiftTint;
            else if (state.Affix == EliteAffix.Burning)
                tint *= tune.burningTint;
            else if (state.Affix == EliteAffix.Shielded)
                tint *= tune.shieldedTint;
            else if (state.Affix == EliteAffix.Splitter)
                tint *= tune.splitterTint;
            else if (state.Affix == EliteAffix.Regen)
                tint *= tune.regenTint;
            tint.a = 1f;
            return tint;
        }

        // Tints every body renderer with one shared MaterialPropertyBlock. Plain tier-1 bodies get the block cleared so they stay SRP-batched.
        public static void Paint(ref AffixState state, Transform root, Transform skip)
        {
            bool plain = state.Tier <= 1 && state.Affix == EliteAffix.None;
            if (plain && !state.Painted)
                return;
            if (block == null)
                block = new MaterialPropertyBlock();
            Color tint = Tint(state);
            root.GetComponentsInChildren(true, renderers);
            for (int i = 0; i < renderers.Count; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || (skip != null && renderer.transform.IsChildOf(skip)))
                    continue;
                if (plain)
                {
                    renderer.SetPropertyBlock(null);
                    continue;
                }

                Material material = renderer.sharedMaterial;
                Color baseColor = Color.white;
                int id = BaseColorId;
                if (material != null && material.HasProperty(BaseColorId))
                    baseColor = material.GetColor(BaseColorId);
                else if (material != null && material.HasProperty(ColorId))
                {
                    baseColor = material.GetColor(ColorId);
                    id = ColorId;
                }

                Color final = baseColor * tint;
                final.a = baseColor.a;
                block.Clear();
                block.SetColor(id, final);
                renderer.SetPropertyBlock(block);
            }

            renderers.Clear();
            state.Painted = !plain;
        }
    }
}
