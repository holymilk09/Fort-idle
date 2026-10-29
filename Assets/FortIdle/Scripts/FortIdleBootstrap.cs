using UnityEngine;
using UnityEngine.Rendering;

namespace FortIdle
{
    public class FortIdleBootstrap : MonoBehaviour
    {
        public FortIdleConfig config;
        public Material litMaterial;
        public Material emissionMaterial;

        void Awake()
        {
            FortWorld.Reset();
            if (config == null)
            {
                Debug.LogError("FORTIDLE missing config asset");
                return;
            }

            FortWorld.Config = config;
            if (config.stages == null)
                config.stages = new FortIdleConfig.StageTuning();
            config.stages.Ensure();
            if (config.quality == null)
                config.quality = new FortIdleConfig.QualityTuning();
            if (config.layout.treeTiltDegrees <= 0f)
                config.layout.treeTiltDegrees = 2.5f;
            if (config.pour.labelFadeNear <= 0f)
                config.pour.labelFadeNear = 7.5f;
            if (config.pour.labelFadeFar <= config.pour.labelFadeNear)
                config.pour.labelFadeFar = 14f;
            Application.targetFrameRate = config.presentation.targetFrameRate;
            Application.runInBackground = config.presentation.runInBackground;
            QualitySettings.vSyncCount = 0;

            FortWorld.Look = BuildPalette();
            if (ElementSheet.Requested())
            {
                Light sheetSun = BuildSun();
                Camera sheetCamera = BuildCamera();
                FortLook.Boot(sheetCamera, sheetSun);
                ElementSheet.Show(sheetSun, sheetCamera);
                Debug.Log("FORTIDLE elements=ready");
                return;
            }

            FortProgress progress = gameObject.AddComponent<FortProgress>();
            FortWorld.Progress = progress;
            if (config.demo.runByDefault || HasLaunchFlag("-demo") || HasLaunchFlag("-stages") || HasLaunchFlag("-perf"))
                Random.InitState(1401);
            FortSave.LoadIntoWorld();
            Light sun = BuildSun();
            Camera camera = BuildCamera();
            FortLook.Boot(camera, sun);
            BuildGround();
            BuildFire();
            GameHud hud = gameObject.AddComponent<GameHud>();
            hud.Build();
            Transform hero = BuildHero(hud.Joystick);
            FollowCamera follow = camera.GetComponent<FollowCamera>();
            follow.SetTarget(hero);
            BuildTrees();
            BuildPads();
            TreeCheck.Report();
            if (FortWorld.StaticRoot != null)
                StaticBatchingUtility.Combine(FortWorld.StaticRoot.gameObject);
            SpawnBoars();
            SpawnTrader();
            FortSave.ApplyStructures();
            MatchDirector match = gameObject.AddComponent<MatchDirector>();
            match.Init(sun, camera);
            FortWorld.Match = match;
            gameObject.AddComponent<OfflineEarnings>();
            if (FrameMeter.Wanted())
                gameObject.AddComponent<FrameMeter>();
            if (StageShowcase.Requested())
            {
                StageShowcase showcase = gameObject.AddComponent<StageShowcase>();
                showcase.Init(follow);
            }
            else if (!PerfHarness.Requested() && !LookReel.Requested() && !TreeShots.Requested() && !QaShot.Requested() && !PoseShot.Requested())
            {
                DemoDriver demo = gameObject.AddComponent<DemoDriver>();
                demo.Init(hud.Joystick, hero);
            }

            if (PerfHarness.Requested())
                gameObject.AddComponent<PerfHarness>();
            if (LookReel.Requested())
                gameObject.AddComponent<LookReel>().Init(follow);
            if (TreeShots.Requested())
                gameObject.AddComponent<TreeShots>().Init(follow);
            if (QaShot.Requested())
                gameObject.AddComponent<QaShot>().Init(follow);
            if (PoseCheck.Requested())
                gameObject.AddComponent<PoseCheck>();
            if (PoseShot.Requested())
                gameObject.AddComponent<PoseShot>();

            bool stageMath = StagedBuild.Reached(20, 100, 5) == 1
                && StagedBuild.Reached(19, 100, 5) == 0
                && StagedBuild.Reached(40, 40, 5) == 5;
            Debug.Log(stageMath ? "FORTIDLE stage-math ok" : "FORTIDLE stage-math fail");
            Debug.Log("FORTIDLE ready"
                + " speed=" + config.player.moveSpeed
                + " chop=" + config.chop.interval
                + " planksPerHit=" + config.chop.planksPerHit
                + " hitsPerTree=" + config.chop.hitsToFellTree
                + " stack=" + config.stack.capacity
                + " pour=" + config.pour.planksPerSecond
                + " pourBoost=" + config.pour.boostedPlanksPerSecond
                + " wallCost=" + config.structures.wallCost
                + " wallHp=" + config.structures.wallHitPoints
                + " turretCost=" + config.structures.turretCost
                + " turretDmg=" + config.turret.damage
                + " turretRate=" + config.turret.shotsPerSecond
                + " turretRange=" + config.turret.range
                + " monsters=" + config.wave.monsterCount
                + " monsterHp=" + config.monsters.hitPoints
                + " monsterSpeed=" + config.monsters.moveSpeed
                + " day=" + config.wave.dayDuration
                + " laterDay=" + config.wave.laterDayDuration
                + " night=" + config.wave.nightDuration
                + " nights=" + config.wave.monsterCount
                + "/" + config.wave.night2Shamblers
                + "/" + config.wave.night3Shamblers + "+" + config.wave.night3Brutes + "b"
                + "/" + config.wave.night4Shamblers + "+" + config.wave.night4Brutes + "b"
                + "/" + config.wave.night5Shamblers + "+" + config.wave.night5Treants + "t"
                + " bruteHp=" + config.monsters.bruteHitPoints
                + " treantHp=" + config.monsters.treantHitPoints
                + " coinsPerKill=" + config.loot.coinsPerKill
                + " bagCost=" + config.upgrades.stackBaseCost
                + " bagCurve=" + config.upgrades.stackCostMultiplier
                + " speedCost=" + config.upgrades.speedBaseCost
                + " hire=" + FortProgress.ScaledCost(config.upgrades.hireBaseCost, config.upgrades.hireCostMultiplier, 0)
                + "," + FortProgress.ScaledCost(config.upgrades.hireBaseCost, config.upgrades.hireCostMultiplier, 1)
                + "," + FortProgress.ScaledCost(config.upgrades.hireBaseCost, config.upgrades.hireCostMultiplier, 2)
                + "," + FortProgress.ScaledCost(config.upgrades.hireBaseCost, config.upgrades.hireCostMultiplier, 3)
                + " hutCost=" + config.structures.hutCost
                + " workerSpeed=" + config.workers.moveSpeed
                + " workerCarry=" + config.workers.carryCapacity);
        }

