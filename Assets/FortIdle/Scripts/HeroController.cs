using System;
using UnityEngine;

namespace FortIdle
{
    public class HeroController : MonoBehaviour
    {
        CharacterController controller;
        Transform visual;
        Transform axe;
        PlankStack stack;
        VirtualJoystick joystick;
        readonly System.Collections.Generic.List<Transform> stones = new System.Collections.Generic.List<Transform>();
        float currentSpeed;
        float verticalSpeed;
        float yaw;
        float bobPhase;
        float chopTimer;
        float swingLeft;
        float pourHold;
        float pourAccumulator;
        float swingTimer;
        float downTimer;
        float grace;
        Vector3 planarVelocity;
        FortAnim anim;

        public float HitPoints { get; private set; }
        public float MaxHitPoints { get; private set; }
        public bool Down { get; private set; }
        public float HealthFraction => MaxHitPoints <= 0f ? 0f : HitPoints / MaxHitPoints;

        public void Init(CharacterController body, Transform visualRig, Transform axeTransform, PlankStack plankStack, VirtualJoystick stick)
        {
            controller = body;
            visual = visualRig;
            axe = axeTransform;
            stack = plankStack;
            joystick = stick;
            yaw = FortWorld.Config.player.spawnYawDegrees;
            visual.localRotation = Quaternion.Euler(0f, yaw, 0f);
            RecalcVitals();
            HitPoints = MaxHitPoints;
            anim = visual != null ? visual.GetComponentInChildren<FortAnim>() : null;
            HpBar bar = HpBar.Create(transform, FortWorld.Config.player.height + 0.2f, 0.9f);
            bar.Read = () => HealthFraction;
        }

        void Update()
        {
            float delta = Time.deltaTime;
            if (Down)
            {
                TickDown(delta);
                return;
            }

            Vector2 move = ReadMove();
            Move(move, delta);
            Gather(delta);
            Pour(delta);
            TickMelee(delta);
            TickContact(delta);
            ShowStone();
            Animate(delta);
            stack.Tick(planarVelocity, delta);
        }

        Vector2 ReadMove()
        {
            Vector2 stick = joystick != null ? joystick.Value : Vector2.zero;
            Vector2 keys = ReadLegacyKeys() + ReadNewInputKeys();
            if (keys.sqrMagnitude > 1f)
                keys.Normalize();
            Vector2 combined = stick + keys;
            if (combined.sqrMagnitude > 1f)
                combined.Normalize();
            return combined;
        }

        static Vector2 ReadLegacyKeys()
        {
            float x = 0f;
            float y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                y += 1f;
            return new Vector2(x, y);
        }

        static Vector2 ReadNewInputKeys()
        {
            Type keyboardType = Type.GetType("UnityEngine.InputSystem.Keyboard, Unity.InputSystem");
            if (keyboardType == null)
                return Vector2.zero;
            object keyboard = keyboardType.GetProperty("current")?.GetValue(null);
            if (keyboard == null)
                return Vector2.zero;

            float x = 0f;
            float y = 0f;
            if (Pressed(keyboard, "aKey") || Pressed(keyboard, "leftArrowKey"))
                x -= 1f;
            if (Pressed(keyboard, "dKey") || Pressed(keyboard, "rightArrowKey"))
                x += 1f;
            if (Pressed(keyboard, "sKey") || Pressed(keyboard, "downArrowKey"))
                y -= 1f;
            if (Pressed(keyboard, "wKey") || Pressed(keyboard, "upArrowKey"))
                y += 1f;
            return new Vector2(x, y);
        }

        static bool Pressed(object keyboard, string propertyName)
        {
            object key = keyboard.GetType().GetProperty(propertyName)?.GetValue(keyboard);
            if (key == null)
                return false;
            object pressed = key.GetType().GetProperty("isPressed")?.GetValue(key);
            return pressed is bool value && value;
        }

        void Move(Vector2 move, float delta)
        {
            FortIdleConfig.PlayerTuning player = FortWorld.Config.player;
            float fullSpeed = FortWorld.Progress != null ? FortWorld.Progress.MoveSpeed : player.moveSpeed;
            float accelTime = player.moveAccelerationTime <= 0f ? delta : player.moveAccelerationTime;
            float accel = fullSpeed / accelTime;
            float targetSpeed = fullSpeed * move.magnitude;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accel * delta);

            Vector3 wish = new Vector3(move.x, 0f, move.y);
            if (wish.sqrMagnitude > 1f)
                wish.Normalize();
            planarVelocity = wish.sqrMagnitude > 0f ? wish.normalized * currentSpeed : Vector3.zero;

