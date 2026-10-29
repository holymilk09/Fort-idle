using UnityEngine;

namespace FortIdle
{
    public enum MonsterKind
    {
        Shambler,
        Brute,
        Treant
    }

    public class Monster : MonoBehaviour
    {
        public bool Alive => alive;

        public MonsterKind Kind => kind;

        public Vector3 Center => transform.position + Vector3.up * (bodyHeight * 0.5f);

        MonsterKind kind;
        int slot;
        int slotCount;
        bool alive = true;
        bool dying;
        float hitPoints;
        float attackTimer;
        float deathAge;
        float hitFlash;
        float moveSpeed;
        float attackDamage;
        float bodyRadius;
        float bodyHeight;
        float separationRadius;
        float maxHitPoints;
        float burnLeft;
        float burnDps;
        float slowUntil;
        float slowFactor = 1f;
        Vector3 deathOrigin;
        HpBar bar;
        bool dressed;
        MonsterKind dressedKind;
        int thinkTick;
        BuildPad thought;
        bool thoughtFlank;
        float baseScale = 1f;
        float artHeight;
        AffixState affixState;
        bool ticking;

        public float HealthFraction => !alive || maxHitPoints <= 0f ? 0f : hitPoints / maxHitPoints;

        public bool IsBoss => alive && kind == MonsterKind.Treant;

        public int Tier => affixState.Tier < 1 ? 1 : affixState.Tier;

        public EliteAffix Affix => affixState.Affix;

        public bool IsSplitling => affixState.Splitling;

        public float AffixScale => affixState.Scale <= 0f ? 1f : affixState.Scale;

        public void Init(MonsterKind monsterKind, int index, int count, float hpScale = 1f, float sizeScale = 1f)
        {
            kind = monsterKind;
            slot = index;
            slotCount = count < 1 ? 1 : count;
            FortIdleConfig.MonsterTuning tuning = FortWorld.Config.monsters;
            if (kind == MonsterKind.Brute)
            {
                hitPoints = tuning.bruteHitPoints;
                moveSpeed = tuning.bruteMoveSpeed;
                attackDamage = tuning.bruteAttackDamage;
                bodyRadius = tuning.bruteRadius;
                bodyHeight = tuning.bruteHeight;
                separationRadius = tuning.bruteRadius * 2f;
            }
            else if (kind == MonsterKind.Treant)
            {
                hitPoints = tuning.treantHitPoints;
                moveSpeed = tuning.treantMoveSpeed;
                attackDamage = tuning.treantAttackDamage;
                bodyRadius = tuning.treantTrunkDiameter * 0.5f;
                bodyHeight = tuning.treantCanopyHeight + tuning.treantCanopyDiameter * 0.5f;
                separationRadius = tuning.treantCanopyDiameter * 0.7f;
            }
            else
            {
                hitPoints = tuning.hitPoints;
                moveSpeed = tuning.moveSpeed;
                attackDamage = tuning.attackDamage;
                bodyRadius = tuning.radius;
                bodyHeight = tuning.height;
                separationRadius = tuning.separationRadius;
            }

            if (hpScale > 0f && Mathf.Abs(hpScale - 1f) > 0.001f)
                hitPoints *= hpScale;
            alive = true;
            dying = false;
            deathAge = 0f;
            burnLeft = 0f;
            hitFlash = 0f;
            attackTimer = 0f;
            maxHitPoints = hitPoints;
            transform.localScale = Vector3.one;
            bool fresh = !dressed || dressedKind != kind;
            if (fresh)
            {
                dressed = true;
                dressedKind = kind;
                BuildBody();
                artHeight = RenderHeight();
            }

            float scale = sizeScale < 1f ? 1f : sizeScale;
            float fit = scale;
            float bossHeight = tuning.bossHeight;
            if (kind == MonsterKind.Treant && bossHeight > 0f && artHeight > 0.01f && bodyHeight > 0.01f)
            {
                scale = bossHeight / artHeight;
                fit = bossHeight / bodyHeight;
            }

            if (fresh)
            {
                bar = HpBar.Create(transform, bodyHeight * fit / scale + 0.35f, kind == MonsterKind.Treant ? 1.8f : 0.8f);
                bar.Read = () => HealthFraction;
            }

            bodyRadius *= fit;
            bodyHeight *= fit;
            separationRadius *= fit;
            baseScale = scale;
            transform.localScale = Vector3.one * baseScale;
            if (kind == MonsterKind.Treant)
                Debug.Log("FORTIDLE boss height=" + bodyHeight.ToString("0.00") + "m art=" + artHeight.ToString("0.00") + "m scale=" + baseScale.ToString("0.000"));
            MonsterAffix.Reset(ref affixState);
            MonsterAffix.Paint(ref affixState, transform, bar != null ? bar.transform : null);
        }

