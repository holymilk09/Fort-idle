using System;
using UnityEngine;

namespace FortIdle
{
    /// <summary>
    /// Single tuning asset for the Map 1 prototype.
    /// SPEC fields are estimates copied from MAP1_SPEC / the GDD.
    /// LAYOUT / FEEL / ESTIMATE fields are prototype stand-ins so the
    /// scene can run. Replace them with measured values; do not add
    /// gameplay numbers to scripts.
    /// </summary>
    [CreateAssetMenu(fileName = "FortIdleConfig", menuName = "Fort Idle/Config")]
    public class FortIdleConfig : ScriptableObject
    {
        public PlayerTuning player = new PlayerTuning();
        public StackTuning stack = new StackTuning();
        public ChopTuning chop = new ChopTuning();
        public PourTuning pour = new PourTuning();
        public StructureTuning structures = new StructureTuning();
        public StageTuning stages = new StageTuning();
        public TurretTuning turret = new TurretTuning();
        public MonsterTuning monsters = new MonsterTuning();
        public WaveTuning wave = new WaveTuning();
        public FireTuning fire = new FireTuning();
        public LayoutTuning layout = new LayoutTuning();
        public CameraTuning cameraRig = new CameraTuning();
        public JoystickTuning joystick = new JoystickTuning();
        public HudTuning hud = new HudTuning();
        public PresentationTuning presentation = new PresentationTuning();
        public CopyTuning copy = new CopyTuning();
        public LootTuning loot = new LootTuning();
        public UpgradeTuning upgrades = new UpgradeTuning();
        public WorkerTuning workers = new WorkerTuning();
        public OfflineTuning offline = new OfflineTuning();
        public DemoTuning demo = new DemoTuning();
        public DefenseTuning defenses = new DefenseTuning();
        public SupplyTuning supplies = new SupplyTuning();
        public HeroCombatTuning heroCombat = new HeroCombatTuning();
        public DropTuning drops = new DropTuning();
        public ProgressionTuning progression = new ProgressionTuning();
        public EliteTuning elites = new EliteTuning();
        public QualityTuning quality = new QualityTuning();
        public EndlessTuning endless = new EndlessTuning();
        public PrestigeTuning prestige = new PrestigeTuning();

        [Serializable]
        public class PlayerTuning
        {
            [Tooltip("SPEC. Base move speed, meters per second.")]
            public float moveSpeed = 5f;

            [Tooltip("SPEC. Seconds to reach full speed. No accel lag beyond this.")]
            public float moveAccelerationTime = 0.1f;

            [Tooltip("FEEL. Yaw response. Higher snaps faster.")]
            public float turnSharpness = 14f;

            [Tooltip("LAYOUT. CharacterController radius, meters.")]
            public float radius = 0.4f;

            [Tooltip("LAYOUT. CharacterController height, meters.")]
            public float height = 1.8f;

            [Tooltip("FEEL. Downward acceleration.")]
            public float gravity = 30f;

            [Tooltip("FEEL. Downward speed applied while grounded so the hero stays on the floor.")]
            public float groundStickSpeed = 2f;

            [Tooltip("LAYOUT.")]
            public float skinWidth = 0.05f;

[Tooltip("LAYOUT.")]
public float stepOffset = 0.25f;

[Tooltip("LAYOUT. CharacterController slope limit, degrees.")]
public float slopeLimit = 50f;

[Tooltip("LAYOUT.")]
public float minMoveDistance = 0.001f;

            [Tooltip("FEEL. Vertical bob amplitude at full speed, meters.")]
            public float visualBobAmplitude = 0.045f;

            [Tooltip("FEEL. Bob cycles per second at full speed.")]
            public float visualBobFrequency = 2.2f;

            [Tooltip("LAYOUT. Body capsule radius.")]
            public float bodyRadius = 0.36f;

            [Tooltip("LAYOUT. Body capsule height.")]
            public float bodyHeight = 1.05f;

            [Tooltip("LAYOUT. Head sphere diameter.")]
            public float headDiameter = 0.42f;

            [Tooltip("LAYOUT. Head center height above the feet.")]
            public float headHeight = 1.48f;

            [Tooltip("LAYOUT. World spawn. Y is a small lift so the controller settles onto y=0.")]
            public Vector3 spawnPosition = new Vector3(0f, 0.15f, 2.2f);

            [Tooltip("LAYOUT. Spawn facing. 0 looks toward +Z (the trees).")]
            public float spawnYawDegrees = 0f;

            [Tooltip("LAYOUT. Axe local position on the visual rig.")]
            public Vector3 axeLocalPosition = new Vector3(0.42f, 0.95f, 0.42f);

            [Tooltip("LAYOUT. Axe mesh size.")]
            public Vector3 axeSize = new Vector3(0.1f, 0.48f, 0.1f);

            [Tooltip("LAYOUT. Keep the hero inside the ground slab.")]
            public float boundaryMargin = 1.25f;
        }

        [Serializable]
        public class StackTuning
        {
            [Tooltip("SPEC. Starting stack capacity. The bought ladder is UpgradeTuning.")]
            public int capacity = 20;

            [Tooltip("SPEC. Vertical step per carried plank, meters.")]
            public float visualHeightStep = 0.12f;

            [Tooltip("SPEC. Meshes drawn. Count above this still tracks, with extraPlankScale.")]
            public int maxRenderedPlanks = 60;

            [Tooltip("SPEC. Uniform scale added per plank past maxRenderedPlanks.")]
            public float extraPlankScale = 0.004f;

            [Tooltip("LAYOUT. Plank mesh size, meters.")]
            public Vector3 plankSize = new Vector3(0.48f, 0.07f, 0.26f);

            [Tooltip("LAYOUT. Stack anchor on the hero visual. -Z is the back when facing +Z.")]
            public Vector3 localOffset = new Vector3(0f, 1.02f, -0.34f);

            [Tooltip("SPEC. Max visual sway, degrees, at full speed.")]
            public float swayDegrees = 4f;

            [Tooltip("FEEL. SmoothDamp time for sway, seconds.")]
            public float swaySmoothTime = 0.12f;
        }

        [Serializable]
        public class ChopTuning
        {
            [Tooltip("SPEC. Seconds between hits while in range.")]
            public float interval = 0.4f;

            [Tooltip("SPEC. Planks added per hit, capped by remaining stack space.")]
            public int planksPerHit = 3;

            [Tooltip("ESTIMATE. Spec has stump respawn but not hits per tree. 4 hits * 3 planks = 12 planks per tree.")]
            public int hitsToFellTree = 4;

            [Tooltip("LAYOUT. Horizontal chop reach, meters.")]
            public float range = 1.8f;

            [Tooltip("SPEC. Seconds before a stump stands again.")]
            public float stumpRespawnSeconds = 8f;

            [Tooltip("FEEL. Plank fly-in time, seconds. Storyboard arc is 0.15s.")]
            public float flyDuration = 0.15f;

            [Tooltip("FEEL. Peak height of the fly-in arc, meters.")]
            public float flyArcHeight = 1.15f;

            [Tooltip("FEEL. Axe swing, degrees.")]
            public float swingDegrees = 62f;

            [Tooltip("FEEL. Axe swing return time, seconds.")]
            public float swingDuration = 0.16f;
        }

        [Serializable]
        public class PourTuning
        {
            [Tooltip("SPEC. Planks per second when first standing on a pad.")]
            public float planksPerSecond = 12f;

            [Tooltip("SPEC. Planks per second after boostDelay on the same pad.")]
            public float boostedPlanksPerSecond = 25f;

            [Tooltip("SPEC. Seconds on the pad before the boosted rate.")]
            public float boostDelay = 1f;

            [Tooltip("LAYOUT. Horizontal radius that counts as standing on the pad.")]
            public float padRadius = 1.45f;

            [Tooltip("LAYOUT. Disc thickness.")]
            public float padDiscHeight = 0.08f;

            [Tooltip("LAYOUT. Disc diameter.")]
            public float padDiscDiameter = 2.7f;

            [Tooltip("FEEL. Pour flyer duration.")]
            public float flyDuration = 0.12f;

            [Tooltip("FEEL. Pour flyer arc height.")]
            public float flyArcHeight = 0.85f;

            [Tooltip("LAYOUT. World label height above the pad.")]
            public float labelHeight = 2.15f;

            [Tooltip("FEEL. Pad labels closer than this stay solid.")]
            public float labelFadeNear = 7.5f;

            [Tooltip("FEEL. Pad labels farther than this disappear.")]
            public float labelFadeFar = 14f;
        }

        [Serializable]
        public class StructureTuning
        {
            [Tooltip("SPEC. Wall plank cost.")]
            public int wallCost = 40;

            [Tooltip("SPEC. Wall hit points.")]
            public float wallHitPoints = 300f;

            [Tooltip("SPEC. Turret plank cost.")]
            public int turretCost = 80;

            [Tooltip("SPEC. Worker hut plank cost. Completing it unlocks hiring.")]
            public int hutCost = 150;

            [Tooltip("LAYOUT. Hut body size.")]
            public Vector3 hutBodySize = new Vector3(2.2f, 1.5f, 2f);

            [Tooltip("LAYOUT. Roof size. Sits on the body.")]
            public Vector3 hutRoofSize = new Vector3(2.6f, 0.38f, 2.4f);

            [Tooltip("LAYOUT. Door size on the +Z face.")]
            public Vector3 hutDoorSize = new Vector3(0.7f, 0.95f, 0.16f);

            [Tooltip("LAYOUT. Wall mesh size. Sits on y=0, centered on the wall pad.")]
            public Vector3 wallSize = new Vector3(7.2f, 2.5f, 0.5f);

            [Tooltip("LAYOUT. Turret pedestal height.")]
            public float turretBaseHeight = 1.2f;

            [Tooltip("LAYOUT. Turret pedestal radius.")]
            public float turretBaseRadius = 0.55f;

            [Tooltip("LAYOUT. Barrel size. Local +Z is the muzzle.")]
            public Vector3 turretBarrelSize = new Vector3(0.22f, 0.22f, 1.15f);

            [Tooltip("LAYOUT. Barrel center relative to the turret pivot.")]
            public Vector3 turretBarrelLocalPosition = new Vector3(0f, 1.15f, 0.35f);

            [Tooltip("LAYOUT. Small cap on top of the pedestal.")]
            public float turretCapDiameter = 0.7f;

            [Tooltip("LAYOUT. Cap center height.")]
            public float turretCapHeight = 1.28f;

            [Tooltip("FEEL. Seconds for the rise, before the hit-stop.")]
            public float buildRiseDuration = 0.55f;

            [Tooltip("FEEL. EaseOutBack overshoot. 1.7 is the classic curve.")]
            public float buildOvershoot = 1.7f;
        }

        [Serializable]
        public class StageTuning
        {
            [Tooltip("FEEL. Wall stages. Threshold is wood cost divided by this. Posts, rails, planks, upper planks, caps.")]
            public int wall = 5;

            [Tooltip("FEEL. Gate stages. Posts, crossbeam, door, hinges, swing shut.")]
            public int gate = 5;

            [Tooltip("FEEL. Watchtower stages. Legs, braces, platform, railing, roof, archer.")]
            public int tower = 6;

            [Tooltip("FEEL. Turret stages. Base, pivot, frame, bolt arm, test spin.")]
            public int turret = 5;