        Palette BuildPalette()
        {
            Palette palette = new Palette();
            palette.Grass = Make(config.presentation.grass, false, 0f);
            palette.Dirt = Make(config.presentation.dirt, false, 0f);
            palette.HeroBody = Make(config.presentation.heroBody, false, 0f);
            palette.HeroHead = Make(config.presentation.heroHead, false, 0f);
            palette.Axe = Make(config.presentation.axe, false, 0f);
            palette.Plank = Make(config.presentation.plank, false, 0f);
            palette.Trunk = Make(config.presentation.trunk, false, 0f);
            palette.Canopy = Make(config.presentation.canopy, false, 0f);
            palette.Stump = Make(config.presentation.stump, false, 0f);
            palette.Wall = Make(config.presentation.wall, false, 0f);
            palette.WallReinforced = Make(config.presentation.wallReinforced, false, 0f);
            palette.WallStone = Make(config.presentation.wallStone, false, 0f);
            palette.Rock = Make(config.presentation.rock, false, 0f);
            palette.Stone = Make(config.presentation.stoneChunk, false, 0f);
            palette.Iron = Make(config.presentation.iron, false, 0f);
            palette.Spike = Make(config.presentation.spike, false, 0f);
            palette.Turret = Make(config.presentation.turret, false, 0f);
            palette.TurretAccent = Make(config.presentation.turretAccent, false, 0f);
            palette.Monster = Make(config.presentation.monster, false, 0f);
            palette.Eye = Make(config.presentation.monsterEye, true, config.presentation.eyeEmission);
            palette.Brute = Make(config.presentation.brute, false, 0f);
            palette.BruteHorn = Make(config.presentation.bruteHorn, false, 0f);
            palette.Fire = Make(config.presentation.fire, true, config.fire.emission);
            palette.FireLog = Make(config.presentation.fireLog, false, 0f);
            palette.Bolt = Make(config.presentation.bolt, true, config.presentation.boltEmission);
            palette.Coin = Make(config.presentation.coin, true, config.presentation.coinEmission);
            palette.HutBody = Make(config.presentation.hutBody, false, 0f);
            palette.HutRoof = Make(config.presentation.hutRoof, false, 0f);
            palette.HutDoor = Make(config.presentation.hutDoor, false, 0f);
            palette.WorkerBody = Make(config.presentation.workerBody, false, 0f);
            palette.WorkerHead = Make(config.presentation.workerHead, false, 0f);
            palette.LockedGate = Make(config.presentation.lockedGate, false, 0f);
            palette.LockedPost = Make(config.presentation.lockedPost, false, 0f);
            palette.Meat = Make(config.presentation.meat, false, 0f);
            palette.Bone = Make(config.presentation.bone, false, 0f);
            palette.Hide = Make(config.presentation.hide, false, 0f);
            palette.Core = Make(config.presentation.core, true, 2.4f);
            palette.Sap = Make(config.presentation.sap, true, 1.4f);
            palette.Boar = Make(config.presentation.boar, false, 0f);
            palette.Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (palette.Font == null)
                palette.Font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (palette.Font == null)
                Debug.LogError("FORTIDLE font missing");
            palette.Circle = CircleSprite(config.joystick.spriteResolution);
            return palette;
        }