            if (controller.isGrounded && verticalSpeed < 0f)
                verticalSpeed = -player.groundStickSpeed;
            verticalSpeed -= player.gravity * delta;

            Vector3 displacement = planarVelocity * delta;
            displacement.y = verticalSpeed * delta;
            controller.Move(displacement);

            ClampToGround();

            Vector3 face = planarVelocity;
            TreeNode tree = NearestTree();
            if (tree != null && stack.HasRoom)
            {
                face = tree.transform.position - transform.position;
                face.y = 0f;
            }

            if (face.sqrMagnitude > 0f)
            {
                float targetYaw = Mathf.Atan2(face.x, face.z) * Mathf.Rad2Deg;
                float blend = 1f - Mathf.Exp(-player.turnSharpness * delta);
                yaw = Mathf.LerpAngle(yaw, targetYaw, blend);
            }
        }

        void ClampToGround()
        {
            FortIdleConfig config = FortWorld.Config;
            float limit = config.layout.groundExtent * 0.5f - config.player.boundaryMargin;
            Vector3 position = transform.position;
            Vector3 clamped = position;
            clamped.x = Mathf.Clamp(clamped.x, -limit, limit);
            clamped.z = Mathf.Clamp(clamped.z, -limit, limit);
            if (clamped == position)
                return;
            controller.enabled = false;
            transform.position = clamped;
            controller.enabled = true;
        }

        void Gather(float delta)
        {
            FortIdleConfig config = FortWorld.Config;
            RockNode rock = NearestRock();
            bool mine = rock != null && FortWorld.Progress != null && FortWorld.Progress.StoneHasRoom;
            TreeNode tree = NearestTree();
            if (mine && tree != null && stack.HasRoom)
            {
                float rockDistance = FortMath.HorizontalDistance(transform.position, rock.transform.position);
                float treeDistance = FortMath.HorizontalDistance(transform.position, tree.transform.position);
                mine = rockDistance <= treeDistance;
            }

            if (mine)
            {
                Mine(rock, delta);
                return;
            }

            Chop(tree, delta);
        }

        void Mine(RockNode rock, float delta)
        {
            FortIdleConfig config = FortWorld.Config;
            if (rock == null || FortWorld.Progress == null || !FortWorld.Progress.StoneHasRoom)
                return;
            chopTimer += delta;
            if (chopTimer < config.chop.interval)
                return;
            chopTimer = 0f;
            swingLeft = config.chop.swingDuration;
            int gain = FortWorld.Progress.AddStone(config.supplies.stonePerHit);
            rock.ApplyHit();
            Vector3 from = rock.transform.position + Vector3.up * config.supplies.rockHeight;
            for (int i = 0; i < gain; i++)
            {
                Flyer.Spawn(
                    from,
                    transform.position + Vector3.up * config.player.headHeight,
                    config.chop.flyDuration,
                    config.chop.flyArcHeight,
                    FortWorld.Look.Stone,
                    config.supplies.stoneChunkSize);
            }
        }

        void Chop(TreeNode tree, float delta)
        {
            FortIdleConfig config = FortWorld.Config;
            if (tree == null || !stack.HasRoom)
                return;

            chopTimer += delta;
            if (chopTimer < config.chop.interval)
                return;

            chopTimer = 0f;
            swingLeft = config.chop.swingDuration;
            int gain = stack.Add(config.chop.planksPerHit);
            if (gain > 0 && FortWorld.Progress != null)
                FortWorld.Progress.NoteWood(gain, true);
            tree.ApplyHit();
            Vector3 from = tree.transform.position + Vector3.up * config.layout.canopyHeight;
            for (int i = 0; i < gain; i++)
            {
                float offset = (i - (gain - 1) * 0.5f) * config.stack.plankSize.x;
                Flyer.Spawn(
                    from + new Vector3(offset, 0f, 0f),
                    stack.TopWorld(),
                    config.chop.flyDuration,
                    config.chop.flyArcHeight,
                    FortWorld.Look.Plank,
                    config.stack.plankSize);
            }
        }