            [Tooltip("FEEL. Spike stages. Pit, stakes, sharpened tips.")]
            public int spike = 3;

            [Tooltip("FEEL. Campfire stages. Stone ring, logs, ignition.")]
            public int fire = 3;

            [Tooltip("FEEL. Hut stages. Posts, walls, roof, door.")]
            public int hut = 4;

            [Tooltip("FEEL. Stages for a level-2 or level-3 upgrade poured onto a wall.")]
            public int wallUpgrade = 4;

            [Tooltip("FEEL. Stages for a gate upgrade.")]
            public int gateUpgrade = 4;

            [Tooltip("FEEL. Stages for a watchtower upgrade.")]
            public int towerUpgrade = 4;

            [Tooltip("FEEL. Stages for the turret upgrade pad.")]
            public int turretUpgrade = 4;

            [Tooltip("FEEL. Stages for a campfire upgrade.")]
            public int fireUpgrade = 4;

            [Tooltip("FEEL. Seconds for a part to drop and squash into place.")]
            public float snapSeconds = 0.2f;

            [Tooltip("FEEL. How far a part falls, meters. Posts rise by this much instead.")]
            public float dropHeight = 1.25f;

            [Tooltip("FEEL. Peak squash scale on impact.")]
            public float squash = 1.22f;

            [Tooltip("FEEL. Wood chips in one puff.")]
            public int puffChips = 6;

            [Tooltip("FEEL. Cap on live chips so four workers cannot flood the scene.")]
            public int puffCap = 28;

            [Tooltip("FEEL. Camera nudge toward the part on the final stage, meters.")]
            public float nudgeMeters = 0.4f;

            [Tooltip("FEEL. Nudge length, seconds.")]
            public float nudgeSeconds = 0.22f;

            [Tooltip("FEEL. Scale punch when the structure is finished.")]
            public float builtPunch = 1.12f;

            [Tooltip("FEEL. Built punch length, seconds.")]
            public float builtSeconds = 0.28f;

            [Tooltip("FEEL. Seconds between supplies in the -stages close-up.")]
            public float showcaseStep = 0.14f;

            public void Ensure()
            {
                if (wall < 1)
                    wall = 5;
                if (gate < 1)
                    gate = 5;
                if (tower < 1)
                    tower = 6;
                if (turret < 1)
                    turret = 5;
                if (spike < 1)
                    spike = 3;
                if (fire < 1)
                    fire = 3;
                if (hut < 1)
                    hut = 4;
                if (wallUpgrade < 1)
                    wallUpgrade = 4;
                if (gateUpgrade < 1)
                    gateUpgrade = 4;
                if (towerUpgrade < 1)
                    towerUpgrade = 4;
                if (turretUpgrade < 1)
                    turretUpgrade = 4;
                if (fireUpgrade < 1)
                    fireUpgrade = 4;
                if (snapSeconds <= 0f)
                    snapSeconds = 0.2f;
                if (dropHeight <= 0f)
                    dropHeight = 1.25f;
                if (squash <= 1f)
                    squash = 1.22f;
                if (puffChips < 1)
                    puffChips = 6;
                if (puffCap < 1)
                    puffCap = 28;
                if (nudgeMeters <= 0f)
                    nudgeMeters = 0.4f;
                if (nudgeSeconds <= 0f)
                    nudgeSeconds = 0.22f;
                if (builtPunch <= 1f)
                    builtPunch = 1.12f;
                if (builtSeconds <= 0f)
                    builtSeconds = 0.28f;
                if (showcaseStep <= 0f)
                    showcaseStep = 0.14f;
            }
        }

        [Serializable]
        public class TurretTuning
        {
            [Tooltip("SPEC. Damage per shot.")]
            public float damage = 25f;

            [Tooltip("SPEC. Shots per second. 25 * 2 = 50 DPS.")]
            public float shotsPerSecond = 2f;

            [Tooltip("SPEC. Targeting range, meters.")]
            public float range = 12f;

            [Tooltip("ESTIMATE. Bolt speed, meters per second. Not in the spec.")]
            public float projectileSpeed = 24f;

            [Tooltip("LAYOUT. Bolt mesh diameter.")]
            public float projectileRadius = 0.14f;

            [Tooltip("ESTIMATE. Bolt lifetime, seconds.")]
            public float projectileLifetime = 1.25f;

            [Tooltip("ESTIMATE. Hit radius against a monster center.")]
            public float projectileHitRadius = 0.55f;

            [Tooltip("FEEL. Barrel yaw response.")]
            public float yawSharpness = 12f;

            [Tooltip("LAYOUT. Muzzle distance along the barrel forward axis.")]
            public float muzzleForward = 0.62f;
        }

        [Serializable]
        public class MonsterTuning
        {
            [Tooltip("SPEC. Hit points.")]
            public float hitPoints = 60f;

            [Tooltip("SPEC. Move speed, meters per second.")]
            public float moveSpeed = 1.5f;

            [Tooltip("ESTIMATE. Damage per attack. Not in the spec. 3 per second lets the spec 300 HP wall survive night 1 when the spec turret is up, and lets the 1000 HP fire fall if it is not.")]
            public float attackDamage = 3f;

            [Tooltip("ESTIMATE. Seconds between attacks.")]
            public float attackInterval = 1f;

            [Tooltip("LAYOUT. Distance at which a monster attacks the wall face or the fire.")]
            public float attackRange = 1.2f;

            [Tooltip("LAYOUT. How far in front of the wall face a monster stops.")]
            public float contactGap = 0.45f;

            [Tooltip("LAYOUT. Body radius.")]
            public float radius = 0.4f;

            [Tooltip("LAYOUT. Body height.")]
            public float height = 1.45f;

            [Tooltip("LAYOUT. Eye diameter.")]
            public float eyeDiameter = 0.16f;

            [Tooltip("LAYOUT. Eye height above the feet.")]
            public float eyeHeight = 1.12f;

            [Tooltip("LAYOUT. Eye local Z (forward).")]
            public float eyeForward = 0.22f;

            [Tooltip("LAYOUT. Horizontal gap between the two eyes.")]
            public float eyeSeparation = 0.18f;

            [Tooltip("FEEL. Death shrink time.")]
            public float deathDuration = 0.4f;

            [Tooltip("FEEL. How far a corpse sinks.")]
            public float deathSinkDistance = 0.8f;

            [Tooltip("FEEL. Lateral push so the wave reads as separate bodies. X only, so they do not squeeze around the wall.")]
            public float separationRadius = 0.75f;

            [Tooltip("FEEL. Meters per second of lateral separation.")]
            public float separationStrength = 1.4f;

            [Tooltip("SPEC. Brute hit points. One on night 3.")]
            public float bruteHitPoints = 600f;

            [Tooltip("ESTIMATE. Brute move speed, meters per second. The spec gives hit points only.")]
            public float bruteMoveSpeed = 1.1f;

            [Tooltip("ESTIMATE. Brute damage per attack.")]
            public float bruteAttackDamage = 6f;

            [Tooltip("LAYOUT. Brute body radius. Wider than a shambler.")]
            public float bruteRadius = 0.85f;

            [Tooltip("LAYOUT. Brute body height.")]
            public float bruteHeight = 2.35f;

            [Tooltip("LAYOUT. Horn cube size.")]
            public Vector3 bruteHornSize = new Vector3(0.16f, 0.55f, 0.16f);

            [Tooltip("LAYOUT. Horn base height above the feet.")]
            public float bruteHornHeight = 2.05f;

            [Tooltip("LAYOUT. Horn spread on local X.")]
            public float bruteHornSpread = 0.42f;

            [Tooltip("LAYOUT. Horn tilt, degrees. Left horn uses the opposite sign.")]
            public float bruteHornTilt = 18f;

            [Tooltip("ESTIMATE. Above the GDD 3000 so night 5 stays close once the hero cleaves and the tower shoots.")]
            public float treantHitPoints = 5000f;

            [Tooltip("ESTIMATE. Treant move speed, meters per second. The spec gives hit points only.")]
            public float treantMoveSpeed = 0.7f;

            [Tooltip("ESTIMATE. Treant damage per attack.")]
            public float treantAttackDamage = 32f;

            [Tooltip("LAYOUT. Treant trunk diameter.")]
            public float treantTrunkDiameter = 1.2f;

            [Tooltip("LAYOUT. Treant trunk height.")]
            public float treantTrunkHeight = 3.1f;

            [Tooltip("LAYOUT. Treant canopy diameter.")]
            public float treantCanopyDiameter = 2.6f;

            [Tooltip("LAYOUT. Canopy center height above the feet.")]
            public float treantCanopyHeight = 3.4f;

            [Tooltip("FEEL. Hit flash length, seconds. A scale punch, so it does not need a unique material.")]
            public float hitFlashDuration = 0.12f;

            [Tooltip("FEEL. Extra scale at the start of a hit flash.")]
            public float hitFlashScale = 0.18f;

            [Tooltip("FEEL. Extra scale at the start of a hit flash on the boss. Smaller than hitFlashScale so a big body does not jolt.")]
            public float bossHitFlashScale = 0.06f;

            [Tooltip("LAYOUT. Boss (Treant) world height, meters, measured from its render bounds after the art is built. Any art is scaled to this; night size scaling never takes it higher. 3.6 with bossCamPullback 0.2 keeps the boss at about 35% of frame height. 0 keeps the art's own height.")]
            public float bossHeight = 3.6f;

            [Tooltip("ESTIMATE. Bodies that can bite the wall at once. About how many shamblers fit across the wall face. The rest wait a rank back. 0 means no cap.")]
            public int maxWallAttackers = 8;

            [Tooltip("LAYOUT. Extra meters of Z between ranks waiting behind the front line.")]
            public float queueSpacing = 0.95f;
        }

        [Serializable]
        public class WaveTuning
        {
            [Tooltip("SPEC. Night 1 day length, seconds.")]
            public float dayDuration = 60f;

            [Tooltip("SPEC. Day length before nights 2–5, seconds.")]
            public float laterDayDuration = 90f;

            [Tooltip("SPEC. Warning when this many seconds of day remain.")]
            public float nightWarningSeconds = 20f;

            [Tooltip("SPEC. Night clock length, seconds. Combat keeps going after the clock so a late turret still resolves the wave.")]
            public float nightDuration = 45f;

            [Tooltip("SPEC. Map 1 nights. Clearing this night wins. A dead fire still loses the night and keeps gathered progress.")]
            public int nightCount = 10;

            [Tooltip("SPEC. Night 1 shamblers.")]
            public int monsterCount = 12;

            [Tooltip("SPEC. Night 2 shamblers.")]
            public int night2Shamblers = 20;

            [Tooltip("SPEC. Night 3 shamblers.")]
            public int night3Shamblers = 30;

            [Tooltip("SPEC. Night 3 brutes (600 HP).")]
            public int night3Brutes = 1;

            [Tooltip("ESTIMATE. Night 4 shamblers. The GDD lists 40. MAP1_SPEC does not.")]
            public int night4Shamblers = 40;

            [Tooltip("ESTIMATE. Night 4 brutes. The GDD lists 2.")]
            public int night4Brutes = 2;

            [Tooltip("ESTIMATE. Night 5 shambler adds. The GDD lists 25 behind the Treant.")]
            public int night5Shamblers = 25;

            [Tooltip("SPEC. Night 5 Treant bosses (3000 HP).")]
            public int night5Treants = 1;