        // Elite / tier hook. Call right after Init. AffixScale multiplies the Init size.
        public void Promote(int tier, EliteAffix affix, bool splitling)
        {
            MonsterAffix.Begin(ref affixState, tier, affix, splitling);
            hitPoints *= MonsterAffix.HpMult(affix);
            maxHitPoints = hitPoints;
            moveSpeed *= MonsterAffix.SpeedMult(affix);
            float grow = AffixScale;
            bodyRadius *= grow;
            bodyHeight *= grow;
            separationRadius *= grow;
            baseScale *= grow;
            transform.localScale = Vector3.one * baseScale;
            MonsterAffix.Paint(ref affixState, transform, bar != null ? bar.transform : null);
        }

        public float ScreenFraction(Camera view)
        {
            if (view == null)
                return 0f;
            Vector3 bottom = view.WorldToViewportPoint(transform.position);
            Vector3 top = view.WorldToViewportPoint(transform.position + Vector3.up * bodyHeight);
            if (bottom.z <= 0f || top.z <= 0f)
                return 0f;
            return Mathf.Abs(top.y - bottom.y);
        }

        float RenderHeight()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            bool any = false;
            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (!any)
                {
                    bounds = renderers[i].bounds;
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return any ? bounds.size.y : 0f;
        }

        public void Burn(float dps, float seconds)
        {
            if (!alive || dps <= 0f)
                return;
            if (MonsterAffix.BurnImmune(affixState))
                return;
            if (dps > burnDps)
                burnDps = dps;
            if (seconds > burnLeft)
                burnLeft = seconds;
        }

        public void Stagger(float seconds, float factor)
        {
            slowUntil = Time.time + seconds;
            slowFactor = factor <= 0f ? 0.35f : factor;
        }

        public void TakeDamage(float amount)
        {
            if (!alive)
                return;
            if (MonsterAffix.Absorb(ref affixState, ticking))
            {
                hitFlash = 1f;
                return;
            }

            hitPoints -= amount;
            hitFlash = 1f;
            WorldLabel.SpawnDamage(transform.position + Vector3.up * bodyHeight, amount);
            if (hitPoints > 0f)
                return;
            alive = false;
            dying = true;
            deathAge = 0f;
            hitFlash = 0f;
            deathOrigin = transform.position;
            DropLoot();
            MonsterAffix.Split(affixState, deathOrigin, maxHitPoints, slot, slotCount);
        }

        void DropLoot()
        {
            FortIdleConfig config = FortWorld.Config;
            if (kind == MonsterKind.Brute)
            {
                CoinPickup.ScatterIron(deathOrigin, config.supplies.ironPerBrute);
                int hide = Random.Range(config.drops.hideMin, config.drops.hideMax + 1);
                CoinPickup.ScatterLoot(deathOrigin, DropKind.Hide, hide);
                CoinPickup.ScatterLoot(deathOrigin, DropKind.Core, config.drops.corePerBrute);
            }
            else if (kind == MonsterKind.Shambler)
            {
                if (Random.value < config.drops.meatChance)
                    CoinPickup.ScatterLoot(deathOrigin, DropKind.Meat, 1);
                if (Random.value < config.drops.boneChance)
                    CoinPickup.ScatterLoot(deathOrigin, DropKind.Bone, 1);
            }
            else if (kind == MonsterKind.Treant)
            {
                CoinPickup.ScatterLoot(deathOrigin, DropKind.Core, config.drops.corePerTreant);
                CoinPickup.ScatterLoot(deathOrigin, DropKind.Sap, config.drops.sapPerTreant);
            }

            int coins = Mathf.RoundToInt(maxHitPoints * config.progression.coinPerHp);
            if (coins < 1)
                coins = 1;
            CoinPickup.ScatterCoins(deathOrigin, coins);
        }

        void BuildBody()
        {
            if (kind == MonsterKind.Treant)
                BuildTreant();
            else if (kind == MonsterKind.Brute)
                BuildBrute();
            else
                BuildShambler();
        }