        void Pour(float delta)
        {
            FortIdleConfig config = FortWorld.Config;
            BuildPad pad = NearestSupplyPad();
            if (pad == null)
            {
                pourHold = 0f;
                pourAccumulator = 0f;
                return;
            }

            pourHold += delta;
            float rate = pourHold >= config.pour.boostDelay
                ? config.pour.boostedPlanksPerSecond
                : config.pour.planksPerSecond;
            pourAccumulator += rate * delta;
            Vector3 landing = pad.DeliveryPoint();
            while (pourAccumulator >= 1f)
            {
                if (pad.WantsWood && stack.Count > 0)
                {
                    pourAccumulator -= 1f;
                    Vector3 from = stack.RemoveOne();
                    pad.AddWood();
                    Flyer.Spawn(from, landing, config.pour.flyDuration, config.pour.flyArcHeight, FortWorld.Look.Plank, config.stack.plankSize);
                    continue;
                }

                if (pad.WantsStone && FortWorld.Progress != null && FortWorld.Progress.CarriedStone > 0)
                {
                    pourAccumulator -= 1f;
                    FortWorld.Progress.RemoveStone();
                    pad.AddStone();
                    Flyer.Spawn(transform.position + Vector3.up, landing, config.pour.flyDuration, config.pour.flyArcHeight, FortWorld.Look.Stone, config.supplies.stoneChunkSize);
                    continue;
                }

                if (pad.WantsIron && FortWorld.Progress != null && FortWorld.Progress.SpendIron(1))
                {
                    pourAccumulator -= 1f;
                    pad.AddIron();
                    Flyer.Spawn(transform.position + Vector3.up, landing, config.pour.flyDuration, config.pour.flyArcHeight, FortWorld.Look.Iron, config.supplies.stoneChunkSize);
                    continue;
                }

                break;
            }
        }

        void ShowStone()
        {
            int count = FortWorld.Progress != null ? FortWorld.Progress.CarriedStone : 0;
            int visible = count < 6 ? count : 6;
            while (stones.Count < visible)
            {
                Vector3 chunkSize = FortWorld.Config.supplies.stoneChunkSize;
                GameObject chunk = FortArt.Box(visual, "RockMoss", chunkSize, Color.white, 0.05f, false, false);
                if (chunk == null)
                {
                    chunk = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Stone, visual, false);
                    chunk.transform.localScale = chunkSize;
                }

                chunk.name = "Stone";
                stones.Add(chunk.transform);
            }