            [Tooltip("FEEL. Seconds between spawns.")]
            public float spawnInterval = 0.35f;

            [Tooltip("FEEL. How long the result banner is the focus. The scene does not reload.")]
            public float resultHoldSeconds = 8f;

            public void Composition(int night, out int shamblers, out int brutes, out int treants)
            {
                shamblers = monsterCount;
                brutes = 0;
                treants = 0;
                if (night == 2)
                    shamblers = night2Shamblers;
                else if (night == 3)
                {
                    shamblers = night3Shamblers;
                    brutes = night3Brutes;
                }
                else if (night == 4)
                {
                    shamblers = night4Shamblers;
                    brutes = night4Brutes;
                }
                else if (night >= 5)
                {
                    shamblers = night5Shamblers;
                    treants = night5Treants;
                }
            }
        }

        [Serializable]
        public class FireTuning
        {
            [Tooltip("GDD. Campfire hit points. Destroyed fire loses the night.")]
            public float hitPoints = 1000f;

            [Tooltip("LAYOUT.")]
            public Vector3 position = new Vector3(0f, 0f, 0f);

            [Tooltip("LAYOUT. Flame sphere diameter.")]
            public float flameDiameter = 0.85f;

            [Tooltip("LAYOUT. Flame center height.")]
            public float flameHeight = 0.7f;

            [Tooltip("LAYOUT. Log size under the flame.")]
            public Vector3 logSize = new Vector3(0.9f, 0.16f, 0.22f);

            [Tooltip("FEEL. Flame scale at 0 HP, relative to full.")]
            public float minVisualScale = 0.15f;

            [Tooltip("FEEL. Emission multiplier for the flame material.")]
            public float emission = 2.4f;
        }

        [Serializable]
        public class LayoutTuning
        {
            [Tooltip("LAYOUT. Ground width and depth, meters.")]
            public float groundExtent = 48f;

            [Tooltip("LAYOUT. Ground slab thickness. Top sits on y=0.")]
            public float groundThickness = 1f;

            [Tooltip("LAYOUT. Dirt clearing diameter around the fire.")]
            public float clearingDiameter = 15f;

            [Tooltip("LAYOUT. Dirt disc thickness.")]
            public float clearingThickness = 0.06f;

            [Tooltip("LAYOUT. Dirt disc center Y, just above the grass to avoid z-fighting.")]
            public float clearingY = 0.03f;

            [Tooltip("LAYOUT. Pour pad sits south of the wall so the hero is not inside the mesh when it rises.")]
            public Vector3 wallPadPosition = new Vector3(0f, 0f, 4.2f);

            [Tooltip("LAYOUT. Wall mesh offset from the pad, in pad-local meters. +Z pushes the wall toward the treeline.")]
            public Vector3 wallLocalOffset = new Vector3(0f, 0f, 2.6f);

            [Tooltip("LAYOUT.")]
            public Vector3 turretPadPosition = new Vector3(3.4f, 0f, 1.2f);

            [Tooltip("LAYOUT. Where the demo stands once both pads are done, south of the fire so the camera sees the wall.")]
            public Vector3 watchPosition = new Vector3(0f, 0f, -1.4f);

            [Tooltip("LAYOUT. Trees between the hero and the treeline, used before the wall blocks them.")]
            public int frontTreeCount = 10;

            [Tooltip("LAYOUT.")]
            public float frontTreeSpacing = 1.65f;

            [Tooltip("LAYOUT.")]
            public float frontTreeZ = 10.5f;

            [Tooltip("LAYOUT. Flank trees the hero can still reach after the wall is up.")]
            public float sideTreeX = 12f;

            [Tooltip("LAYOUT.")]
            public int sideTreeCountPerSide = 4;

            [Tooltip("LAYOUT.")]
            public float sideTreeZStart = 0.4f;

            [Tooltip("LAYOUT.")]
            public float sideTreeZSpacing = 2.3f;

            [Tooltip("LAYOUT.")]
            public float trunkHeight = 2.15f;

            [Tooltip("LAYOUT.")]
            public float trunkRadius = 0.28f;

            [Tooltip("LAYOUT.")]
            public float canopyDiameter = 2.05f;

            [Tooltip("LAYOUT. Canopy center height.")]
            public float canopyHeight = 2.55f;

            [Tooltip("LAYOUT.")]
            public float stumpHeight = 0.32f;

            [Tooltip("LAYOUT.")]
            public float stumpRadius = 0.34f;

            [Tooltip("LAYOUT. Monster spawn line, meters north of the origin.")]
            public float spawnZ = 18f;

            [Tooltip("LAYOUT. Half-width of the spawn line. Narrower than the wall so the wave hits the wall.")]
            public float spawnHalfWidth = 3f;

            [Tooltip("LAYOUT.")]
            public float spawnHeight = 0f;

            [Tooltip("LAYOUT. Coin pad for the stack upgrade, south of the wall.")]
            public Vector3 stackPadPosition = new Vector3(-2.4f, 0f, 0.8f);

            [Tooltip("LAYOUT. Coin pad for move speed. Kept clear of the turret pad.")]
            public Vector3 speedPadPosition = new Vector3(2.4f, 0f, -1.6f);

            [Tooltip("LAYOUT. Hut pour pad, southwest of the fire.")]
            public Vector3 hutPadPosition = new Vector3(-4.8f, 0f, -2.4f);

            [Tooltip("LAYOUT. Hut mesh offset from its pad, in pad-local meters.")]
            public Vector3 hutLocalOffset = new Vector3(0f, 0f, -1.55f);

            [Tooltip("LAYOUT. Hire pad. Stays locked until the hut is complete.")]
            public Vector3 hirePadPosition = new Vector3(-7.4f, 0f, 0.8f);

            [Tooltip("LAYOUT. Melee-upgrade pad, south of the fire.")]
            public Vector3 meleePadPosition = new Vector3(0.8f, 0f, -3.4f);

            [Tooltip("LAYOUT. Where workers dump wood once every pad is finished, relative to the hut pad.")]
            public Vector3 stockpileOffset = new Vector3(2.3f, 0f, -1.55f);

            [Tooltip("LAYOUT. Locked map-2 gate the camera pans to. North of the spawn line.")]
            public Vector3 lockedAreaPosition = new Vector3(0f, 0f, 22f);

            [Tooltip("LAYOUT. Gate slab.")]
            public Vector3 lockedGateSize = new Vector3(8f, 2.4f, 0.45f);

            [Tooltip("LAYOUT. Gate posts.")]
            public Vector3 lockedPostSize = new Vector3(0.5f, 3.3f, 0.5f);

            [Tooltip("LAYOUT. Post distance from the gate center, meters.")]
            public float lockedPostSpread = 4.3f;

            [Tooltip("LAYOUT. LOCKED label height.")]
            public float lockedLabelHeight = 3.6f;

            [Tooltip("FEEL. Max lean on a tree, degrees. Clamped to 3.")]
            public float treeTiltDegrees = 2.5f;
        }

        [Serializable]
        public class QualityTuning
        {
            [Tooltip("FEEL. Apply the mobile budget at boot: no HDR, low MSAA, render scale, one shadow cascade.")]
            public bool mobile = true;

            [Tooltip("FEEL. HDR color buffers.")]
            public bool hdr = false;

            [Tooltip("FEEL. Render scale. About 0.85 on a mid-range phone.")]
            public float renderScale = 0.85f;

            [Tooltip("FEEL. MSAA sample count. 1 is off.")]
            public int msaa = 1;

            [Tooltip("FEEL. Shadow cascades. One is the mobile budget.")]
            public int shadowCascades = 1;

            [Tooltip("FEEL. Shadow distance, meters.")]
            public float shadowDistance = 18f;

            [Tooltip("FEEL. Monster separation and target picks run on this stride.")]
            public int monsterThinkStride = 3;
        }

        [Serializable]
        public class CameraTuning
        {
            [Tooltip("SPEC / storyboard. Pitch down from the horizon, degrees. CM_Follow is 50.")]
            public float pitchDegrees = 50f;

            [Tooltip("LAYOUT. Distance from the hero, meters.")]
            public float distance = 11f;

            [Tooltip("Storyboard 40mm lens as a vertical field of view. Full-frame 40mm is about 33 degrees; 34 is the stand-in.")]
            public float fieldOfView = 34f;

            [Tooltip("LAYOUT. World yaw. 0 looks from the south toward +Z so the stack on the hero's back reads.")]
            public float yawDegrees = 0f;

            [Tooltip("LAYOUT. Look-at height on the hero.")]
            public float lookHeight = 1.3f;

            [Tooltip("FEEL. Position SmoothDamp time.")]
            public float followSmoothTime = 0.14f;

[Tooltip("FEEL. Look-point SmoothDamp time.")]
public float lookSmoothTime = 0.08f;

[Tooltip("LAYOUT. Camera near clip. Rendering, not gameplay.")]
public float nearClip = 0.08f;

[Tooltip("LAYOUT. Camera far clip.")]
public float farClip = 180f;

            [Tooltip("FEEL. Seconds to pan out to the locked gate.")]
            public float teaseDuration = 1.6f;

            [Tooltip("FEEL. Hold on the locked gate.")]
            public float teaseHold = 1.5f;

            [Tooltip("FEEL. Seconds to pan back to the hero.")]
            public float teaseReturnDuration = 1.3f;

            [Tooltip("LAYOUT. Look height on the locked gate during the tease.")]
            public float teaseLookHeight = 1.6f;

            [Tooltip("FEEL. Extra follow distance while a live boss is near the hero, as a fraction of distance. 0.2 is 20% farther.")]
            public float bossCamPullback = 0.2f;

            [Tooltip("LAYOUT. Horizontal meters from the hero within which a live boss triggers the pullback.")]
            public float bossCamRange = 12f;

            [Tooltip("FEEL. Seconds to blend the boss pullback in or out.")]
            public float bossCamBlendTime = 0.6f;
        }

        [Serializable]
        public class JoystickTuning
        {
            [Tooltip("LAYOUT. Base diameter in reference pixels.")]
            public float baseSize = 168f;

            [Tooltip("LAYOUT. Knob diameter in reference pixels.")]
            public float knobSize = 74f;

            [Tooltip("LAYOUT. Knob travel from the center, reference pixels.")]
            public float travel = 58f;

            [Tooltip("FEEL. Stick magnitudes below this are zero. Keyboard is digital and is not deadzoned.")]
            public float deadzone = 0.12f;

            [Tooltip("LAYOUT. Inset from the bottom-left of the reference resolution.")]
            public float margin = 52f;

            [Tooltip("LAYOUT. Generated circle texture size. Not gameplay.")]
            public int spriteResolution = 64;

            [Tooltip("FEEL. Base image alpha.")]
            public float baseAlpha = 0.42f;

            [Tooltip("FEEL. Knob image alpha.")]
            public float knobAlpha = 0.92f;
        }

        [Serializable]
        public class HudTuning
        {
            [Tooltip("LAYOUT. Canvas scaler reference width.")]
            public float referenceWidth = 1920f;

            [Tooltip("LAYOUT. Canvas scaler reference height.")]
            public float referenceHeight = 1080f;