        void BuildShambler()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject dressed = FortArt.Stand(transform, "Shambler", config.monsters.height);
            if (dressed != null)
            {
                FortArt.CastShadows(dressed, false);
                return;
            }
            GameObject body = Prop.Primitive(PrimitiveType.Capsule, FortWorld.Look.Monster, transform, false);
            body.name = "Body";
            body.transform.localScale = Prop.CapsuleScale(config.monsters.radius, config.monsters.height);
            body.transform.localPosition = new Vector3(0f, config.monsters.height * 0.5f, 0f);
            PlaceEye("EyeL", -config.monsters.eyeSeparation * 0.5f, config.monsters.eyeHeight, config.monsters.eyeForward, config.monsters.eyeDiameter);
            PlaceEye("EyeR", config.monsters.eyeSeparation * 0.5f, config.monsters.eyeHeight, config.monsters.eyeForward, config.monsters.eyeDiameter);
        }

        void BuildBrute()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject dressed = FortArt.Stand(transform, "Brute", config.monsters.bruteHeight);
            if (dressed != null)
            {
                FortArt.CastShadows(dressed, false);
                return;
            }
            GameObject body = Prop.Primitive(PrimitiveType.Capsule, FortWorld.Look.Brute, transform, false);
            body.name = "Body";
            body.transform.localScale = Prop.CapsuleScale(config.monsters.bruteRadius, config.monsters.bruteHeight);
            body.transform.localPosition = new Vector3(0f, config.monsters.bruteHeight * 0.5f, 0f);
            float eyeY = config.monsters.bruteHeight * 0.72f;
            float eye = config.monsters.eyeDiameter * 1.6f;
            PlaceEye("EyeL", -config.monsters.bruteHornSpread * 0.45f, eyeY, config.monsters.bruteRadius * 0.7f, eye);
            PlaceEye("EyeR", config.monsters.bruteHornSpread * 0.45f, eyeY, config.monsters.bruteRadius * 0.7f, eye);
            PlaceHorn("HornL", -config.monsters.bruteHornSpread);
            PlaceHorn("HornR", config.monsters.bruteHornSpread);
        }

        void BuildTreant()
        {
            FortIdleConfig config = FortWorld.Config;
            float height = config.monsters.treantCanopyHeight + config.monsters.treantCanopyDiameter * 0.35f;
            bool dressed = FortArt.Stand(transform, "Pine", height, FortArt.TreantTint, 0f, false, false) != null;
            if (!dressed)
            {
                GameObject trunk = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Trunk, transform, false);
                trunk.name = "Trunk";
                trunk.transform.localScale = Prop.CylinderScale(config.monsters.treantTrunkDiameter, config.monsters.treantTrunkHeight);
                trunk.transform.localPosition = new Vector3(0f, config.monsters.treantTrunkHeight * 0.5f, 0f);
                GameObject canopy = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.Canopy, transform, false);
                canopy.name = "Canopy";
                canopy.transform.localScale = Prop.SphereScale(config.monsters.treantCanopyDiameter);
                canopy.transform.localPosition = new Vector3(0f, config.monsters.treantCanopyHeight, 0f);
            }

            float eye = config.monsters.eyeDiameter * 2.2f;
            float eyeZ = config.monsters.treantCanopyDiameter * 0.28f;
            PlaceEye("EyeL", -eye * 1.4f, config.monsters.treantCanopyHeight, eyeZ, eye);
            PlaceEye("EyeR", eye * 1.4f, config.monsters.treantCanopyHeight, eyeZ, eye);
        }

        void PlaceHorn(string hornName, float x)
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject horn = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.BruteHorn, transform, false);
            horn.name = hornName;
            horn.transform.localScale = config.monsters.bruteHornSize;
            horn.transform.localPosition = new Vector3(x, config.monsters.bruteHornHeight, 0.05f);
            float tilt = config.monsters.bruteHornTilt;
            horn.transform.localRotation = Quaternion.Euler(0f, 0f, x < 0f ? tilt : -tilt);
        }

        void PlaceEye(string eyeName, float x, float y, float z, float diameter)
        {
            GameObject eye = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.Eye, transform, false);
            eye.name = eyeName;
            eye.transform.localScale = Prop.SphereScale(diameter);
            eye.transform.localPosition = new Vector3(x, y, z);
        }

        void Update()
        {
            FortIdleConfig config = FortWorld.Config;
            float delta = Time.deltaTime;
            if (!alive)
            {
                if (!dying)
                    return;
                deathAge += delta;
                float span = config.monsters.deathDuration <= 0f ? 1f : config.monsters.deathDuration;
                float u = Mathf.Clamp01(deathAge / span);
                transform.position = deathOrigin + Vector3.down * (config.monsters.deathSinkDistance * u);
                float scale = baseScale * (1f - u);
                transform.localScale = new Vector3(scale, scale, scale);
                if (u >= 1f)
                    gameObject.SetActive(false);
                return;
            }

            if (burnLeft > 0f)
            {
                burnLeft -= delta;
                ticking = true;
                TakeDamage(burnDps * delta);
                ticking = false;
                if (!alive)
                    return;
            }

            hitPoints = MonsterAffix.Regen(ref affixState, hitPoints, maxHitPoints, delta);

            ApplySpikes(delta);
            if (!alive)
                return;
            int stride = 3;
            if (FortWorld.Config.quality != null && FortWorld.Config.quality.monsterThinkStride > 0)
                stride = FortWorld.Config.quality.monsterThinkStride;
            thinkTick += 1;
            if ((thinkTick + slot) % stride == 0)
            {
                Separate(delta * stride);
                thought = FlankTarget();
                thoughtFlank = thought != null;
                if (!thoughtFlank)
                    thought = TargetStructure();
            }

            if (thoughtFlank && thought != null)
            {
                Vector3 face = thought.transform.position - transform.position;
                face.y = 0f;
                Face(face);
                TryAttack(delta, thought);
                ApplyFlash(delta);
                return;
            }

            if (thought != null)
                ApproachStructure(thought, delta);
            else
                ApproachFire(delta);
            ApplyFlash(delta);
        }

        void ApplySpikes(float delta)
        {
            if (delta <= 0f || kind == MonsterKind.Treant)
                return;
            for (int i = 0; i < FortWorld.Pads.Count; i++)
            {
                float dps = FortWorld.Pads[i].SpikeDamage(kind, transform.position);
                if (dps <= 0f)
                    continue;
                ticking = true;
                TakeDamage(dps * delta);
                ticking = false;
                if (!alive)
                    return;
            }
        }

        BuildPad TargetStructure()
        {
            if (kind == MonsterKind.Brute)
                return NearestStructure();
            return StructureAhead();
        }

        BuildPad StructureAhead()
        {
            BuildPad best = null;
            float bestZ = float.NegativeInfinity;
            for (int i = 0; i < FortWorld.Pads.Count; i++)
            {
                BuildPad pad = FortWorld.Pads[i];
                if (!pad.IsBlocking)
                    continue;
                float face = pad.WorldBounds.max.z;
                if (transform.position.z + 0.2f < face)
                    continue;
                if (face > bestZ)
                {
                    bestZ = face;
                    best = pad;
                }
            }

            return best;
        }

        BuildPad NearestStructure()
        {
            BuildPad best = null;
            float bestDistance = float.PositiveInfinity;
            for (int i = 0; i < FortWorld.Pads.Count; i++)
            {
                BuildPad pad = FortWorld.Pads[i];
                if (!pad.IsBlocking)
                    continue;
                float distance = FortMath.HorizontalDistance(transform.position, pad.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = pad;
                }
            }

            return best;
        }

        void ApplyFlash(float delta)
        {
            if (hitFlash <= 0f)
            {
                Vector3 rest = new Vector3(baseScale, baseScale, baseScale);
                if (transform.localScale != rest)
                    transform.localScale = rest;
                return;
            }

            float duration = FortWorld.Config.monsters.hitFlashDuration;
            if (duration <= 0f)
                duration = 0.12f;
            hitFlash -= delta / duration;
            if (hitFlash < 0f)
                hitFlash = 0f;
            float kick = kind == MonsterKind.Treant ? FortWorld.Config.monsters.bossHitFlashScale : FortWorld.Config.monsters.hitFlashScale;
            float punch = baseScale * (1f + hitFlash * kick);
            transform.localScale = new Vector3(punch, punch, punch);
        }

        void ApproachStructure(BuildPad structure, float delta)
        {
            FortIdleConfig config = FortWorld.Config;
            Bounds bounds = structure.WorldBounds;
            float inset = bodyRadius;
            float span = slotCount <= 1 ? 0.5f : (slot + 0.5f) / slotCount;
            float slotX = Mathf.Lerp(bounds.min.x + inset, bounds.max.x - inset, span);
            int rank = WallRank(structure);
            int cap = config.monsters.maxWallAttackers;
            bool front = cap <= 0 || rank < cap;
            float standZ = bounds.max.z + config.monsters.contactGap;
            if (!front)
                standZ += config.monsters.queueSpacing * (rank - cap + 1);
            Vector3 goal = new Vector3(slotX, transform.position.y, standZ);
            float gap = transform.position.z - bounds.max.z;
            bool inFront = transform.position.x >= bounds.min.x - bodyRadius
                && transform.position.x <= bounds.max.x + bodyRadius;
            if (front && inFront && gap <= config.monsters.attackRange)
            {
                Face(new Vector3(0f, 0f, -1f));
                TryAttack(delta, structure);
                Vector3 held = transform.position;
                if (held.z < standZ)
                    held.z = standZ;
                transform.position = held;
                return;
            }

            MoveToward(goal, delta);
        }

        int WallRank(BuildPad structure)
        {
            float z = transform.position.z;
            int rank = 0;
            for (int i = 0; i < FortWorld.Monsters.Count; i++)
            {
                Monster other = FortWorld.Monsters[i];
                if (other == this || !other.Alive)
                    continue;
                if (structure == null || other.transform.position.z < structure.WorldBounds.max.z)
                    continue;
                if (other.transform.position.z < z || (other.transform.position.z == z && other.slot < slot))
                    rank += 1;
            }

            return rank;
        }

        BuildPad FlankTarget()
        {
            float reach = FortWorld.Config.defenses.flankAttackRange;
            BuildPad best = null;
            float bestDistance = reach;
            for (int i = 0; i < FortWorld.Pads.Count; i++)
            {
                BuildPad pad = FortWorld.Pads[i];
                if (!pad.IsAttackTarget)
                    continue;
                float distance = FortMath.HorizontalDistance(transform.position, pad.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = pad;
                }
            }

            return best;
        }

        void ApproachFire(float delta)
        {
            Vector3 fire = FortWorld.Fire != null ? FortWorld.Fire.transform.position : Vector3.zero;
            fire.y = transform.position.y;
            float distance = FortMath.HorizontalDistance(transform.position, fire);
            if (distance <= FortWorld.Config.monsters.attackRange + bodyRadius)
            {
                Vector3 face = fire - transform.position;
                face.y = 0f;
                Face(face);
                TryAttack(delta, null);
                return;
            }

            MoveToward(fire, delta);
        }

        void MoveToward(Vector3 goal, float delta)
        {
            Vector3 offset = goal - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance <= 0f)
                return;
            float scale = Time.time < slowUntil ? slowFactor : 1f;
            float step = moveSpeed * scale * delta;
            if (step > distance)
                step = distance;
            Vector3 direction = offset / distance;
            transform.position += direction * step;
            Face(direction);
        }

        void Face(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0f)
                return;
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        void TryAttack(float delta, BuildPad structure)
        {
            attackTimer -= delta;
            if (attackTimer > 0f)
                return;
            FortIdleConfig config = FortWorld.Config;
            attackTimer = config.monsters.attackInterval;
            if (structure != null)
            {
                structure.ApplyDamage(attackDamage);
                if (structure.PadKind == BuildPad.Kind.Wall && structure.Tier >= 2)
                    Stagger(config.defenses.staggerSeconds, config.defenses.staggerSpeedScale);
                if (structure.PadKind == BuildPad.Kind.Gate && structure.Tier >= 2)
                    TakeDamage(config.defenses.gateThornsPerSecond * config.monsters.attackInterval);
            }
            else if (FortWorld.Fire != null)
                FortWorld.Fire.ApplyDamage(attackDamage);
        }

        void Separate(float delta)
        {
            float strength = FortWorld.Config.monsters.separationStrength;
            if (separationRadius <= 0f || strength <= 0f)
                return;

            float push = 0f;
            for (int i = 0; i < FortWorld.Monsters.Count; i++)
            {
                Monster other = FortWorld.Monsters[i];
                if (other == this || !other.Alive)
                    continue;
                float reach = Mathf.Max(separationRadius, other.separationRadius);
                float dx = transform.position.x - other.transform.position.x;
                float abs = Mathf.Abs(dx);
                if (abs >= reach || abs <= 0f)
                    continue;
                push += Mathf.Sign(dx) * (reach - abs);
            }

            if (push == 0f)
                return;
            Vector3 position = transform.position;
            position.x += Mathf.Clamp(push, -1f, 1f) * strength * delta;
            transform.position = position;
        }
    }
}