            for (int i = 0; i < stones.Count; i++)
            {
                bool on = i < visible;
                stones[i].gameObject.SetActive(on);
                if (on)
                    stones[i].localPosition = new Vector3(0.45f, 0.7f + i * 0.22f, 0.15f);
            }
        }

        void Animate(float delta)
        {
            FortIdleConfig config = FortWorld.Config;
            if (anim != null && anim.Ready)
            {
                string clip = "Idle";
                if (swingLeft > 0f)
                    clip = "Punch";
                else if (pourHold > 0.05f)
                    clip = "PickUp";
                else if (stack != null && stack.Count > 0 && currentSpeed > 0.2f)
                    clip = "Walk_Carry";
                else if (currentSpeed > 0.2f)
                    clip = "Walk";
                anim.Play(clip);
                visual.localRotation = Quaternion.Euler(0f, yaw, 0f);
                return;
            }

            float reference = FortWorld.Progress != null ? FortWorld.Progress.MoveSpeed : config.player.moveSpeed;
            float speedFraction = reference <= 0f ? 0f : currentSpeed / reference;
            bobPhase += delta * config.player.visualBobFrequency * Mathf.PI * 2f;
            float bob = Mathf.Sin(bobPhase) * config.player.visualBobAmplitude * speedFraction;
            Vector3 visualPosition = visual.localPosition;
            visualPosition.y = bob;
            visual.localPosition = visualPosition;
            visual.localRotation = Quaternion.Euler(0f, yaw, 0f);

            if (swingLeft > 0f)
                swingLeft -= delta;
            float span = config.chop.swingDuration;
            float u = span <= 0f ? 1f : 1f - Mathf.Clamp01(swingLeft / span);
            float angle = Mathf.Lerp(config.chop.swingDegrees, 0f, u);
            if (axe != null)
                axe.localRotation = Quaternion.Euler(-angle, 0f, 0f);
        }

        TreeNode NearestTree()
        {
            float best = FortWorld.Config.chop.range;
            TreeNode found = null;
            for (int i = 0; i < FortWorld.Trees.Count; i++)
            {
                TreeNode tree = FortWorld.Trees[i];
                if (!tree.Standing)
                    continue;
                float distance = FortMath.HorizontalDistance(transform.position, tree.transform.position);
                if (distance <= best)
                {
                    best = distance;
                    found = tree;
                }
            }

            return found;
        }

        BuildPad NearestSupplyPad()
        {
            float radius = FortWorld.Config.pour.padRadius;
            float best = radius;
            BuildPad found = null;
            bool holdStone = FortWorld.Progress != null && FortWorld.Progress.CarriedStone > 0;
            bool holdIron = FortWorld.Progress != null && FortWorld.Progress.Iron > 0;
            for (int i = 0; i < FortWorld.Pads.Count; i++)
            {
                BuildPad pad = FortWorld.Pads[i];
                bool useful = (pad.WantsWood && stack.Count > 0)
                    || (pad.WantsStone && holdStone)
                    || (pad.WantsIron && holdIron);
                if (!useful)
                    continue;
                float distance = FortMath.HorizontalDistance(transform.position, pad.transform.position);
                if (distance <= best)
                {
                    best = distance;
                    found = pad;
                }
            }

            return found;
        }

        RockNode NearestRock()
        {
            float best = FortWorld.Config.supplies.mineRange;
            RockNode found = null;
            for (int i = 0; i < FortWorld.Rocks.Count; i++)
            {
                RockNode rock = FortWorld.Rocks[i];
                if (!rock.Available)
                    continue;
                float distance = FortMath.HorizontalDistance(transform.position, rock.transform.position);
                if (distance <= best)
                {
                    best = distance;
                    found = rock;
                }
            }

            return found;
        }

        public void RecalcVitals()
        {
            FortIdleConfig config = FortWorld.Config;
            FortProgress progress = FortWorld.Progress;
            float next = config.heroCombat.hitPoints;
            if (progress != null)
                next += progress.ArmorLevel * config.heroCombat.armorPerLevel + progress.PatchLevel * config.heroCombat.patchHitPoints;
            float gain = next - MaxHitPoints;
            MaxHitPoints = next;
            if (gain > 0f)
                HitPoints += gain;
            if (HitPoints > MaxHitPoints)
                HitPoints = MaxHitPoints;
            if (HitPoints <= 0f)
                HitPoints = MaxHitPoints;
        }

        public void Heal(float amount)
        {
            if (Down || amount <= 0f)
                return;
            HitPoints += amount;
            if (HitPoints > MaxHitPoints)
                HitPoints = MaxHitPoints;
        }

        void TickMelee(float delta)
        {
            FortIdleConfig config = FortWorld.Config;
            FortProgress progress = FortWorld.Progress;
            if (config == null || progress == null)
                return;
            swingTimer += delta;
            if (swingTimer < config.heroCombat.swingInterval)
                return;
            swingTimer = 0f;
            float damage = progress.SwingDamage;
            float radius = config.heroCombat.cleaveRadius;
            bool hit = false;
            for (int i = 0; i < FortWorld.Monsters.Count; i++)
            {
                Monster monster = FortWorld.Monsters[i];
                if (!monster.Alive)
                    continue;
                if (FortMath.HorizontalDistance(transform.position, monster.transform.position) > radius)
                    continue;
                monster.TakeDamage(damage);
                hit = true;
            }

            for (int i = 0; i < FortWorld.Boars.Count; i++)
            {
                Boar boar = FortWorld.Boars[i];
                if (!boar.Alive)
                    continue;
                if (FortMath.HorizontalDistance(transform.position, boar.transform.position) > radius)
                    continue;
                boar.TakeDamage(damage);
                hit = true;
            }

            if (hit)
                swingLeft = config.chop.swingDuration;
        }

        void TickContact(float delta)
        {
            if (grace > 0f)
            {
                grace -= delta;
                return;
            }

            FortIdleConfig.HeroCombatTuning combat = FortWorld.Config.heroCombat;
            float dps = 0f;
            for (int i = 0; i < FortWorld.Monsters.Count; i++)
            {
                Monster monster = FortWorld.Monsters[i];
                if (!monster.Alive)
                    continue;
                if (FortMath.HorizontalDistance(transform.position, monster.transform.position) > combat.cleaveRadius)
                    continue;
                if (monster.Kind == MonsterKind.Brute)
                    dps += combat.bruteContact;
                else if (monster.Kind == MonsterKind.Treant)
                    dps += combat.treantContact;
            }

            if (dps <= 0f)
                return;
            HitPoints -= dps * delta;
            if (HitPoints <= 0f)
                Downed();
        }

        void Downed()
        {
            Down = true;
            HitPoints = 0f;
            downTimer = FortWorld.Config.heroCombat.respawnSeconds;
            Debug.Log("FORTIDLE hero=down");
        }

        void TickDown(float delta)
        {
            if (anim != null)
                anim.Play("Defeat");
            downTimer -= delta;
            if (downTimer > 0f)
                return;
            Down = false;
            Vector3 spot = FortWorld.Fire != null ? FortWorld.Fire.transform.position : Vector3.zero;
            spot += new Vector3(0f, 0.2f, -1.5f);
            if (controller != null)
                controller.enabled = false;
            transform.position = spot;
            if (controller != null)
                controller.enabled = true;
            HitPoints = MaxHitPoints;
            grace = FortWorld.Config.heroCombat.respawnGrace;
            Debug.Log("FORTIDLE hero=respawn");
        }
    }
}