            public int stackFontSize = 34;
            public int timerFontSize = 56;
            public int hintFontSize = 30;
            public int resultFontSize = 68;
            public int fireFontSize = 28;
            public int controlsFontSize = 22;

[Tooltip("LAYOUT. Insets for the corner labels, reference pixels.")]
public float labelMargin = 36f;

[Tooltip("LAYOUT. Canvas scaler blend. 0.5 weights width and height equally.")]
public float matchWidthOrHeight = 0.5f;

public float stackPanelWidth = 700f;
public float sidePanelWidth = 420f;
public float widePanelWidth = 1400f;
public float panelHeight = 80f;
public float timerPanelHeight = 120f;
public float resultPanelHeight = 160f;
public float controlsPanelHeight = 50f;

            [Tooltip("FEEL. Timer scale at the peak of the night-warning pulse.")]
            public float timerWarningScale = 1.14f;

            [Tooltip("FEEL. Warning pulse speed.")]
            public float timerPulseSpeed = 5f;

            [Tooltip("LAYOUT. World-space label rectangle, canvas pixels.")]
            public Vector2 worldLabelSize = new Vector2(280f, 140f);

            [Tooltip("LAYOUT. World canvas scale. 0.012 turns a 280px label into about 3.4 meters.")]
            public float worldCanvasScale = 0.012f;

            public int worldLabelFontSize = 42;

            [Tooltip("FEEL. Damage number lifetime.")]
            public float damageNumberDuration = 0.65f;

            [Tooltip("FEEL. Damage number rise, meters.")]
            public float damageNumberRise = 1.15f;

            [Tooltip("LAYOUT.")]
            public float damageNumberScale = 0.008f;

            public int damageNumberFontSize = 56;
        }

        [Serializable]
        public class PresentationTuning
        {
            public Color grass = new Color(0.34f, 0.46f, 0.26f, 1f);
            public Color dirt = new Color(0.45f, 0.36f, 0.22f, 1f);
            public Color heroBody = new Color(0.18f, 0.42f, 0.78f, 1f);
            public Color heroHead = new Color(0.93f, 0.76f, 0.58f, 1f);
            public Color axe = new Color(0.55f, 0.57f, 0.6f, 1f);
            public Color plank = new Color(0.76f, 0.55f, 0.28f, 1f);
            public Color trunk = new Color(0.38f, 0.25f, 0.13f, 1f);
            public Color canopy = new Color(0.18f, 0.46f, 0.2f, 1f);
            public Color stump = new Color(0.32f, 0.22f, 0.12f, 1f);
            public Color padWall = new Color(0.95f, 0.78f, 0.2f, 1f);
            public Color padTurret = new Color(0.95f, 0.42f, 0.16f, 1f);
            public Color padActive = new Color(1f, 0.95f, 0.55f, 1f);
            public Color padComplete = new Color(0.4f, 0.82f, 0.42f, 1f);
            public Color wall = new Color(0.58f, 0.4f, 0.22f, 1f);
            public Color wallReinforced = new Color(0.48f, 0.44f, 0.38f, 1f);
            public Color wallStone = new Color(0.66f, 0.68f, 0.7f, 1f);
            public Color padSpike = new Color(0.72f, 0.2f, 0.18f, 1f);
            public Color padGate = new Color(0.78f, 0.64f, 0.28f, 1f);
            public Color padTower = new Color(0.22f, 0.55f, 0.4f, 1f);
            public Color padUpgrade = new Color(0.95f, 0.5f, 0.12f, 1f);
            public Color padRepair = new Color(0.4f, 0.95f, 0.82f, 1f);
            public Color rock = new Color(0.52f, 0.54f, 0.56f, 1f);
            public Color stoneChunk = new Color(0.7f, 0.72f, 0.74f, 1f);
            public Color iron = new Color(0.63f, 0.67f, 0.73f, 1f);
            public Color spike = new Color(0.32f, 0.33f, 0.35f, 1f);
            public Color turret = new Color(0.25f, 0.28f, 0.32f, 1f);
            public Color turretAccent = new Color(0.15f, 0.62f, 0.9f, 1f);
            public Color monster = new Color(0.12f, 0.13f, 0.12f, 1f);
            public Color monsterEye = new Color(1f, 0.22f, 0.12f, 1f);
            public Color brute = new Color(0.42f, 0.16f, 0.12f, 1f);
            public Color bruteHorn = new Color(0.72f, 0.66f, 0.52f, 1f);
            public Color fire = new Color(1f, 0.48f, 0.08f, 1f);
            public Color fireLog = new Color(0.3f, 0.16f, 0.08f, 1f);
            public Color bolt = new Color(1f, 0.92f, 0.45f, 1f);
            public Color daySky = new Color(0.58f, 0.76f, 0.92f, 1f);
            public Color duskSky = new Color(0.86f, 0.42f, 0.24f, 1f);
            public Color nightSky = new Color(0.05f, 0.07f, 0.14f, 1f);
            public Color daySun = new Color(1f, 0.96f, 0.86f, 1f);
            public Color nightSun = new Color(0.55f, 0.66f, 0.95f, 1f);
            public Color warningText = new Color(1f, 0.32f, 0.22f, 1f);
            public Color hudText = new Color(0.96f, 0.94f, 0.88f, 1f);
            public Color resultWin = new Color(0.55f, 0.95f, 0.55f, 1f);
            public Color resultFail = new Color(1f, 0.45f, 0.35f, 1f);
            public Color joystickBase = new Color(0.05f, 0.06f, 0.08f, 1f);
            public Color joystickKnob = new Color(0.95f, 0.95f, 0.92f, 1f);
            public Color coin = new Color(1f, 0.82f, 0.18f, 1f);
            public Color padBag = new Color(0.25f, 0.62f, 0.95f, 1f);
            public Color padSpeed = new Color(0.58f, 0.38f, 0.92f, 1f);
            public Color padHut = new Color(0.62f, 0.4f, 0.22f, 1f);
            public Color padHire = new Color(0.95f, 0.78f, 0.28f, 1f);
            public Color hutBody = new Color(0.55f, 0.34f, 0.18f, 1f);
            public Color hutRoof = new Color(0.45f, 0.16f, 0.12f, 1f);
            public Color hutDoor = new Color(0.16f, 0.1f, 0.07f, 1f);
            public Color workerBody = new Color(0.18f, 0.55f, 0.38f, 1f);
            public Color workerHead = new Color(0.93f, 0.76f, 0.58f, 1f);
            public Color lockedGate = new Color(0.32f, 0.3f, 0.36f, 1f);
            public Color lockedPost = new Color(0.42f, 0.4f, 0.46f, 1f);
            public Color meat = new Color(0.72f, 0.18f, 0.16f, 1f);
            public Color bone = new Color(0.9f, 0.86f, 0.72f, 1f);
            public Color hide = new Color(0.55f, 0.34f, 0.18f, 1f);
            public Color core = new Color(0.35f, 0.85f, 0.95f, 1f);
            public Color sap = new Color(0.45f, 0.78f, 0.28f, 1f);
            public Color boar = new Color(0.48f, 0.28f, 0.18f, 1f);
            public Color health = new Color(0.35f, 0.82f, 0.32f, 1f);
            public Color healthLow = new Color(0.9f, 0.22f, 0.16f, 1f);
            public Color bossBar = new Color(0.62f, 0.18f, 0.72f, 1f);

            [Tooltip("FEEL. Emission multiplier on loot coins.")]
            public float coinEmission = 2.6f;

            [Tooltip("FEEL. Directional light intensity at noon.")]
            public float daySunIntensity = 1.15f;

            [Tooltip("FEEL. Directional light intensity at night.")]
            public float nightSunIntensity = 0.32f;

            [Tooltip("LAYOUT. Sun pitch at noon.")]
            public float daySunPitch = 52f;

            [Tooltip("LAYOUT. Sun pitch at night.")]
            public float nightSunPitch = 24f;

            [Tooltip("LAYOUT. Sun yaw.")]
            public float sunYaw = -38f;

            [Tooltip("FEEL. Where dusk sits in the day-to-night blend, 0 to 1.")]
            public float duskMidpoint = 0.45f;

            [Tooltip("FEEL. Flat ambient multiplier on the sky color.")]
            public float ambientDay = 0.55f;

            [Tooltip("FEEL.")]
            public float ambientNight = 0.22f;

            [Tooltip("FEEL. URP Lit smoothness written onto runtime materials.")]
            public float materialSmoothness = 0.16f;

            [Tooltip("FEEL.")]
            public float materialMetallic = 0f;

            [Tooltip("FEEL. Eye emission multiplier.")]
            public float eyeEmission = 3.2f;

            [Tooltip("FEEL. Bolt emission multiplier.")]
            public float boltEmission = 2.2f;

            [Tooltip("SPEC juice. Time scale during the build hit-stop.")]
            public float hitStopScale = 0.05f;

            [Tooltip("SPEC juice. Hit-stop length in real seconds. Storyboard uses 80ms.")]
            public float hitStopSeconds = 0.08f;

            [Tooltip("LAYOUT. Player window width. Also used when the Linux player is built.")]
            public int windowWidth = 1280;

            [Tooltip("LAYOUT. Player window height.")]
            public int windowHeight = 720;

            [Tooltip("FEEL. Caps the player so a software GPU does not sprint past the design.")]
            public int targetFrameRate = 60;

            [Tooltip("LAYOUT. Shadows off keeps the software-GL capture playable. Flip for a real GPU.")]
            public bool enableShadows = false;

            [Tooltip("FEEL. Keep simulating when the window is unfocused so a capture still runs.")]
            public bool runInBackground = true;

            [Tooltip("FEEL. Seconds between one-line status logs.")]
            public float statusLogInterval = 2f;

            [Tooltip("LAYOUT. Fill bar world height at 100% pour or full HP.")]
            public float fillBarHeight = 1.7f;

            [Tooltip("LAYOUT.")]
            public Vector3 fillBarSize = new Vector3(0.18f, 1f, 0.18f);
        }