        Material Make(Color color, bool emission, float emissionStrength)
        {
            Material source = emission && emissionMaterial != null ? emissionMaterial : litMaterial;
            if (source == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                source = shader != null ? new Material(shader) : new Material(Shader.Find("Standard"));
            }

            Material material = new Material(source);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", config.presentation.materialSmoothness);
            material.SetFloat("_Metallic", config.presentation.materialMetallic);
            if (emission)
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                material.SetColor("_EmissionColor", color * emissionStrength);
            }

            return material;
        }

        static Sprite CircleSprite(int resolution)
        {
            if (resolution < 2)
                resolution = 2;
            Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            float radius = (resolution - 1) * 0.5f;
            Vector2 center = new Vector2(radius, radius);
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = 0f;
                    if (distance <= radius - 0.5f)
                        alpha = 1f;
                    else if (distance <= radius + 0.5f)
                        alpha = radius + 0.5f - distance;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
        }

        Light BuildSun()
        {
            GameObject sunObject = new GameObject("Sun");
            Light light = sunObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = FortLook.SoftShadows ? LightShadows.Soft : LightShadows.None;
            light.shadowStrength = 0.55f;
            return light;
        }

        Camera BuildCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            FollowCamera follow = cameraObject.AddComponent<FollowCamera>();
            follow.Configure(camera);
            FortWorld.Camera = camera;
            FortWorld.Rig = follow;
            return camera;
        }

        void BuildGround()
        {
            GameObject batch = new GameObject("StaticWorld");
            FortWorld.StaticRoot = batch.transform;
            GameObject ground = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Grass, batch.transform, true);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(config.layout.groundExtent, config.layout.groundThickness, config.layout.groundExtent);
            ground.transform.position = new Vector3(0f, -config.layout.groundThickness * 0.5f, 0f);
            Renderer groundRenderer = ground.GetComponent<Renderer>();
            if (groundRenderer != null)
                groundRenderer.enabled = false;
            FortGround.Build(config);
            if (FortWorld.GroundRoot != null)
                FortWorld.GroundRoot.SetParent(batch.transform, true);
            BuildForestEdge();
        }

        void BuildForestEdge()
        {
            float front = config.layout.frontTreeZ + 1.6f;
            int count = config.layout.frontTreeCount;
            for (int i = 0; i < count; i++)
            {
                float x = (i - (count - 1) * 0.5f) * config.layout.frontTreeSpacing;
                string id = i % 2 == 0 ? "GrassTuft" : "Flowers";
                PlaceEdge(id, new Vector3(x, 0f, front), i % 2 == 0 ? 0.5f : 0.4f);
            }

            for (int i = 0; i < config.layout.sideTreeCountPerSide; i++)
            {
                float z = config.layout.sideTreeZStart + i * config.layout.sideTreeZSpacing;
                string id = i % 2 == 0 ? "Bush" : "GrassTuft";
                float height = i % 2 == 0 ? 0.85f : 0.45f;
                float x = config.layout.sideTreeX + 1.8f;
                PlaceEdge(id, new Vector3(x, 0f, z), height);
                PlaceEdge(id, new Vector3(-x, 0f, z), height);
            }

            float rim = config.layout.clearingDiameter * 0.42f;
            PlaceEdge("Bush", new Vector3(-rim, 0f, -rim * 0.35f), 0.7f);
            PlaceEdge("Flowers", new Vector3(rim, 0f, -rim * 0.2f), 0.4f);
            PlaceEdge("GrassTuft", new Vector3(rim * 0.55f, 0f, -rim), 0.5f);
        }

        void PlaceEdge(string id, Vector3 position, float height)
        {
            GameObject root = new GameObject(id);
            if (FortWorld.StaticRoot != null)
                root.transform.SetParent(FortWorld.StaticRoot, false);
            root.transform.position = position;
            GameObject prop = FortArt.Stand(root.transform, id, height);
            if (prop == null)
            {
                Destroy(root);
                return;
            }

            prop.name = id;
            PoseTree(root.transform);
            FortWorld.Plants.Add(root.transform);
        }

        void BuildFire()
        {
            GameObject root = new GameObject("Fire");
            root.transform.position = config.fire.position;
            Light glow = root.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = config.presentation.fire;
            glow.intensity = 3.2f;
            glow.range = 8f;
            glow.shadows = LightShadows.None;
            glow.transform.localPosition = new Vector3(0f, config.fire.flameHeight, 0f);

            Transform flame;
            StagedBuild staged = StageKit.Fire(root.transform, out flame);
            FireCore fire = root.AddComponent<FireCore>();
            fire.Init(flame, config.fire.hitPoints);
            fire.BindStages(staged);
            FortWorld.Fire = fire;
        }

        Transform BuildHero(VirtualJoystick joystick)
        {
            GameObject hero = new GameObject("Hero");
            hero.transform.position = config.player.spawnPosition;
            CharacterController body = hero.AddComponent<CharacterController>();
            body.radius = config.player.radius;
            body.height = config.player.height;
            body.center = new Vector3(0f, config.player.height * 0.5f, 0f);
            body.skinWidth = config.player.skinWidth;
            body.stepOffset = config.player.stepOffset;
            body.minMoveDistance = config.player.minMoveDistance;
            body.slopeLimit = config.player.slopeLimit;

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(hero.transform, false);
            visual.transform.localRotation = Quaternion.Euler(0f, config.player.spawnYawDegrees, 0f);

            float heroHeight = config.player.headHeight + config.player.headDiameter * 0.5f;
            Transform axe = null;
            if (FortArt.Stand(visual.transform, "Hero", heroHeight) == null)
            {
                GameObject torso = Prop.Primitive(PrimitiveType.Capsule, FortWorld.Look.HeroBody, visual.transform, false);
                torso.name = "Body";
                torso.transform.localScale = Prop.CapsuleScale(config.player.bodyRadius, config.player.bodyHeight);
                torso.transform.localPosition = new Vector3(0f, config.player.bodyHeight * 0.5f, 0f);

                GameObject head = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.HeroHead, visual.transform, false);
                head.name = "Head";
                head.transform.localScale = Prop.SphereScale(config.player.headDiameter);
                head.transform.localPosition = new Vector3(0f, config.player.headHeight, 0f);

                GameObject axeObject = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Axe, visual.transform, false);
                axeObject.name = "Axe";
                axeObject.transform.localScale = config.player.axeSize;
                axeObject.transform.localPosition = config.player.axeLocalPosition;
                axe = axeObject.transform;
            }

            GameObject stackObject = new GameObject("Stack");
            stackObject.transform.SetParent(visual.transform, false);
            PlankStack stack = stackObject.AddComponent<PlankStack>();
            stack.Init(config, FortWorld.Look.Plank);

            HeroController controller = hero.AddComponent<HeroController>();
            controller.Init(body, visual.transform, axe, stack, joystick);
            FortWorld.Hero = hero.transform;
            FortWorld.Stack = stack;
            return hero.transform;
        }

        void SpawnBoars()
        {
            int count = config.drops.boarCount;
            for (int i = 0; i < count; i++)
            {
                float side = i % 2 == 0 ? 1f : -1f;
                Vector3 position = new Vector3(side * (7f + i * 1.4f), 0f, 11.5f + i * 1.6f);
                GameObject root = new GameObject("Boar");
                Boar boar = root.AddComponent<Boar>();
                boar.Init(position);
                FortWorld.Boars.Add(boar);
            }
        }

        void SpawnTrader()
        {
            GameObject root = new GameObject("Trader");
            TraderStall stall = root.AddComponent<TraderStall>();
            stall.Init(config.progression.traderPosition);
            FortWorld.Trader = root.transform;
        }

        static bool HasLaunchFlag(string flag)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == flag)
                    return true;
            }

            return false;
        }

        void BuildTrees()
        {
            int front = config.layout.frontTreeCount;
            for (int i = 0; i < front; i++)
            {
                float x = (i - (front - 1) * 0.5f) * config.layout.frontTreeSpacing;
                SpawnTree(new Vector3(x, 0f, config.layout.frontTreeZ));
            }

            for (int i = 0; i < config.layout.sideTreeCountPerSide; i++)
            {
                float z = config.layout.sideTreeZStart + i * config.layout.sideTreeZSpacing;
                SpawnTree(new Vector3(config.layout.sideTreeX, 0f, z));
                SpawnTree(new Vector3(-config.layout.sideTreeX, 0f, z));
            }
        }

        void SpawnTree(Vector3 position)
        {
            GameObject root = new GameObject("Tree");
            root.transform.position = position;

            GameObject trunk = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Trunk, root.transform, true);
            trunk.name = "Trunk";
            trunk.transform.localScale = Prop.CylinderScale(config.layout.trunkRadius * 2f, config.layout.trunkHeight);
            trunk.transform.localPosition = new Vector3(0f, config.layout.trunkHeight * 0.5f, 0f);
            Renderer trunkRenderer = trunk.GetComponent<Renderer>();
            if (trunkRenderer != null)
                trunkRenderer.enabled = false;

            float treeHeight = config.layout.canopyHeight + config.layout.canopyDiameter * 0.5f;
            GameObject canopy = FortArt.Stand(root.transform, FortArt.TreeId(Mathf.RoundToInt(position.x * 10f + position.z)), treeHeight);
            if (canopy == null)
            {
                canopy = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.Canopy, root.transform, false);
                canopy.transform.localScale = Prop.SphereScale(config.layout.canopyDiameter);
                canopy.transform.localPosition = new Vector3(0f, config.layout.canopyHeight, 0f);
                if (trunkRenderer != null)
                    trunkRenderer.enabled = true;
            }

            canopy.name = "Canopy";
            FortArt.CastShadows(canopy, false);
            GameObject stump = FortArt.Stand(root.transform, "Stump", config.layout.stumpHeight);
            if (stump == null)
            {
                stump = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Stump, root.transform, false);
                stump.transform.localScale = Prop.CylinderScale(config.layout.stumpRadius * 2f, config.layout.stumpHeight);
                stump.transform.localPosition = new Vector3(0f, config.layout.stumpHeight * 0.5f, 0f);
            }

            stump.name = "Stump";
            PoseTree(root.transform);
            FortWorld.Plants.Add(root.transform);
            if (!loggedTreeUp && canopy != null)
            {
                loggedTreeUp = true;
                Renderer treeRenderer = canopy.GetComponentInChildren<Renderer>();
                Bounds treeBounds = treeRenderer != null ? treeRenderer.bounds : new Bounds();
                Debug.Log("FORTIDLE tree-up height=" + treeBounds.size.y.ToString("0.00")
                    + " wide=" + Mathf.Max(treeBounds.size.x, treeBounds.size.z).ToString("0.00"));
            }

            TreeNode node = root.AddComponent<TreeNode>();
            node.Init(trunk.transform, canopy.transform, stump.transform, trunk.GetComponent<Collider>(), config.chop.hitsToFellTree);
            FortWorld.Trees.Add(node);
        }

        void BuildPads()
        {
            FortWorld.Wall = SpawnPad(
                BuildPad.Kind.Wall,
                config.layout.wallPadPosition,
                config.structures.wallCost,
                config.structures.wallHitPoints,
                config.presentation.padWall,
                true);
            FortWorld.TurretPad = SpawnPad(
                BuildPad.Kind.Turret,
                config.layout.turretPadPosition,
                config.structures.turretCost,
                config.defenses.turretHitPoints,
                config.presentation.padTurret,
                false);
            FortWorld.Hut = SpawnHut();
            FortWorld.StackPad = SpawnCoinPad(CoinPad.Kind.Stack, config.layout.stackPadPosition, config.presentation.padBag);
            FortWorld.SpeedPad = SpawnCoinPad(CoinPad.Kind.Speed, config.layout.speedPadPosition, config.presentation.padSpeed);
            FortWorld.HirePad = SpawnCoinPad(CoinPad.Kind.Hire, config.layout.hirePadPosition, config.presentation.padHire);
            FortWorld.MeleePad = SpawnCoinPad(CoinPad.Kind.Melee, config.layout.meleePadPosition, config.presentation.padUpgrade);
            FortWorld.Spike = SpawnSpike();
            FortWorld.Gate = SpawnGate();
            FortWorld.Tower = SpawnTower();
            FortWorld.TurretUpgrade = SpawnTurretUpgrade();
            SpawnRocks();
            FortWorld.StoneAssign = SpawnAssign(WorkerAgent.Duty.Stone, config.supplies.stoneAssignPosition, config.presentation.padBag);
            SpawnAssign(WorkerAgent.Duty.Wood, config.supplies.woodAssignPosition, config.presentation.padWall);
            BuildLockedArea();
        }

        BuildPad SpawnSpike()
        {
            FortIdleConfig.DefenseTuning defenses = config.defenses;
            GameObject root = new GameObject("SpikePad");
            root.transform.position = defenses.spikePosition;
            GameObject disc = Disc(root.transform, config.presentation.padSpike);
            WorldLabel label = PadLabel(root.transform, 0f, 0.35f);
            GameObject spikes = new GameObject("Spikes");
            spikes.transform.SetParent(root.transform, false);
            StagedBuild staged = StageKit.Spike(spikes.transform);
            BuildPad pad = root.AddComponent<BuildPad>();
            pad.Init(BuildPad.Kind.Spike, defenses.spikeWood, 0f, spikes.transform, null, disc.GetComponent<Renderer>(), null, label, null, config.presentation.padSpike);
            pad.BindStages(staged);
            pad.SetUnlockNight(defenses.spikeUnlockNight);
            FortWorld.Pads.Add(pad);
            return pad;
        }

        BuildPad SpawnGate()
        {
            FortIdleConfig.DefenseTuning defenses = config.defenses;
            GameObject root = new GameObject("GatePad");
            root.transform.position = defenses.gatePosition;
            GameObject disc = Disc(root.transform, config.presentation.padGate);
            WorldLabel label = PadLabel(root.transform, 3.5f, 0.2f);
            GameObject pivot = new GameObject("Gate");
            pivot.transform.SetParent(root.transform, false);
            pivot.transform.localPosition = defenses.gateLocalOffset;
            GameObject mesh = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.LockedGate, pivot.transform, true);
            mesh.name = "GateMesh";
            mesh.transform.localScale = defenses.gateSize;
            mesh.transform.localPosition = new Vector3(0f, defenses.gateSize.y * 0.5f, 0f);
            mesh.GetComponent<Renderer>().enabled = false;
            BuildPad pad = root.AddComponent<BuildPad>();
            pad.Init(BuildPad.Kind.Gate, defenses.gateWood, defenses.gateHitPoints, pivot.transform, mesh.GetComponent<Collider>(), disc.GetComponent<Renderer>(), null, label, null, config.presentation.padGate);
            pad.BindMesh(mesh.transform, mesh.GetComponent<Renderer>());
            pad.BindStages(StageKit.Gate(pivot.transform));
            pad.SetUnlockNight(defenses.gateUnlockNight);
            FortWorld.Pads.Add(pad);
            return pad;
        }

        BuildPad SpawnTower()
        {
            FortIdleConfig.DefenseTuning defenses = config.defenses;
            GameObject root = new GameObject("TowerPad");
            root.transform.position = defenses.towerPosition;
            GameObject disc = Disc(root.transform, config.presentation.padTower);
            WorldLabel label = PadLabel(root.transform, 2.6f, 0.35f);
            GameObject pivot = new GameObject("Watchtower");
            pivot.transform.SetParent(root.transform, false);
            Transform aim;
            StagedBuild staged = StageKit.Tower(pivot.transform, out aim);
            Turret weapon = pivot.AddComponent<Turret>();
            weapon.Init(aim, defenses.towerRange, defenses.towerShotsPerSecond, defenses.towerDamage);
            BuildPad pad = root.AddComponent<BuildPad>();
            pad.Init(BuildPad.Kind.Watchtower, defenses.towerWood + defenses.towerStone, defenses.towerHitPoints, pivot.transform, null, disc.GetComponent<Renderer>(), null, label, weapon, config.presentation.padTower);
            pad.BindStages(staged);
            pad.SetRecipe(defenses.towerWood, defenses.towerStone, 0);
            pad.SetUnlockNight(defenses.towerUnlockNight);
            FortWorld.Pads.Add(pad);
            return pad;
        }

        BuildPad SpawnTurretUpgrade()
        {
            FortIdleConfig.DefenseTuning defenses = config.defenses;
            GameObject root = new GameObject("TurretUpgradePad");
            root.transform.position = defenses.turretUpgradePosition;
            GameObject disc = Disc(root.transform, config.presentation.padUpgrade);
            WorldLabel label = PadLabel(root.transform, 1.8f, 0.9f);
            BuildPad pad = root.AddComponent<BuildPad>();
            pad.Init(BuildPad.Kind.TurretUpgrade, defenses.turretUpgradeWood, 0f, null, null, disc.GetComponent<Renderer>(), null, label, null, config.presentation.padUpgrade);
            if (FortWorld.TurretPad != null)
                pad.BindUpgrade(FortWorld.TurretPad.Stages);
            pad.SetRecipe(defenses.turretUpgradeWood, 0, defenses.turretUpgradeIron);
            pad.SetUnlockNight(defenses.turretUpgradeUnlockNight);
            FortWorld.Pads.Add(pad);
            return pad;
        }

        void SpawnRocks()
        {
            FortIdleConfig.SupplyTuning supplies = config.supplies;
            for (int i = 0; i < supplies.rockCount; i++)
            {
                GameObject root = new GameObject("Rock");
                root.transform.position = new Vector3(supplies.rockX, 0f, supplies.rockZStart + i * supplies.rockZSpacing);
                GameObject body = FortArt.Stand(root.transform, i % 2 == 0 ? "Rock" : "RockMoss", supplies.rockHeight);
                if (body == null)
                {
                    body = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Rock, root.transform, false);
                    body.transform.localScale = new Vector3(supplies.rockRadius * 2f, supplies.rockHeight, supplies.rockRadius * 1.6f);
                    body.transform.localPosition = new Vector3(0f, supplies.rockHeight * 0.5f, 0f);
                }

                body.name = "Body";
                body.transform.localRotation = Quaternion.Euler(0f, 18f * i, 8f);
                RockNode rock = root.AddComponent<RockNode>();
                rock.Init(body.transform, supplies.hitsToBreakRock);
                FortWorld.Rocks.Add(rock);
            }
        }

        AssignPad SpawnAssign(WorkerAgent.Duty duty, Vector3 position, Color color)
        {
            GameObject root = new GameObject(duty + "Assign");
            root.transform.position = position;
            GameObject disc = Disc(root.transform, color);
            float assignX = duty == WorkerAgent.Duty.Stone ? 2.2f : -2.2f;
            WorldLabel label = PadLabel(root.transform, assignX, 0.2f);
            AssignPad pad = root.AddComponent<AssignPad>();
            pad.Init(duty, disc.GetComponent<Renderer>(), label, color);
            return pad;
        }

        GameObject Disc(Transform parent, Color color)
        {
            GameObject disc = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Dirt, parent, false);
            disc.name = "Disc";
            disc.transform.localScale = Prop.CylinderScale(config.pour.padDiscDiameter, config.pour.padDiscHeight);
            disc.transform.localPosition = new Vector3(0f, config.pour.padDiscHeight * 0.5f, 0f);
            disc.GetComponent<Renderer>().material.SetColor("_BaseColor", color);
            return disc;
        }

        WorldLabel PadLabel(Transform parent, float x, float extraY)
        {
            return WorldLabel.Create(
                parent,
                new Vector3(x, config.pour.labelHeight + extraY, 0f),
                "",
                config.hud.worldLabelFontSize,
                config.hud.worldCanvasScale,
                config.hud.worldLabelSize);
        }

        BuildPad SpawnHut()
        {
            GameObject root = new GameObject("HutPad");
            root.transform.position = new Vector3(config.layout.hutPadPosition.x, 0f, config.layout.hutPadPosition.z);

            GameObject disc = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Dirt, root.transform, false);
            disc.name = "Disc";
            disc.transform.localScale = Prop.CylinderScale(config.pour.padDiscDiameter, config.pour.padDiscHeight);
            disc.transform.localPosition = new Vector3(0f, config.pour.padDiscHeight * 0.5f, 0f);

            GameObject bar = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.HutBody, root.transform, false);
            bar.name = "Fill";
            float side = config.pour.padDiscDiameter * 0.5f + config.presentation.fillBarSize.x;
            bar.transform.localPosition = new Vector3(side, 0f, 0f);
            bar.transform.localScale = config.presentation.fillBarSize;

            WorldLabel label = PadLabel(root.transform, -3.2f, 0.85f);

            GameObject pivotObject = new GameObject("Hut");
            pivotObject.transform.SetParent(root.transform, false);
            pivotObject.transform.localPosition = config.layout.hutLocalOffset;

            GameObject body = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.HutBody, pivotObject.transform, false);
            body.name = "Body";
            body.transform.localScale = config.structures.hutBodySize;
            body.transform.localPosition = new Vector3(0f, config.structures.hutBodySize.y * 0.5f, 0f);

            GameObject roof = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.HutRoof, pivotObject.transform, false);
            roof.name = "Roof";
            roof.transform.localScale = config.structures.hutRoofSize;
            roof.transform.localPosition = new Vector3(0f, config.structures.hutBodySize.y + config.structures.hutRoofSize.y * 0.5f, 0f);

            GameObject door = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.HutDoor, pivotObject.transform, false);
            door.name = "Door";
            door.transform.localScale = config.structures.hutDoorSize;
            door.transform.localPosition = new Vector3(
                0f,
                config.structures.hutDoorSize.y * 0.5f,
                config.structures.hutBodySize.z * 0.5f);

            BuildPad pad = root.AddComponent<BuildPad>();
            pad.Init(BuildPad.Kind.Hut, config.structures.hutCost, 0f, pivotObject.transform, null, disc.GetComponent<Renderer>(), bar.transform, label, null, config.presentation.padHut);
            pad.BindStages(StageKit.Hut(pivotObject.transform, body.transform, roof.transform, door.transform));
            FortWorld.Pads.Add(pad);

            GameObject stockObject = new GameObject("Stockpile");
            stockObject.transform.SetParent(root.transform, false);
            stockObject.transform.localPosition = config.layout.stockpileOffset;
            WorldLabel stockLabel = WorldLabel.Create(
                stockObject.transform,
                new Vector3(0f, config.pour.labelHeight, 0f),
                "",
                config.hud.worldLabelFontSize,
                config.hud.worldCanvasScale,
                config.hud.worldLabelSize);
            Stockpile stock = stockObject.AddComponent<Stockpile>();
            stock.Init(stockLabel);
            FortWorld.Stock = stock;
            return pad;
        }

        CoinPad SpawnCoinPad(CoinPad.Kind kind, Vector3 position, Color color)
        {
            GameObject root = new GameObject(kind + "Pad");
            root.transform.position = new Vector3(position.x, 0f, position.z);
            GameObject disc = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Dirt, root.transform, false);
            disc.name = "Disc";
            disc.transform.localScale = Prop.CylinderScale(config.pour.padDiscDiameter, config.pour.padDiscHeight);
            disc.transform.localPosition = new Vector3(0f, config.pour.padDiscHeight * 0.5f, 0f);
            float coinX = kind == CoinPad.Kind.Stack ? -1.4f : kind == CoinPad.Kind.Speed ? 1.6f : kind == CoinPad.Kind.Hire ? 0f : 0.8f;
            float coinY = kind == CoinPad.Kind.Melee ? -0.45f : kind == CoinPad.Kind.Hire ? 1.3f : 0.35f;
            WorldLabel label = PadLabel(root.transform, coinX, coinY);
            CoinPad pad = root.AddComponent<CoinPad>();
            pad.Init(kind, disc.GetComponent<Renderer>(), label, color);
            return pad;
        }

        void BuildLockedArea()
        {
            GameObject root = new GameObject("LockedGate");
            if (FortWorld.StaticRoot != null)
                root.transform.SetParent(FortWorld.StaticRoot, false);
            root.transform.position = config.layout.lockedAreaPosition;

            GameObject gate = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.LockedGate, root.transform, false);
            gate.name = "Gate";
            gate.transform.localScale = config.layout.lockedGateSize;
            gate.transform.localPosition = new Vector3(0f, config.layout.lockedGateSize.y * 0.5f, 0f);
            if (FortArt.Fill(gate.transform, "Gate", 1, true))
                gate.GetComponent<Renderer>().enabled = false;

            PlacePost(root.transform, -config.layout.lockedPostSpread);
            PlacePost(root.transform, config.layout.lockedPostSpread);

            WorldLabel label = WorldLabel.Create(
                root.transform,
                new Vector3(0f, config.layout.lockedLabelHeight, 0f),
                config.copy.lockedLabel,
                config.hud.worldLabelFontSize,
                config.hud.worldCanvasScale,
                config.hud.worldLabelSize);
            label.SetColor(config.presentation.warningText);
            MapGate research = root.AddComponent<MapGate>();
            research.Init(label);
            BuildLockedForest();
        }

        void BuildLockedForest()
        {
            Vector3 gate = config.layout.lockedAreaPosition;
            for (int i = -3; i <= 3; i++)
            {
                if (i == 0)
                    continue;
                SpawnBackdrop(new Vector3(gate.x + i * 3.2f, 0f, gate.z + 3.8f));
            }

            SpawnBackdrop(new Vector3(gate.x - 6.2f, 0f, gate.z + 1.4f));
            SpawnBackdrop(new Vector3(gate.x + 6.2f, 0f, gate.z + 1.4f));
        }

        void SpawnBackdrop(Vector3 position)
        {
            GameObject root = new GameObject("BackdropTree");
            if (FortWorld.StaticRoot != null)
                root.transform.SetParent(FortWorld.StaticRoot, false);
            root.transform.position = position;
            float treeHeight = config.layout.canopyHeight + config.layout.canopyDiameter * 0.5f;
            GameObject canopy = FortArt.Stand(root.transform, FortArt.TreeId(Mathf.RoundToInt(position.x * 4f + position.z)), treeHeight);
            if (canopy == null)
            {
                Destroy(root);
                return;
            }

            canopy.name = "Canopy";
            FortArt.CastShadows(canopy, false);
            PoseTree(root.transform);
            FortWorld.Plants.Add(root.transform);
        }

        void PoseTree(Transform root)
        {
            float tiltMax = config.layout.treeTiltDegrees;
            if (tiltMax > 3f)
                tiltMax = 3f;
            if (tiltMax < 0f)
                tiltMax = 0f;
            float yaw = Random.Range(0f, 360f);
            float tilt = Random.Range(0f, tiltMax);
            float heading = Random.Range(0f, Mathf.PI * 2f);
            Vector3 axis = new Vector3(Mathf.Cos(heading), 0f, Mathf.Sin(heading));
            root.rotation = Quaternion.AngleAxis(tilt, axis) * Quaternion.Euler(0f, yaw, 0f);
        }

        static bool loggedTreeUp;

        void PlacePost(Transform parent, float x)
        {
            GameObject post = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.LockedPost, parent, false);
            post.name = "Post";
            post.transform.localScale = config.layout.lockedPostSize;
            post.transform.localPosition = new Vector3(x, config.layout.lockedPostSize.y * 0.5f, 0f);
        }

        BuildPad SpawnPad(BuildPad.Kind kind, Vector3 position, int cost, float hitPoints, Color color, bool blocking)
        {
            GameObject root = new GameObject(kind == BuildPad.Kind.Wall ? "WallPad" : "TurretPad");
            root.transform.position = new Vector3(position.x, 0f, position.z);

            GameObject disc = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Dirt, root.transform, false);
            disc.name = "Disc";
            disc.transform.localScale = Prop.CylinderScale(config.pour.padDiscDiameter, config.pour.padDiscHeight);
            disc.transform.localPosition = new Vector3(0f, config.pour.padDiscHeight * 0.5f, 0f);

            GameObject bar = Prop.Primitive(PrimitiveType.Cube, kind == BuildPad.Kind.Wall ? FortWorld.Look.Wall : FortWorld.Look.TurretAccent, root.transform, false);
            bar.name = "Fill";
            float side = config.pour.padDiscDiameter * 0.5f + config.presentation.fillBarSize.x;
            bar.transform.localPosition = new Vector3(side, 0f, 0f);
            bar.transform.localScale = config.presentation.fillBarSize;

            float labelX = kind == BuildPad.Kind.Wall ? -3.6f : 0.15f;
            float labelY = kind == BuildPad.Kind.Wall ? -1.25f : -0.35f;
            WorldLabel label = PadLabel(root.transform, labelX, labelY);

            GameObject pivotObject = new GameObject(kind == BuildPad.Kind.Wall ? "Wall" : "Turret");
            pivotObject.transform.SetParent(root.transform, false);
            if (blocking)
                pivotObject.transform.localPosition = config.layout.wallLocalOffset;
            Collider blocker = null;
            Turret weapon = null;
            Transform wallMesh = null;
            Renderer wallRenderer = null;
            if (blocking)
            {
                GameObject mesh = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Wall, pivotObject.transform, true);
                mesh.name = "WallMesh";
                mesh.transform.localScale = config.structures.wallSize;
                mesh.transform.localPosition = new Vector3(0f, config.structures.wallSize.y * 0.5f, 0f);
                mesh.GetComponent<Renderer>().enabled = false;
                blocker = mesh.GetComponent<Collider>();
                wallMesh = mesh.transform;
                wallRenderer = mesh.GetComponent<Renderer>();
            }

            StagedBuild stagedTurret = null;
            if (!blocking)
            {
                Transform aim;
                stagedTurret = StageKit.Turret(pivotObject.transform, out aim);
                weapon = pivotObject.AddComponent<Turret>();
                weapon.Init(aim);
                FortWorld.TurretWeapon = weapon;
            }

            BuildPad pad = root.AddComponent<BuildPad>();
            pad.Init(kind, cost, hitPoints, pivotObject.transform, blocker, disc.GetComponent<Renderer>(), bar.transform, label, weapon, color);
            if (wallMesh != null)
            {
                pad.BindMesh(wallMesh, wallRenderer);
                pad.BindStages(StageKit.Wall(pivotObject.transform));
            }
            else if (stagedTurret != null)
                pad.BindStages(stagedTurret);
            FortWorld.Pads.Add(pad);
            return pad;
        }
    }
}
