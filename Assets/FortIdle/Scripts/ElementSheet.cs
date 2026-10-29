using System.Collections;
using UnityEngine;

namespace FortIdle
{
    // Catalog framing for the element sheet. Not gameplay tuning.
    public class ElementSheet : MonoBehaviour
    {
        const int Columns = 6;
        const float PitchX = 11f;
        const float PitchZ = 9f;
        const float LabelZ = -3.7f;
        const string ShotName = "fort-idle-elements-v5.png";

        public static bool Requested()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-elements")
                    return true;
            }

            return false;
        }

        public static void Show(Light sun, Camera camera)
        {
            GameObject host = new GameObject("ElementSheet");
            ElementSheet sheet = host.AddComponent<ElementSheet>();
            sheet.Begin(sun, camera);
        }

        public void Begin(Light sun, Camera camera)
        {
            PaintDay(sun, camera);
            BuildFloor();
            PlaceAll();
            Frame(camera);
            StartCoroutine(Capture());
        }

        static void PaintDay(Light sun, Camera camera)
        {
            FortIdleConfig.PresentationTuning look = FortWorld.Config.presentation;
            camera.backgroundColor = look.daySky;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = look.daySky * look.ambientDay;
            RenderSettings.fog = false;
            if (sun == null)
                return;
            sun.color = look.daySun;
            sun.intensity = look.daySunIntensity;
            sun.transform.rotation = Quaternion.Euler(look.daySunPitch, look.sunYaw, 0f);
            if (FortLook.SoftShadows)
                sun.shadows = LightShadows.Soft;
        }

        static void BuildFloor()
        {
            GameObject floor = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Grass, null, false);
            floor.name = "SheetFloor";
            floor.transform.localScale = new Vector3(78f, 1f, 84f);
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.SetColor("_BaseColor", new Color(0.2f, 0.26f, 0.18f, 1f));
        }

        void PlaceAll()
        {
            int index = 0;
            Place(index++, "Wall pad", Pad(FortWorld.Config.presentation.padWall));
            Place(index++, "Turret pad", Pad(FortWorld.Config.presentation.padTurret));
            Place(index++, "Hut pad", Pad(FortWorld.Config.presentation.padHut));
            Place(index++, "Bag pad", Pad(FortWorld.Config.presentation.padBag));
            Place(index++, "Speed pad", Pad(FortWorld.Config.presentation.padSpeed));
            Place(index++, "Hire pad", Pad(FortWorld.Config.presentation.padHire));

            Place(index++, "Coin", Coin());
            Place(index++, "Planks", Planks());
            Place(index++, "Fire", Fire());
            Place(index++, "Hut", Hut());
            Place(index++, "Turret", Turret());
            Place(index++, "Gate tease", Gate());

            Place(index++, "Wall", Wall());
            Place(index++, "Hero", Hero());
            Place(index++, "Worker", Worker());
            Place(index++, "Shambler", MonsterOf(MonsterKind.Shambler, 0));
            Place(index++, "Brute", MonsterOf(MonsterKind.Brute, 1));
            Place(index++, "Treant", MonsterOf(MonsterKind.Treant, 2));

            Place(index++, "Tree", Tree(false));
            Place(index++, "Stump", Tree(true));
            Place(index++, "Ground", Ground());
            Place(index++, "Clearing", Clearing());

            Place(index++, "Repair", RepairWall());
            Place(index++, "Reinforced", TierWall(1));
            Place(index++, "Stone wall", TierWall(2));
            Place(index++, "Gate", BuiltGate());
            Place(index++, "Watchtower", Watchtower());
            Place(index++, "Spike trap", Spikes());

            Place(index++, "Rock", Rock());
            Place(index++, "Stone", StoneChunk());
            Place(index++, "Iron", IronBar());

            Place(index++, "Boar", BoarProp());
            Place(index++, "Meat", DropCube(FortWorld.Look.Meat, new Vector3(0.5f, 0.28f, 0.4f)));
            Place(index++, "Bone", DropCube(FortWorld.Look.Bone, new Vector3(0.18f, 0.18f, 0.7f)));
            Place(index++, "Hide", DropCube(FortWorld.Look.Hide, new Vector3(0.7f, 0.12f, 0.5f)));
            Place(index++, "Core", DropSphere(FortWorld.Look.Core, 0.45f));
            Place(index++, "Sap", DropSphere(FortWorld.Look.Sap, 0.38f));
            Place(index++, "Bone spikes", BoneSpikes());
            Place(index++, "Bonfire", Bonfire());
            Place(index++, "Trader", TraderProp());
        }

        static void Place(int index, string caption, GameObject specimen)
        {
            int column = index % Columns;
            int row = index / Columns;
            const int Rows = 7;
            float x = (column - (Columns - 1) * 0.5f) * PitchX;
            float z = ((Rows - 1) * 0.5f - row) * PitchZ;
            GameObject root = new GameObject(caption);
            root.transform.position = new Vector3(x, 0f, z);
            specimen.transform.SetParent(root.transform, false);

            GameObject plate = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Monster, root.transform, false);
            plate.name = "Plate";
            plate.transform.localPosition = new Vector3(0f, 0.06f, LabelZ);
            plate.transform.localScale = new Vector3(4.6f, 0.08f, 0.9f);

            WorldLabel label = WorldLabel.Create(
                root.transform,
                new Vector3(0f, 0.7f, LabelZ),
                caption,
                64,
                0.009f,
                new Vector2(520f, 120f));
            label.SetColor(FortWorld.Config.presentation.hudText);
        }

        static void Frame(Camera camera)
        {
            camera.fieldOfView = 54f;
            camera.transform.position = new Vector3(0f, 72f, -66f);
            camera.transform.LookAt(new Vector3(0f, 1.2f, 0f));
        }

        IEnumerator Capture()
        {
            string path = System.IO.Path.Combine(PoseShot.ShotDir(), ShotName);
            yield return new WaitForSeconds(0.4f);
            ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForSeconds(0.8f);
            Debug.Log("FORTIDLE elements=saved path=" + path);
            Application.Quit();
        }

        static GameObject Pad(Color color)
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject root = new GameObject("Pad");
            GameObject disc = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Dirt, root.transform, false);
            disc.transform.localScale = Prop.CylinderScale(config.pour.padDiscDiameter, config.pour.padDiscHeight);
            disc.transform.localPosition = new Vector3(0f, config.pour.padDiscHeight * 0.5f, 0f);
            disc.GetComponent<Renderer>().material.SetColor("_BaseColor", color);
            return root;
        }

        static GameObject Coin()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject coin = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Coin, null, false);
            coin.transform.localScale = Prop.CylinderScale(config.loot.coinDiameter, config.loot.coinThickness);
            coin.transform.localPosition = new Vector3(0f, config.loot.coinThickness * 0.5f + 0.4f, 0f);
            return coin;
        }

        static GameObject Planks()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject root = new GameObject("Planks");
            for (int i = 0; i < 4; i++)
            {
                GameObject plank = FortArt.Box(root.transform, "WoodLog", config.stack.plankSize, Color.white, 0f, false, false);
                if (plank == null)
                {
                    plank = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Plank, root.transform, false);
                    plank.transform.localScale = config.stack.plankSize;
                }

                plank.transform.localPosition = new Vector3(0f, (i + 0.5f) * config.stack.visualHeightStep + 0.7f, 0f);
            }

            return root;
        }

        static GameObject Fire()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject root = new GameObject("Fire");
            if (FortArt.Stand(root.transform, "Campfire", config.fire.flameHeight) == null)
            {
                GameObject log = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.FireLog, root.transform, false);
                log.transform.localScale = config.fire.logSize;
                log.transform.localPosition = new Vector3(0f, config.fire.logSize.y * 0.5f, 0f);
            }

            GameObject flame = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.Fire, root.transform, false);
            flame.transform.localScale = Prop.SphereScale(config.fire.flameDiameter);
            flame.transform.localPosition = new Vector3(0f, config.fire.flameHeight, 0f);
            return root;
        }

        static GameObject Hut()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject root = new GameObject("Hut");
            GameObject body = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.HutBody, root.transform, false);
            body.transform.localScale = config.structures.hutBodySize;
            body.transform.localPosition = new Vector3(0f, config.structures.hutBodySize.y * 0.5f, 0f);
            GameObject roof = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.HutRoof, root.transform, false);
            roof.transform.localScale = config.structures.hutRoofSize;
            roof.transform.localPosition = new Vector3(0f, config.structures.hutBodySize.y + config.structures.hutRoofSize.y * 0.5f, 0f);
            GameObject door = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.HutDoor, root.transform, false);
            door.transform.localScale = config.structures.hutDoorSize;
            door.transform.localPosition = new Vector3(0f, config.structures.hutDoorSize.y * 0.5f, config.structures.hutBodySize.z * 0.5f);
            root.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            return root;
        }

        static GameObject Turret()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject root = new GameObject("Turret");
            if (FortArt.Stand(root.transform, "Turret", config.structures.turretCapHeight, Color.white, 0f, false, true) == null)
            {
                GameObject pedestal = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Turret, root.transform, false);
                pedestal.transform.localScale = Prop.CylinderScale(config.structures.turretBaseRadius * 2f, config.structures.turretBaseHeight);
                pedestal.transform.localPosition = new Vector3(0f, config.structures.turretBaseHeight * 0.5f, 0f);
                GameObject cap = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.TurretAccent, root.transform, false);
                cap.transform.localScale = Prop.SphereScale(config.structures.turretCapDiameter);
                cap.transform.localPosition = new Vector3(0f, config.structures.turretCapHeight, 0f);
                GameObject barrel = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Turret, root.transform, false);
                barrel.transform.localScale = config.structures.turretBarrelSize;
                barrel.transform.localPosition = config.structures.turretBarrelLocalPosition;
            }

            root.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            return root;
        }

        static GameObject Gate()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject root = new GameObject("Gate");
            GameObject gate = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.LockedGate, root.transform, false);
            gate.transform.localScale = config.layout.lockedGateSize;
            gate.transform.localPosition = new Vector3(0f, config.layout.lockedGateSize.y * 0.5f, 0f);
            if (FortArt.Fill(gate.transform, "Gate", 1, true))
                gate.GetComponent<Renderer>().enabled = false;
            PlacePost(root.transform, -config.layout.lockedPostSpread);
            PlacePost(root.transform, config.layout.lockedPostSpread);
            return root;
        }

        static void PlacePost(Transform parent, float x)
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject post = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.LockedPost, parent, false);
            post.transform.localScale = config.layout.lockedPostSize;
            post.transform.localPosition = new Vector3(x, config.layout.lockedPostSize.y * 0.5f, 0f);
        }

        static GameObject Wall()
        {
            return TierWall(0);
        }

        static GameObject Hero()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject root = new GameObject("Hero");
            root.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            float height = config.player.headHeight + config.player.headDiameter * 0.5f;
            if (FortArt.Stand(root.transform, "Hero", height) == null)
            {
                GameObject torso = Prop.Primitive(PrimitiveType.Capsule, FortWorld.Look.HeroBody, root.transform, false);
                torso.transform.localScale = Prop.CapsuleScale(config.player.bodyRadius, config.player.bodyHeight);
                torso.transform.localPosition = new Vector3(0f, config.player.bodyHeight * 0.5f, 0f);
                GameObject head = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.HeroHead, root.transform, false);
                head.transform.localScale = Prop.SphereScale(config.player.headDiameter);
                head.transform.localPosition = new Vector3(0f, config.player.headHeight, 0f);
            }

            return root;
        }

        static GameObject Worker()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject root = new GameObject("Worker");
            root.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            float height = config.workers.headHeight + config.workers.headDiameter * 0.5f;
            if (FortArt.Stand(root.transform, "Worker", height) == null)
            {
                GameObject body = Prop.Primitive(PrimitiveType.Capsule, FortWorld.Look.WorkerBody, root.transform, false);
                body.transform.localScale = Prop.CapsuleScale(config.workers.bodyRadius, config.workers.bodyHeight);
                body.transform.localPosition = new Vector3(0f, config.workers.bodyHeight * 0.5f, 0f);
                GameObject head = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.WorkerHead, root.transform, false);
                head.transform.localScale = Prop.SphereScale(config.workers.headDiameter);
                head.transform.localPosition = new Vector3(0f, config.workers.headHeight, 0f);
            }

            return root;
        }

        static GameObject MonsterOf(MonsterKind kind, int slot)
        {
            GameObject root = new GameObject(kind.ToString());
            root.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Monster monster = root.AddComponent<Monster>();
            monster.Init(kind, slot, 3);
            monster.enabled = false;
            return root;
        }

        static GameObject Tree(bool stumpOnly)
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject root = new GameObject(stumpOnly ? "Stump" : "Tree");
            if (stumpOnly)
            {
                if (FortArt.Stand(root.transform, "Stump", config.layout.stumpHeight) == null)
                {
                    GameObject stump = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Stump, root.transform, false);
                    stump.transform.localScale = Prop.CylinderScale(config.layout.stumpRadius * 2f, config.layout.stumpHeight);
                    stump.transform.localPosition = new Vector3(0f, config.layout.stumpHeight * 0.5f, 0f);
                }

                return root;
            }

            float treeHeight = config.layout.canopyHeight + config.layout.canopyDiameter * 0.5f;
            if (FortArt.Stand(root.transform, "CommonTree", treeHeight) == null)
            {
                GameObject trunk = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Trunk, root.transform, false);
                trunk.transform.localScale = Prop.CylinderScale(config.layout.trunkRadius * 2f, config.layout.trunkHeight);
                trunk.transform.localPosition = new Vector3(0f, config.layout.trunkHeight * 0.5f, 0f);
                GameObject canopy = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.Canopy, root.transform, false);
                canopy.transform.localScale = Prop.SphereScale(config.layout.canopyDiameter);
                canopy.transform.localPosition = new Vector3(0f, config.layout.canopyHeight, 0f);
            }

            return root;
        }

        static GameObject Ground()
        {
            GameObject root = new GameObject("Ground");
            GameObject tile = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Grass, root.transform, false);
            tile.transform.localScale = new Vector3(4.2f, 0.45f, 4.2f);
            tile.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            FortArt.Stand(root.transform, "GrassTuft", 0.7f);
            GameObject bush = FortArt.Stand(root.transform, "Bush", 0.9f);
            if (bush != null)
                bush.transform.localPosition = new Vector3(1.1f, 0f, 0.4f);
            return root;
        }

        static GameObject RepairWall()
        {
            GameObject wall = Wall();
            GameObject glow = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Dirt, wall.transform, false);
            glow.name = "RepairGlow";
            FortIdleConfig config = FortWorld.Config;
            glow.transform.localScale = Prop.CylinderScale(config.pour.padDiscDiameter, config.pour.padDiscHeight);
            glow.transform.localPosition = new Vector3(0f, 0.08f, -1.6f);
            glow.GetComponent<Renderer>().material.SetColor("_BaseColor", config.presentation.padRepair);
            return wall;
        }

        static GameObject TierWall(int tier)
        {
            FortIdleConfig config = FortWorld.Config;
            float height = tier >= 2 ? 1.22f : tier == 1 ? 1.08f : 1f;
            GameObject wall = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Wall, null, false);
            Vector3 size = config.structures.wallSize;
            wall.transform.localScale = new Vector3(size.x, size.y * height, size.z);
            wall.transform.localPosition = new Vector3(0f, size.y * height * 0.5f, 0f);
            string id = tier >= 2 ? "WallStone" : tier == 1 ? "WallReinforced" : "WallWood";
            int tiles = tier == 0 ? 4 : 3;
            if (FortArt.Fill(wall.transform, id, tiles, true))
                wall.GetComponent<Renderer>().enabled = false;
            else if (tier == 1)
                wall.GetComponent<Renderer>().sharedMaterial = FortWorld.Look.WallReinforced;
            else if (tier >= 2)
                wall.GetComponent<Renderer>().sharedMaterial = FortWorld.Look.WallStone;
            return wall;
        }

        static GameObject BuiltGate()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject gate = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.LockedGate, null, false);
            gate.transform.localScale = config.defenses.gateSize;
            gate.transform.localPosition = new Vector3(0f, config.defenses.gateSize.y * 0.5f, 0f);
            if (FortArt.Fill(gate.transform, "Gate", 1, true))
                gate.GetComponent<Renderer>().enabled = false;
            return gate;
        }

        static GameObject Watchtower()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject root = new GameObject("Watchtower");
            if (FortArt.Stand(root.transform, "Watchtower", config.defenses.towerHeight, Color.white, 0f, false, true) == null)
            {
                GameObject post = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Turret, root.transform, false);
                post.transform.localScale = Prop.CylinderScale(config.defenses.towerRadius * 2f, config.defenses.towerHeight);
                post.transform.localPosition = new Vector3(0f, config.defenses.towerHeight * 0.5f, 0f);
                GameObject cap = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.HutRoof, root.transform, false);
                cap.transform.localScale = new Vector3(1.3f, 0.18f, 1.3f);
                cap.transform.localPosition = new Vector3(0f, config.defenses.towerHeight + 0.1f, 0f);
            }

            GameObject archer = FortArt.Stand(root.transform, "Worker", 0.7f);
            if (archer != null)
                archer.transform.localPosition = new Vector3(0f, config.defenses.towerHeight, 0f);
            return root;
        }

        static GameObject Spikes()
        {
            GameObject root = new GameObject("Spikes");
            GameObject disc = Pad(FortWorld.Config.presentation.padSpike);
            disc.transform.SetParent(root.transform, false);
            float span = FortWorld.Config.defenses.spikeRadius * 2f;
            GameObject box = new GameObject("SpikeBox");
            box.transform.SetParent(root.transform, false);
            box.transform.localScale = new Vector3(span, 0.55f, span * 0.45f);
            box.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            if (!FortArt.Fill(box.transform, "Spikes", 1, true))
            {
                for (int i = 0; i < 5; i++)
                {
                    GameObject spike = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Spike, root.transform, false);
                    spike.transform.localScale = new Vector3(0.12f, 0.55f, 0.12f);
                    spike.transform.localPosition = new Vector3((i - 2) * 0.45f, 0.35f, 0f);
                }
            }

            return root;
        }

        static GameObject Rock()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject rock = FortArt.Stand(null, "Rock", config.supplies.rockHeight);
            if (rock != null)
                return rock;
            rock = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Rock, null, false);
            rock.transform.localScale = new Vector3(config.supplies.rockRadius * 2f, config.supplies.rockHeight, config.supplies.rockRadius * 1.6f);
            rock.transform.localPosition = new Vector3(0f, config.supplies.rockHeight * 0.5f, 0f);
            return rock;
        }

        static GameObject StoneChunk()
        {
            Vector3 size = FortWorld.Config.supplies.stoneChunkSize * 2.4f;
            GameObject chunk = FortArt.Box(null, "RockMoss", size, Color.white, 0.05f, false, false);
            if (chunk == null)
            {
                chunk = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Stone, null, false);
                chunk.transform.localScale = size;
            }

            chunk.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            return chunk;
        }

        static GameObject IronBar()
        {
            Vector3 size = FortWorld.Config.supplies.ironBarSize * 2.2f;
            GameObject bar = FortArt.Box(null, "IronRock", size, FortArt.IronTint, 0.72f, true, false);
            if (bar == null)
            {
                bar = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Iron, null, false);
                bar.transform.localScale = size;
            }

            bar.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            return bar;
        }

        static GameObject Clearing()
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject disc = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Dirt, null, false);
            disc.transform.localScale = Prop.CylinderScale(config.layout.clearingDiameter * 0.3f, config.layout.clearingThickness);
            disc.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            return disc;
        }

        static GameObject TraderProp()
        {
            GameObject root = new GameObject("Trader");
            GameObject crate = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.HutBody, root.transform, false);
            crate.transform.localScale = new Vector3(1.4f, 0.9f, 0.9f);
            crate.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            GameObject awning = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Coin, root.transform, false);
            awning.transform.localScale = new Vector3(1.7f, 0.12f, 1.1f);
            awning.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            return root;
        }

        static GameObject BoarProp()
        {
            GameObject root = new GameObject("Boar");
            GameObject body = Prop.Primitive(PrimitiveType.Capsule, FortWorld.Look.Boar, root.transform, false);
            body.transform.localScale = new Vector3(0.7f, 0.35f, 0.9f);
            body.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            return root;
        }

        static GameObject DropCube(Material material, Vector3 size)
        {
            GameObject cube = Prop.Primitive(PrimitiveType.Cube, material, null, false);
            cube.transform.localScale = size;
            cube.transform.localPosition = new Vector3(0f, size.y * 0.5f + 0.2f, 0f);
            return cube;
        }

        static GameObject DropSphere(Material material, float diameter)
        {
            GameObject sphere = Prop.Primitive(PrimitiveType.Sphere, material, null, false);
            sphere.transform.localScale = Prop.SphereScale(diameter);
            sphere.transform.localPosition = new Vector3(0f, diameter, 0f);
            return sphere;
        }

        static GameObject BoneSpikes()
        {
            GameObject root = new GameObject("BoneSpikes");
            for (int i = 0; i < 4; i++)
            {
                GameObject spike = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Bone, root.transform, false);
                spike.transform.localScale = new Vector3(0.14f, 0.7f, 0.14f);
                spike.transform.localPosition = new Vector3((i - 1.5f) * 0.35f, 0.35f, 0f);
            }

            return root;
        }

        static GameObject Bonfire()
        {
            GameObject root = new GameObject("Bonfire");
            for (int i = 0; i < 5; i++)
            {
                float angle = i / 5f * Mathf.PI * 2f;
                GameObject rock = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Stone, root.transform, false);
                rock.transform.localScale = new Vector3(0.28f, 0.22f, 0.28f);
                rock.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.55f, 0.12f, Mathf.Sin(angle) * 0.55f);
            }

            GameObject flame = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.Fire, root.transform, false);
            flame.transform.localScale = Prop.SphereScale(0.7f);
            flame.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            return root;
        }
    }
}