        [Serializable]
        public class CopyTuning
        {
            public string woodLabel = "WOOD";
            public string untilNight = "UNTIL NIGHT";
            public string nightPrefix = "N";
            public string hordeLabel = "HORDE";
            public string fireLabel = "FIRE";
            public string hintChop = "Stack wood!";
            public string hintPourWall = "Stand on the yellow pad — pour the wall";
            public string hintPourTurret = "Stand on the orange pad — pour the turret";
            public string hintStackFull = "Stack full — pour it into a pad";
            public string hintReady = "Wall and turret are up. Brace for night.";
            public string hintNight = "Night falls. The turret takes the shot.";
            public string hintNoTurret = "No turret yet — the horde is on the wall.";
            public string resultClear = "The clearing holds";
            public string resultFail = "The fire went out";
            public string wallName = "WALL";
            public string turretName = "TURRET";
            public string controls = "WASD or the joystick";
            public string coinsLabel = "COINS";
            public string crewLabel = "CREW";
            public string speedLabel = "SPEED";
            public string bagName = "BAG";
            public string hutName = "HUT";
            public string hireName = "HIRE";
            public string lockedLabel = "LOCKED";
            public string maxLabel = "MAX";
            public string stockpileName = "STOCK";
            public string hintPourHut = "Stand on the brown pad — build the hut";
            public string hintUpgradeBag = "Stand on the blue pad — bigger stack";
            public string hintUpgradeSpeed = "Stand on the purple pad — move faster";
            public string hintHire = "Stand on the gold pad to hire a worker";
            public string hintWorkers = "Workers chop and haul on their own.";
            public string offlineTitle = "WELCOME BACK";
            public string offlineCoins = "coins";
            public string offlinePlanks = "planks";
            public string offlineAway = "away for";
            public string offlineHours = "h";
            public string offlineMinutes = "m";
            public string offlineCollect = "COLLECT";
            public string offlineTriple = "×3 (ad)";
            public string stoneLabel = "STONE";
            public string ironLabel = "IRON";
            public string spikeName = "SPIKE";
            public string gateName = "GATE";
            public string towerName = "TOWER";
            public string upgradeName = "FASTER";
            public string reinforcedName = "REINFORCED";
            public string stoneWallName = "STONE WALL";
            public string hintRepair = "Glowing pad — pour wood to repair";
            public string hintMine = "Rocks are open — mine stone";
            public string assignWood = "WOOD CREW";
            public string assignStone = "STONE CREW";
            public string meatLabel = "MEAT";
            public string boneLabel = "BONE";
            public string hideLabel = "HIDE";
            public string coreLabel = "CORE";
            public string sapLabel = "SAP";
            public string meleeName = "MELEE";
            public string armorName = "ARMOR";
            public string packName = "PACK";
            public string cookName = "COOK";
            public string mealName = "MEAL";
            public string researchName = "RESEARCH";
            public string goalWall = "Build the wall";
            public string goalTurret = "Build the turret";
            public string goalHut = "Build the hut";
            public string goalRepair = "Repair the defenses";
            public string goalUpgrade = "Upgrade the defenses";
            public string goalNight = "Survive the night";
            public string powerLabel = "POWER";
            public string threatLabel = "THREAT";
            public string needLabel = "Need";
            public string goalCook = "Cook at the fire";
            public string goalHero = "Upgrade the hero";
            public string nightBanner = "NIGHT";
            public string waveLabel = "WAVE";
            public string bossName = "TREANT";
            public string dawnTitle = "DAWN";
            public string maxBadge = "MAX";
            public string map2Label = "MAP 2";
        }

        [Serializable]
        public class LootTuning
        {
            [Tooltip("SPEC. Coins dropped per monster kill. Each pickup is worth 1.")]
            public int coinsPerKill = 5;

            [Tooltip("FEEL. Hero collects a coin inside this radius, meters.")]
            public float pickupRadius = 1.15f;

            [Tooltip("FEEL. Coins fly to the hero inside this radius. Wide enough to cross the clearing.")]
            public float magnetRadius = 30f;

            [Tooltip("FEEL. Magnet speed, meters per second.")]
            public float magnetSpeed = 18f;

            [Tooltip("FEEL. Ring radius the coins pop out to.")]
            public float scatterRadius = 0.85f;

            [Tooltip("FEEL. Pop arc time.")]
            public float popDuration = 0.28f;

            [Tooltip("FEEL. Pop arc height, meters.")]
            public float popArcHeight = 1.35f;

            [Tooltip("LAYOUT. Coin rest height.")]
            public float restHeight = 0.32f;

            [Tooltip("LAYOUT. Spawn height above the corpse.")]
            public float dropHeight = 0.9f;

            [Tooltip("LAYOUT. Coin diameter.")]
            public float coinDiameter = 0.34f;

            [Tooltip("LAYOUT. Coin thickness.")]
            public float coinThickness = 0.08f;

            [Tooltip("FEEL. Spin, degrees per second.")]
            public float spinDegreesPerSecond = 200f;
        }

        [Serializable]
        public class UpgradeTuning
        {
            [Tooltip("SPEC. First stack-capacity purchase.")]
            public int stackBaseCost = 50;

            [Tooltip("SPEC. Stack cost multiplier per level already owned.")]
            public float stackCostMultiplier = 1.6f;

            [Tooltip("SPEC. Capacity at level 0.")]
            public int capacityLevel0 = 20;

            [Tooltip("SPEC. Capacity at level 1.")]
            public int capacityLevel1 = 40;

            [Tooltip("SPEC. Capacity at level 2.")]
            public int capacityLevel2 = 60;

            [Tooltip("SPEC. Capacity at level 3.")]
            public int capacityLevel3 = 90;

            [Tooltip("SPEC. Capacity at level 4.")]
            public int capacityLevel4 = 120;

            [Tooltip("ESTIMATE. First move-speed purchase. MAP1_SPEC prices the bag pad only, so this copies that curve.")]
            public int speedBaseCost = 50;

            [Tooltip("ESTIMATE. Speed cost multiplier. Same curve as the bag.")]
            public float speedCostMultiplier = 1.6f;

            [Tooltip("SPEC. Fractional speed added per level. 0.1 is +10%.")]
            public float speedBonusPerLevel = 0.1f;

            [Tooltip("SPEC. Move-speed cap, meters per second.")]
            public float speedCap = 7.5f;

            [Tooltip("SPEC. Levels that can be bought before the cap. 5 * 1.1^5 meets 7.5.")]
            public int speedMaxLevels = 5;

            [Tooltip("First worker hire. Matches the night-1 haul (12 kills × 5 coins) so the first hire lands right after that payoff.")]
            public int hireBaseCost = 60;

            [Tooltip("Hire cost multiplier per worker already hired. Ceil(60 × 1.5^level) is 60, 90, 135, 203.")]
            public float hireCostMultiplier = 1.5f;

            [Tooltip("SPEC. Worker cap.")]
            public int maxWorkers = 4;

            [Tooltip("FEEL. Seconds standing on a coin pad before it spends. One purchase per visit.")]
            public float payDelay = 0.6f;

            [Tooltip("SPEC. Highest stack level. Level 0 is the starting bag; level 4 is 120.")]
            public int maxStackLevel = 4;

            public int CapacityForLevel(int level)
            {
                if (level <= 0)
                    return capacityLevel0;
                if (level == 1)
                    return capacityLevel1;
                if (level == 2)
                    return capacityLevel2;
                if (level == 3)
                    return capacityLevel3;
                return capacityLevel4;
            }
        }

        [Serializable]
        public class WorkerTuning
        {
            [Tooltip("SPEC. Move speed, meters per second.")]
            public float moveSpeed = 3.5f;

            [Tooltip("SPEC. Planks a worker can carry.")]
            public int carryCapacity = 15;

            [Tooltip("LAYOUT. Body radius.")]
            public float bodyRadius = 0.28f;

            [Tooltip("LAYOUT. Body height.")]
            public float bodyHeight = 0.8f;

            [Tooltip("LAYOUT. Head diameter.")]
            public float headDiameter = 0.3f;

            [Tooltip("LAYOUT. Head center height.")]
            public float headHeight = 1.02f;

            [Tooltip("FEEL. Yaw response.")]
            public float turnSharpness = 10f;

            [Tooltip("FEEL. Fraction of chop range that counts as arrived.")]
            public float chopRangeFactor = 0.82f;

            [Tooltip("ESTIMATE. Planks per second a worker pours. The spec does not set a worker pour rate.")]
            public float deliverPlanksPerSecond = 8f;

            [Tooltip("LAYOUT. How close a worker stands to a pad, meters. Inside the pour radius.")]
            public float padArriveDistance = 0.9f;

            [Tooltip("LAYOUT. How close a worker stands to the stockpile.")]
            public float stockArriveDistance = 0.7f;

            [Tooltip("LAYOUT. First worker offset from the hut pad.")]
            public Vector3 spawnLocal = new Vector3(1.1f, 0f, 1.5f);

            [Tooltip("LAYOUT. Extra X offset per hired worker.")]
            public float spawnSpacing = 0.8f;

            [Tooltip("FEEL. Lateral separation between workers.")]
            public float separationRadius = 0.7f;

            [Tooltip("FEEL. Separation push, meters per second.")]
            public float separationStrength = 1.2f;
        }

        [Serializable]
        public class OfflineTuning
        {
            [Tooltip("GDD. Workers keep producing while the app is closed.")]
            public bool enabled = true;

            [Tooltip("ESTIMATE. Offline coins per hired worker per second. 0.03 is ~108 coins/hour per worker; 4 workers for 2 h is ~864.")]
            public float coinsPerWorkerSecond = 0.03f;

            [Tooltip("ESTIMATE. Offline planks per hired worker per second, banked to the stockpile. 0.01 is 36/hour per worker.")]
            public float planksPerWorkerSecond = 0.01f;

            [Tooltip("ESTIMATE. Most planks banked in one return. Matches stack.maxRenderedPlanks.")]
            public int maxPlanks = 60;

            [Tooltip("GDD. Offline cap, seconds. 2 h now; research can raise it to 8 h (28800).")]
            public float capSeconds = 7200f;

            [Tooltip("ESTIMATE. Shortest absence, seconds, that earns anything or shows the popup.")]
            public float minAwaySeconds = 60f;

            [Tooltip("ESTIMATE. Worker count assumed for -offline QA runs when fewer are hired.")]
            public int qaMinWorkers = 2;

            [Tooltip("GDD. Shows the ×3 rewarded-video button. Off until an ad SDK exists; it collects ×1 for now.")]
            public bool showAdButton = false;

            [Tooltip("LAYOUT. Popup canvas sorting order, above the HUD (10).")]
            public int sortingOrder = 40;

            [Tooltip("LAYOUT. Popup panel size, reference pixels.")]
            public Vector2 panelSize = new Vector2(760f, 520f);

            [Tooltip("LAYOUT. Button size, reference pixels.")]
            public Vector2 buttonSize = new Vector2(280f, 84f);

            [Tooltip("LAYOUT. Gap between the two buttons, reference pixels.")]
            public float buttonGap = 40f;

            [Tooltip("LAYOUT. Button centre height above the panel bottom, reference pixels.")]
            public float buttonLift = 70f;

            [Tooltip("LAYOUT.")]
            public int titleFontSize = 60;

            [Tooltip("LAYOUT.")]
            public int bodyFontSize = 40;

            [Tooltip("LAYOUT.")]
            public int awayFontSize = 28;

            [Tooltip("LAYOUT.")]
            public int buttonFontSize = 30;

            [Tooltip("FEEL. Full-screen dim behind the popup; also blocks taps until COLLECT.")]
            public Color dimColor = new Color(0f, 0f, 0f, 0.6f);

            [Tooltip("FEEL.")]
            public Color panelColor = new Color(0.16f, 0.12f, 0.09f, 0.96f);

            [Tooltip("FEEL.")]
            public Color buttonColor = new Color(0.86f, 0.62f, 0.16f, 1f);

            [Tooltip("FEEL.")]
            public Color adButtonColor = new Color(0.24f, 0.5f, 0.28f, 1f);
        }

        [Serializable]
        public class DemoTuning
        {
            [Tooltip("Launch with -demo, or enable this, to play the loop without input.")]
            public bool runByDefault = false;

            [Tooltip("FEEL. How often the demo picks a new goal.")]
            public float repickInterval = 0.2f;

            [Tooltip("FEEL. Fraction of chop range that counts as arrived.")]
public float chopRangeFactor = 0.78f;

[Tooltip("FEEL. Fraction of pad radius that counts as arrived. High enough to stay inside the pad, low enough to stay south of the wall mesh.")]
public float padRangeFactor = 0.62f;

            [Tooltip("FEEL. Arrival distance for the watch point.")]
            public float watchArrive = 0.4f;

            [Tooltip("FEEL. Stack upgrades the demo buys only when coins are left after the hire.")]
            public int stackLevelsToBuy = 1;

            [Tooltip("FEEL. Speed upgrades the demo buys only when coins are left after the hire.")]
            public int speedLevelsToBuy = 1;

            [Tooltip("FEEL. Workers the demo hires. The first hire spends the night-1 haul, before bag or speed.")]
            public int workersToHire = 1;

            [Tooltip("FEEL. Melee-damage purchases the demo takes when it can pay.")]
            public int meleeLevelsToBuy = 3;

            [Tooltip("FEEL. Hide armor purchases the demo takes.")]
            public int armorLevelsToBuy = 2;

            [Tooltip("FEEL. Hide backpack purchases the demo takes.")]
            public int packLevelsToBuy = 2;

            [Tooltip("FEEL. How far the demo hero stands from a working crew member. Outside chop range, inside the camera.")]
            public float workerFollowGap = 2.4f;

            [Tooltip("ESTIMATE. Demo repairs a wall or gate that has fallen under this fraction of its hit points. 1 tops a structure off so the next tier can be poured.")]
            public float repairBelowFraction = 1f;
        }

        [Serializable]
        public class DefenseTuning
        {
            [Tooltip("ESTIMATE. Hit points restored per wood plank poured into a damaged wall or gate.")]
            public float repairHitPointsPerWood = 8f;

            [Tooltip("ESTIMATE. Fire hit points restored at dawn. A top-up, not a full heal.")]
            public float dawnFireHeal = 120f;

            [Tooltip("ESTIMATE. Reinforced wall wood cost. Same pad as the wood wall. Unlocks day 2.")]
            public int reinforcedWood = 18;

            [Tooltip("ESTIMATE. Reinforced wall stone cost.")]
            public int reinforcedStone = 12;

            [Tooltip("GDD. Reinforced wall is about 600 HP.")]
            public float reinforcedHitPoints = 600f;

            [Tooltip("ESTIMATE. Night index when the reinforced tier can be poured. 2 is the day after night 1.")]
            public int reinforcedUnlockNight = 2;

            [Tooltip("ESTIMATE. Stone wall wood cost. Needs iron from brutes.")]
            public int stoneWood = 14;

            [Tooltip("ESTIMATE. Stone wall stone cost.")]
            public int stoneStone = 10;

            [Tooltip("ESTIMATE. Stone wall iron cost.")]
            public int stoneIron = 4;

            [Tooltip("ESTIMATE. Stone wall hit points.")]
            public float stoneHitPoints = 1100f;

            [Tooltip("ESTIMATE. Night index when the stone tier can be poured. Iron exists after night 3.")]
            public int stoneUnlockNight = 4;

            [Tooltip("ESTIMATE. Spike trap wood cost. Unlocks day 2.")]
            public int spikeWood = 18;

            [Tooltip("ESTIMATE. Night index when the spike pad appears.")]
            public int spikeUnlockNight = 2;

            [Tooltip("ESTIMATE. Damage per second to a shambler standing on the spike pad. Brutes and the Treant ignore it.")]
            public float spikeDamagePerSecond = 16f;

            [Tooltip("ESTIMATE. Horizontal radius of the spike hazard.")]
            public float spikeRadius = 1.7f;

            [Tooltip("LAYOUT. Spike pad. North of the gate so the horde walks over it.")]
            public Vector3 spikePosition = new Vector3(0f, 0f, 14.6f);

            [Tooltip("ESTIMATE. Buildable gate wood cost. Unlocks day 3.")]
            public int gateWood = 40;

            [Tooltip("ESTIMATE. Gate hit points.")]
            public float gateHitPoints = 500f;

            [Tooltip("ESTIMATE. Night index when the gate pad appears.")]
            public int gateUnlockNight = 3;

            [Tooltip("LAYOUT. Gate pour pad. South of the wood wall so the hero never has to cross the lane to build it.")]
            public Vector3 gatePosition = new Vector3(0f, 0f, 3f);

            [Tooltip("LAYOUT. Gate mesh offset from the pad, toward the spawn line. Keeps the slab inside turret range.")]
            public Vector3 gateLocalOffset = new Vector3(0f, 0f, 6.15f);

            [Tooltip("LAYOUT. Gate slab size.")]
            public Vector3 gateSize = new Vector3(6.4f, 2.35f, 0.48f);

            [Tooltip("ESTIMATE. Watchtower wood cost. Unlocks day 4.")]
            public int towerWood = 28;

            [Tooltip("ESTIMATE. Watchtower stone cost.")]
            public int towerStone = 10;

            [Tooltip("ESTIMATE. Night index when the watchtower pad appears.")]
            public int towerUnlockNight = 4;

            [Tooltip("GDD. 15 damage * 2 shots = 30 DPS at level 1.")]
            public float towerDamage = 15f;

            [Tooltip("GDD. Archer shots per second at level 1.")]
            public float towerShotsPerSecond = 2f;

            [Tooltip("ESTIMATE. Archer range, meters. Longer than the turret.")]
            public float towerRange = 18f;

            [Tooltip("LAYOUT. Watchtower pad.")]
            public Vector3 towerPosition = new Vector3(6.8f, 0f, 2.4f);

            [Tooltip("LAYOUT. Tower height.")]
            public float towerHeight = 3.4f;

            [Tooltip("LAYOUT. Tower post radius.")]
            public float towerRadius = 0.38f;

            [Tooltip("ESTIMATE. Faster-turret wood cost. Unlocks day 5.")]
            public int turretUpgradeWood = 12;

            [Tooltip("ESTIMATE. Faster-turret iron cost.")]
            public int turretUpgradeIron = 4;

            [Tooltip("ESTIMATE. Night index when the turret upgrade pad appears.")]
            public int turretUpgradeUnlockNight = 5;

            [Tooltip("GDD. 50 DPS * 1.6 = 80 DPS at turret level 2.")]
            public float turretRateMultiplier = 1.6f;

            [Tooltip("LAYOUT. Turret upgrade pad, south of the turret.")]
            public Vector3 turretUpgradePosition = new Vector3(3.4f, 0f, -1.6f);

            [Tooltip("ESTIMATE. Watchtower hit points. Monsters can attack it.")]
            public float towerHitPoints = 720f;

            [Tooltip("ESTIMATE. Turret hit points. Monsters can attack it.")]
            public float turretHitPoints = 640f;

            [Tooltip("ESTIMATE. A monster at the wall peels onto a tower or the turret inside this range, meters.")]
            public float flankAttackRange = 6.4f;

            [Tooltip("ESTIMATE. Stone-wall hits slow the attacker.")]
            public float staggerSeconds = 0.45f;

            [Tooltip("ESTIMATE. Speed scale while staggered.")]
            public float staggerSpeedScale = 0.35f;

            [Tooltip("ESTIMATE. Gate level 2 wood.")]
            public int gateLv2Wood = 12;

            [Tooltip("ESTIMATE. Gate level 2 iron.")]
            public int gateLv2Iron = 4;

            [Tooltip("GDD. Iron-bound gate hit points.")]
            public float gateLv2HitPoints = 1000f;

            [Tooltip("ESTIMATE. Night index when gate level 2 can be poured.")]
            public int gateLv2UnlockNight = 4;

            [Tooltip("ESTIMATE. Gate level 3 wood.")]
            public int gateLv3Wood = 10;

            [Tooltip("ESTIMATE. Gate level 3 stone.")]
            public int gateLv3Stone = 8;

            [Tooltip("ESTIMATE. Gate level 3 iron.")]
            public int gateLv3Iron = 4;

            [Tooltip("GDD. Portcullis hit points.")]
            public float gateLv3HitPoints = 1800f;

            [Tooltip("ESTIMATE. Night index when gate level 3 can be poured.")]
            public int gateLv3UnlockNight = 5;

            [Tooltip("ESTIMATE. Damage per second a level-3 gate deals to its attacker.")]
            public float gateThornsPerSecond = 8f;

            [Tooltip("ESTIMATE. Watchtower level 2 wood. Two archers, 60 DPS.")]
            public int towerLv2Wood = 10;

            [Tooltip("ESTIMATE. Watchtower level 2 stone.")]
            public int towerLv2Stone = 6;

            [Tooltip("GDD. Rate scale from 30 DPS to 60 DPS.")]
            public float towerLv2RateScale = 2f;

            [Tooltip("ESTIMATE. Night index when the second archer can be poured.")]
            public int towerLv2UnlockNight = 5;

            [Tooltip("ESTIMATE. Watchtower level 3 wood.")]
            public int towerLv3Wood = 8;

            [Tooltip("ESTIMATE. Watchtower level 3 iron.")]
            public int towerLv3Iron = 4;

            [Tooltip("ESTIMATE. Damage scale for fire arrows.")]
            public float towerLv3DamageScale = 1.35f;

            [Tooltip("GDD. Fire-arrow range, meters.")]
            public float towerLv3Range = 22f;

            [Tooltip("ESTIMATE. Burn damage per second.")]
            public float towerBurnDamage = 4f;

            [Tooltip("ESTIMATE. Burn duration, seconds.")]
            public float towerBurnSeconds = 2.2f;

            [Tooltip("ESTIMATE. Night index when fire arrows can be poured.")]
            public int towerLv3UnlockNight = 5;

            [Tooltip("GDD. Turret level 2 range, meters. Bolts pierce 2.")]
            public float turretLv2Range = 15f;

            [Tooltip("GDD. Targets a level-2 bolt can pass through.")]
            public int turretPierce = 2;

            [Tooltip("ESTIMATE. Sap spent on turret level 3.")]
            public int turretLv3Sap = 2;

            [Tooltip("GDD. 50 * 2.4 = 120 DPS.")]
            public float turretLv3RateScale = 2.4f;

            [Tooltip("GDD. Fire-bolt range, meters.")]
            public float turretLv3Range = 18f;

            [Tooltip("ESTIMATE. Fire-bolt splash radius, meters.")]
            public float turretSplashRadius = 1.6f;

            [Tooltip("ESTIMATE. Bones for spike level 2. Those spikes hit brutes.")]
            public int spikeLv2Bones = 8;

            [Tooltip("ESTIMATE. Spike level 2 damage per second.")]
            public float spikeLv2Damage = 28f;

            [Tooltip("ESTIMATE. Bones for spike level 3.")]
            public int spikeLv3Bones = 6;

            [Tooltip("ESTIMATE. Iron for spike level 3.")]
            public int spikeLv3Iron = 3;

            [Tooltip("ESTIMATE. Spike level 3 damage per second.")]
            public float spikeLv3Damage = 40f;

            [Tooltip("ESTIMATE. Wood for the stone fire ring.")]
            public int fireLv2Wood = 6;

            [Tooltip("ESTIMATE. Stone for the stone fire ring.")]
            public int fireLv2Stone = 8;

            [Tooltip("GDD. Stone-ring campfire hit points.")]
            public float fireLv2HitPoints = 1500f;

            [Tooltip("ESTIMATE. Night index when the fire ring can be built. Stone exists.")]
            public int fireLv2UnlockNight = 2;

            [Tooltip("ESTIMATE. Wood for the bonfire.")]
            public int fireLv3Wood = 8;

            [Tooltip("ESTIMATE. Iron for the bonfire.")]
            public int fireLv3Iron = 3;

            [Tooltip("GDD. Bonfire hit points.")]
            public float fireLv3HitPoints = 2200f;

            [Tooltip("ESTIMATE. Night index when the bonfire can be built.")]
            public int fireLv3UnlockNight = 4;

            [Tooltip("ESTIMATE. Warmth radius at campfire level 3, meters.")]
            public float warmthRadius = 4.2f;

            [Tooltip("ESTIMATE. Warmth heal per second for the hero.")]
            public float warmthHealPerSecond = 4f;
        }

        [Serializable]
        public class SupplyTuning
        {
            [Tooltip("ESTIMATE. Stone the hero can carry.")]
            public int heroStoneCapacity = 16;

            [Tooltip("ESTIMATE. Stone a worker assigned to stone can carry.")]
            public int workerStoneCapacity = 10;

            [Tooltip("ESTIMATE. Hits to break a rock.")]
            public int hitsToBreakRock = 3;

            [Tooltip("ESTIMATE. Stone gained per hit.")]
            public int stonePerHit = 2;

            [Tooltip("ESTIMATE. Seconds before a broken rock returns.")]
            public float rockRespawnSeconds = 12f;

            [Tooltip("ESTIMATE. Horizontal mine reach, meters.")]
            public float mineRange = 1.8f;

            [Tooltip("ESTIMATE. Iron dropped by each brute. Tier-3 walls and the turret upgrade spend it.")]
            public int ironPerBrute = 4;

            [Tooltip("ESTIMATE. Night index when the rock row appears. 2 is the dawn after night 1.")]
            public int rockUnlockNight = 2;

            [Tooltip("LAYOUT. Rocks in the east row.")]
            public int rockCount = 6;

            [Tooltip("LAYOUT. Rock row X.")]
            public float rockX = 15.2f;

            [Tooltip("LAYOUT. First rock Z.")]
            public float rockZStart = -1.2f;

            [Tooltip("LAYOUT. Spacing down the rock row.")]
            public float rockZSpacing = 2.15f;

            [Tooltip("LAYOUT. Rock mesh radius.")]
            public float rockRadius = 0.55f;

            [Tooltip("LAYOUT. Rock mesh height.")]
            public float rockHeight = 0.72f;

            [Tooltip("LAYOUT. Carried stone chunk size.")]
            public Vector3 stoneChunkSize = new Vector3(0.28f, 0.2f, 0.26f);

            [Tooltip("LAYOUT. Iron bar size.")]
            public Vector3 ironBarSize = new Vector3(0.42f, 0.1f, 0.16f);

            [Tooltip("LAYOUT. Assign-to-wood pad.")]
            public Vector3 woodAssignPosition = new Vector3(-9.4f, 0f, 0.4f);

            [Tooltip("LAYOUT. Assign-to-stone pad.")]
            public Vector3 stoneAssignPosition = new Vector3(-9.4f, 0f, -2.2f);

            [Tooltip("ESTIMATE. Night index when the crew assign pads appear.")]
            public int assignUnlockNight = 2;
        }

        [Serializable]
        public class HeroCombatTuning
        {
            [Tooltip("ESTIMATE. Hero hit points before armor.")]
            public float hitPoints = 140f;

            [Tooltip("ESTIMATE. Extra hit points per hide-armor level.")]
            public int armorPerLevel = 40;

            [Tooltip("ESTIMATE. Armor levels. Costs 2 hide, then 3.")]
            public int maxArmorLevel = 2;

            [Tooltip("ESTIMATE. Hide for the first armor level.")]
            public int armorHideCost = 2;

            [Tooltip("ESTIMATE. Extra hide per armor level already owned.")]
            public int armorHideStep = 1;

            [Tooltip("ESTIMATE. Backpack levels bought with hide.")]
            public int maxPackLevel = 2;

            [Tooltip("ESTIMATE. Extra wood slots per backpack level.")]
            public int packCapacity = 15;

            [Tooltip("ESTIMATE. Hide for backpack level 1, then 2.")]
            public int packHideCost = 1;

            [Tooltip("ESTIMATE. Extra hide per backpack level already owned.")]
            public int packHideStep = 1;

            [Tooltip("ESTIMATE. Hide patches after armor and the pack. Each adds a little HP.")]
            public int maxPatches = 4;

            [Tooltip("ESTIMATE. Hide per patch.")]
            public int patchHideCost = 1;

            [Tooltip("ESTIMATE. Hit points per patch.")]
            public int patchHitPoints = 12;

            [Tooltip("ESTIMATE. Seconds lying down before the hero stands at the fire.")]
            public float respawnSeconds = 2.2f;

            [Tooltip("ESTIMATE. Seconds of safety after standing back up.")]
            public float respawnGrace = 1.3f;

            [Tooltip("GDD. Seconds between cleaves. 0.5 * 20 damage = 40 DPS.")]
            public float swingInterval = 0.5f;

            [Tooltip("GDD. Damage per cleave before upgrades.")]
            public float swingDamage = 20f;

            [Tooltip("ESTIMATE. Cleave radius, meters. Reaches the wall line from the wall pad.")]
            public float cleaveRadius = 3.6f;

            [Tooltip("ESTIMATE. First melee purchase. Cheap enough for the night-1 coin haul.")]
            public int meleeCoinCost = 18;

            [Tooltip("ESTIMATE. Melee cost multiplier per level already owned.")]
            public float meleeCostMultiplier = 1.35f;

            [Tooltip("ESTIMATE. Melee levels bought with coins.")]
            public int maxMeleeLevel = 4;

            [Tooltip("ESTIMATE. Extra damage per coin melee level.")]
            public float damagePerMeleeLevel = 6f;

            [Tooltip("ESTIMATE. Bones to sharpen the swing.")]
            public int sharpenBoneCost = 4;

            [Tooltip("ESTIMATE. Extra damage per sharpen.")]
            public float sharpenDamage = 4f;

            [Tooltip("ESTIMATE. Sharpen purchases.")]
            public int maxSharpen = 4;

            [Tooltip("ESTIMATE. Damage per second a brute deals to a hero in cleave range.")]
            public float bruteContact = 5f;

            [Tooltip("ESTIMATE. Damage per second the Treant deals to a hero in cleave range.")]
            public float treantContact = 7f;
        }

        [Serializable]
        public class DropTuning
        {
            [Tooltip("GDD. Chance a shambler drops meat.")]
            public float meatChance = 0.3f;

            [Tooltip("GDD. Chance a shambler drops a bone.")]
            public float boneChance = 0.5f;

            [Tooltip("GDD. Minimum hide on a brute.")]
            public int hideMin = 2;

            [Tooltip("GDD. Maximum hide on a brute.")]
            public int hideMax = 3;

            [Tooltip("GDD. Monster cores on a brute.")]
            public int corePerBrute = 1;

            [Tooltip("GDD. Monster cores on the Treant.")]
            public int corePerTreant = 5;

            [Tooltip("ESTIMATE. Sap vials on the Treant. Level-3 turret spends all of them.")]
            public int sapPerTreant = 2;

            [Tooltip("ESTIMATE. Extra coins in the Treant fountain, on top of the kill coins.")]
            public int treantBonusCoins = 8;

            [Tooltip("ESTIMATE. Wild boars in the day forest.")]
            public int boarCount = 3;

            [Tooltip("ESTIMATE. Boar hit points. Non-hostile.")]
            public float boarHitPoints = 24f;

            [Tooltip("ESTIMATE. Meat on a boar.")]
            public int boarMeat = 1;

            [Tooltip("ESTIMATE. Seconds before a butchered boar returns.")]
            public float boarRespawnSeconds = 22f;

            [Tooltip("ESTIMATE. Boar wander speed.")]
            public float boarSpeed = 1.1f;

            [Tooltip("ESTIMATE. Hero heal from one cooked meat.")]
            public float cookHeal = 48f;

            [Tooltip("ESTIMATE. Meat spent on the dawn hearty meal.")]
            public int dawnMealMeat = 2;

            [Tooltip("GDD. Worker speed multiplier the day after a hearty meal.")]
            public float mealSpeed = 1.25f;

            [Tooltip("ESTIMATE. Bones spent on one broth.")]
            public int brothBones = 3;

            [Tooltip("ESTIMATE. Worker speed added per broth stack.")]
            public float brothSpeed = 0.05f;

            [Tooltip("ESTIMATE. Broth stacks. High enough that bones always have a sink.")]
            public int brothMax = 20;

            [Tooltip("ESTIMATE. Cores for research ranks. 2 + 4 + 2 matches brute and Treant drops.")]
            public int researchCost0 = 2;

            [Tooltip("ESTIMATE. Second research rank.")]
            public int researchCost1 = 4;

            [Tooltip("ESTIMATE. Third research rank.")]
            public int researchCost2 = 2;

            [Tooltip("ESTIMATE. How close the hero stands to cook, spend, or research.")]
            public float useRadius = 2.3f;
        }

        [Serializable]
        public class EliteTuning
        {
            [Tooltip("SPEC. Elites roll from the night after this one. Chance = perNight x (night - this).")]
            public int eliteStartNight = 5;

            [Tooltip("SPEC. Elite chance added per night past the start night.")]
            public float elitePerNight = 0.02f;

            [Tooltip("SPEC. Highest elite chance.")]
            public float eliteMax = 0.25f;

            [Tooltip("SPEC. Visual tier an elite shows at least. Champion = 3.")]
            public int eliteMinTier = 3;

            [Tooltip("SPEC. Shambler HP multiplier past the count cap that promotes to tier 2 (Ghoul).")]
            public float tierPromoteAtHpx = 2f;

            [Tooltip("SPEC. Shambler HP multiplier that promotes to tier 3 (Wight). Every x2 above base is a tier.")]
            public float championAtHpx = 4f;

            [Tooltip("SPEC. Night Threat that promotes every non-boss to at least tier 2. 0 turns it off.")]
            public float veteranAtThreat = 8000f;

            [Tooltip("ESTIMATE. Armored, Swift, Burning, Shielded, Splitter, Regen roll weights. 0 turns an affix off.")]
            public float armoredWeight = 1f;

            [Tooltip("ESTIMATE. Swift roll weight.")]
            public float swiftWeight = 1f;

            [Tooltip("ESTIMATE. Burning roll weight.")]
            public float burningWeight = 1f;

            [Tooltip("ESTIMATE. Shielded roll weight.")]
            public float shieldedWeight = 1f;

            [Tooltip("ESTIMATE. Splitter roll weight.")]
            public float splitterWeight = 1f;

            [Tooltip("ESTIMATE. Regen roll weight.")]
            public float regenWeight = 1f;

            [Tooltip("SPEC. Armored hit point multiplier.")]
            public float armoredHp = 1.5f;

            [Tooltip("SPEC. Swift move speed multiplier.")]
            public float swiftSpeed = 1.4f;

            [Tooltip("SPEC. Hits a Shielded elite blocks. Burn and spike ticks do not count.")]
            public int shieldHits = 3;

            [Tooltip("SPEC. Small shamblers a Splitter leaves on death.")]
            public int splitCount = 2;

            [Tooltip("ESTIMATE. Each split shambler's hit points, as a share of the parent's max.")]
            public float splitHpShare = 0.25f;

            [Tooltip("ESTIMATE. Live split shamblers allowed at once. The count cap also holds.")]
            public int splitMaxLive = 24;

            [Tooltip("LAYOUT. Sideways gap between split shamblers, meters.")]
            public float splitSpread = 0.6f;

            [Tooltip("ESTIMATE. Regen heal per second, as a share of max hit points.")]
            public float regenPerSecond = 0.04f;

            [Tooltip("ESTIMATE. Seconds without damage before Regen starts.")]
            public float regenCalmSeconds = 2.5f;

            [Tooltip("FEEL. Extra scale per tier above 1.")]
            public float tierScaleStep = 0.08f;

            [Tooltip("FEEL. Extra scale on an elite.")]
            public float eliteScale = 0.06f;

            [Tooltip("FEEL. Scale of a split shambler.")]
            public float splitScale = 0.7f;

            [Tooltip("FEEL. Tier 2 tint (Ghoul: pale). Multiplies the base color.")]
            public Color tier2Tint = new Color(0.86f, 0.9f, 0.82f, 1f);

            [Tooltip("FEEL. Tier 3 tint (Wight: bone grey).")]
            public Color tier3Tint = new Color(0.72f, 0.7f, 0.78f, 1f);

            [Tooltip("FEEL. Armored tint (iron bands).")]
            public Color armoredTint = new Color(0.55f, 0.6f, 0.7f, 1f);

            [Tooltip("FEEL. Swift tint (speed trails).")]
            public Color swiftTint = new Color(0.7f, 0.9f, 1f, 1f);

            [Tooltip("FEEL. Burning tint (ember aura).")]
            public Color burningTint = new Color(1f, 0.55f, 0.3f, 1f);

            [Tooltip("FEEL. Shielded tint (bubble).")]
            public Color shieldedTint = new Color(0.6f, 0.75f, 1f, 1f);

            [Tooltip("FEEL. Splitter tint (crack lines).")]
            public Color splitterTint = new Color(0.85f, 0.75f, 0.55f, 1f);

            [Tooltip("FEEL. Regen tint (green pulse).")]
            public Color regenTint = new Color(0.55f, 1f, 0.55f, 1f);
        }

        [Serializable]
        public class ProgressionTuning
        {
            [Tooltip("SPEC. Seconds of night used by the Fort Power formula.")]
            public float nightSeconds = 45f;

            [Tooltip("SPEC. Share of listed DPS that lands. Travel, overkill, and targeting.")]
            public float powerEta = 0.5f;

            [Tooltip("SPEC. Fort HP where walls stop adding much time.")]
            public float fortHpRef = 5000f;

            [Tooltip("SPEC. Map 1 night budgets. 12 / 20 / 30+1 brute / 40+2 / Treant+25.")]
            public int budget0 = 720;

            [Tooltip("SPEC. Night 2 budget.")]
            public int budget1 = 1200;

            [Tooltip("SPEC. Night 3 budget.")]
            public int budget2 = 2400;

            [Tooltip("SPEC. Night 4 budget.")]
            public int budget3 = 3600;

            [Tooltip("SPEC. Night 5 budget.")]
            public int budget4 = 4500;

            [Tooltip("SPEC. Each later map's budgets, relative to map 1.")]
            public float mapGrowth = 1.6f;

            [Tooltip("SPEC. Boss every this many nights.")]
            public int bossEvery = 5;

            [Tooltip("SPEC. Rubber-band exponent. Mild on purpose.")]
            public float rubberAlpha = 0.25f;

            [Tooltip("SPEC. Lowest threat multiplier.")]
            public float rubberMin = 0.85f;

            [Tooltip("SPEC. Highest threat multiplier.")]
            public float rubberMax = 1.2f;

            [Tooltip("SPEC. Nights 1 through this value skip the rubber band.")]
            public int rubberOffUntil = 3;

            [Tooltip("SPEC. Threat multiplier after two losses in a row.")]
            public float lossValve = 0.8f;

            [Tooltip("SPEC. Expected power is budget / this, on a normal night.")]
            public float targetNormal = 0.75f;

            [Tooltip("SPEC. Expected power divisor on a boss night.")]
            public float targetBoss = 0.95f;

            [Tooltip("SPEC. Boss takes this share of the threat budget.")]
            public float bossShare = 0.667f;

            [Tooltip("SPEC. Brute share of the remainder, before it climbs.")]
            public float bruteShareBase = 0.1f;

            [Tooltip("SPEC. Extra brute share per night after night 3.")]
            public float bruteSharePerNight = 0.03f;

            [Tooltip("SPEC. Brute share cap.")]
            public float bruteShareMax = 0.45f;

            [Tooltip("SPEC. Bodies on screen. Past this, HP and size climb.")]
            public int countCap = 150;

            [Tooltip("SPEC. Retries of one night before the run stops.")]
            public int maxTries = 6;

            [Tooltip("SPEC. Coins dropped per hit point.")]
            public float coinPerHp = 0.05f;

            [Tooltip("SPEC. Coin share kept when the fire falls.")]
            public float lossLootKeep = 0.5f;

            [Tooltip("SPEC. Hero, stack, and speed base coin cost.")]
            public float heroBase = 40f;

            [Tooltip("SPEC. Hero, stack, and speed growth. Kept inside 1.12–1.18.")]
            public float heroGrowth = 1.15f;

            [Tooltip("SPEC. Hero damage added per level, as a fraction of the base swing.")]
            public float heroDpsPerLevel = 0.1f;

            [Tooltip("SPEC. Extra damage multiplier every this many hero levels.")]
            public int heroMilestoneEvery = 10;

            [Tooltip("SPEC. Milestone multiplier.")]
            public float heroMilestoneMult = 1.5f;

            [Tooltip("SPEC. Hero levels the curve will sell.")]
            public int heroLevelCap = 40;

            [Tooltip("SPEC. Stack levels the curve will sell.")]
            public int stackLevelCap = 20;

            [Tooltip("SPEC. Speed levels the curve will sell.")]
            public int speedLevelCap = 20;

            [Tooltip("SPEC. Worker hire base cost.")]
            public float workerBase = 200f;

            [Tooltip("SPEC. Worker hire growth.")]
            public float workerGrowth = 1.5f;

            [Tooltip("SPEC. Extra planks on every worker chop, so a hire beats another hero level.")]
            public int workerBonusPlanks = 3;

            [Tooltip("SPEC. Coins for one surplus meat. About a quarter of a nominal 4.")]
            public int sellMeat = 1;

            [Tooltip("SPEC. Coins for one surplus bone.")]
            public int sellBone = 1;

            [Tooltip("SPEC. Coins for one surplus hide. About a quarter of a nominal 12.")]
            public int sellHide = 3;

            [Tooltip("SPEC. Meat kept back from the trader for meals.")]
            public int meatReserve = 4;

            [Tooltip("SPEC. Bones kept back for spikes, sharpen, and broth.")]
            public int boneReserve = 8;

            [Tooltip("SPEC. Hide kept back for armor and the pack.")]
            public int hideReserve = 2;

            [Tooltip("SPEC. Sap spent on a wick. Heals the fire and oils the turrets.")]
            public int sapWickCost = 1;

            [Tooltip("SPEC. Fire healed by one sap wick.")]
            public float sapWickHeal = 400f;

            [Tooltip("SPEC. Turret damage added per sap wick.")]
            public float sapOilBonus = 0.08f;

            [Tooltip("SPEC. Sap-oil stacks.")]
            public int sapOilMax = 5;

            [Tooltip("SPEC. Cores spent on a focus, separate from the map gate.")]
            public int coreFocusCost = 1;

            [Tooltip("SPEC. Hero damage added per core focus.")]
            public float coreFocusBonus = 0.08f;

            [Tooltip("SPEC. Core focus ranks.")]
            public int coreFocusMax = 4;

            [Tooltip("LAYOUT. Trader stall.")]
            public Vector3 traderPosition = new Vector3(-6.5f, 0f, -3.2f);

            public int MapBudget(int index)
            {
                if (index <= 0)
                    return budget0;
                if (index == 1)
                    return budget1;
                if (index == 2)
                    return budget2;
                if (index == 3)
                    return budget3;
                return budget4;
            }
        }

        [Serializable]
        public class EndlessTuning
        {
            [Tooltip("SPEC. Keep playing past wave.nightCount instead of resolving the run. Demo sessions still stop at nightCount.")]
            public bool enabled = true;

            [Tooltip("SPEC. Last authored night (maps 1–5 × 5). Later nights use the endless curve.")]
            public int authoredNights = 25;

            [Tooltip("SPEC. Endless threat growth per night past authoredNights.")]
            public float endlessGrowth = 1.1f;

            [Tooltip("SPEC. Blood Moon every this many endless nights (first one is authoredNights + this). 0 turns it off.")]
            public int bloodMoonEvery = 10;

            [Tooltip("SPEC. Blood Moon threat multiplier.")]
            public float bloodMoonMult = 1.5f;

            [Tooltip("SPEC. Blood Moon coin multiplier.")]
            public float bloodMoonLoot = 2f;

            [Tooltip("SPEC. Threat multiplier on the rest night after a Blood Moon.")]
            public float restMult = 0.7f;

            [Tooltip("SPEC. Expected power divisor on a Blood Moon.")]
            public float targetBlood = 1f;

            [Tooltip("SPEC. Expected power divisor on a rest night.")]
            public float targetRest = 0.55f;

            [Tooltip("ESTIMATE. Fort carries over between maps while only map 1 exists, so a new map's opener is at least this share of the last boss budget. Lifts nights 6–7 from ratio ~0.3 to ~0.55–0.65 (Tools/sim/threat_sim.py). 0 restores the map sawtooth for maps that rebuild the fort.")]
            public float carryFloor = 0.65f;

            [Tooltip("ESTIMATE. Carry floor growth per night inside a map.")]
            public float carryStep = 1.25f;

            [Tooltip("SPEC. First night of endless cash decay.")]
            public int decayStart = 41;

            [Tooltip("SPEC. Nights per cash-decay band.")]
            public int decaySpan = 10;

            [Tooltip("SPEC. Kill coins in the first decay band (nights 41–50).")]
            public float decayFirst = 0.5f;

            [Tooltip("SPEC. Kill coins in the second decay band (nights 51–60).")]
            public float decaySecond = 0.25f;

            [Tooltip("SPEC. Kill coins from the third band on (night 61+).")]
            public float decayLast = 0.1f;
        }

        [Serializable]
        public class PrestigeTuning
        {
            [Tooltip("SPEC. Embers_total = floor(emberK × sqrt(lifetime threat cleared / emberDiv)).")]
            public float emberK = 10f;

            [Tooltip("SPEC. Lifetime threat divisor in the Ember formula.")]
            public float emberDiv = 10000f;

            [Tooltip("SPEC. DPS bonus per claimed Ember.")]
            public float emberDps = 0.03f;

            [Tooltip("SPEC. Income bonus per claimed Ember.")]
            public float emberIncome = 0.03f;

            [Tooltip("SPEC. Same-family soft cap: eff = raw / (1 + raw / cap). 3 = 300%. 0 turns it off.")]
            public float softCap = 3f;

            [Tooltip("SPEC. Losses on one night before Rebuild is pushed.")]
            public int offerLosses = 3;

            [Tooltip("ESTIMATE. Rebuild on demand only from this night, so the FTUE never shows it.")]
            public int demandNight = 10;

            [Tooltip("LAYOUT. Rebuild button caption; the Ember gain follows it.")]
            public string buttonLabel = "REBUILD +";
        }
    }
}
