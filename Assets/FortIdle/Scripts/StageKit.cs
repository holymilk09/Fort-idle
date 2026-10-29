using UnityEngine;

namespace FortIdle
{
    // Greybox part groups. Names stay stable so a later mesh can replace one piece.
    public static class StageKit
    {
        public static StagedBuild Wall(Transform pivot)
        {
            GameObject visual = Child(pivot, "StageVisual");
            StagedBuild staged = visual.AddComponent<StagedBuild>();
            Vector3 size = FortWorld.Config.structures.wallSize;
            float half = size.x * 0.5f - 0.28f;
            Material wood = FortWorld.Look.Wall;
            Material iron = FortWorld.Look.Iron;
            Material stone = FortWorld.Look.WallStone != null ? FortWorld.Look.WallStone : FortWorld.Look.Stone;

            Transform posts = Group(visual.transform, "Posts");
            for (int i = 0; i < 4; i++)
            {
                float x = Mathf.Lerp(-half, half, i / 3f);
                Cylinder(posts, "Post", wood, new Vector3(x, size.y * 0.46f, 0f), 0.26f, size.y * 0.92f);
            }

            staged.Add(0, 0, posts, "rise", null);

            Transform rails = Group(visual.transform, "Rails");
            Cube(rails, "Rail", wood, new Vector3(0f, size.y * 0.28f, 0f), new Vector3(size.x * 0.9f, 0.12f, 0.16f));
            Cube(rails, "Rail", wood, new Vector3(0f, size.y * 0.62f, 0f), new Vector3(size.x * 0.9f, 0.12f, 0.16f));
            staged.Add(0, 1, rails, "drop", null);

            Transform low = Group(visual.transform, "PlanksLow");
            FillBoards(low, wood, size, 0.55f, 0.7f);
            staged.Add(0, 2, low, "drop", null);

            Transform high = Group(visual.transform, "PlanksHigh");
            FillBoards(high, wood, size, size.y * 0.72f, 0.62f);
            staged.Add(0, 3, high, "drop", null);

            Transform caps = Group(visual.transform, "Caps");
            for (int i = 0; i < 4; i++)
            {
                float x = Mathf.Lerp(-half, half, i / 3f);
                Cube(caps, "Cap", wood, new Vector3(x, size.y * 0.98f, 0f), new Vector3(0.34f, 0.22f, 0.34f));
            }

            staged.Add(0, 4, caps, "drop", null);

            Transform bands = Group(visual.transform, "IronBands");
            for (int i = 0; i < 4; i++)
            {
                float x = Mathf.Lerp(-half, half, i / 3f);
                Cube(bands, "Band", iron, new Vector3(x, size.y * 0.48f, 0.12f), new Vector3(0.1f, size.y * 0.7f, 0.08f));
            }

            staged.Add(1, 0, bands, "drop", null);
            Transform strap = Group(visual.transform, "MidStrap");
            Cube(strap, "Strap", iron, new Vector3(0f, size.y * 0.45f, 0.16f), new Vector3(size.x * 0.88f, 0.1f, 0.08f));
            staged.Add(1, 1, strap, "drop", null);
            Transform foot = Group(visual.transform, "Footings");
            for (int i = 0; i < 4; i++)
            {
                float x = Mathf.Lerp(-half, half, i / 3f);
                Cube(foot, "Foot", stone, new Vector3(x, 0.16f, 0.18f), new Vector3(0.46f, 0.28f, 0.4f));
            }

            staged.Add(1, 2, foot, "rise", null);
            Transform crest = Group(visual.transform, "Crest");
            Cube(crest, "Crest", iron, new Vector3(0f, size.y * 1.02f, 0f), new Vector3(size.x * 0.92f, 0.1f, 0.28f));
            staged.Add(1, 3, crest, "drop", null);

            Transform course = Group(visual.transform, "StoneCourse");
            FillBoards(course, stone, size, 0.4f, 0.55f);
            staged.Add(2, 0, course, "rise", null);
            Transform courseHigh = Group(visual.transform, "StoneCourseHigh");
            FillBoards(courseHigh, stone, size, size.y * 0.7f, 0.5f);
            staged.Add(2, 1, courseHigh, "drop", null);
            Transform merlon = Group(visual.transform, "Battlement");
            for (int i = 0; i < 6; i++)
            {
                float x = Mathf.Lerp(-half, half, i / 5f);
                Cube(merlon, "Merlon", stone, new Vector3(x, size.y * 1.12f, 0f), new Vector3(0.38f, 0.32f, 0.36f));
            }

            staged.Add(2, 2, merlon, "drop", null);
            Transform spikes = Group(visual.transform, "IronCaps");
            for (int i = 0; i < 4; i++)
            {
                float x = Mathf.Lerp(-half, half, i / 3f);
                Cube(spikes, "Spike", iron, new Vector3(x, size.y * 1.28f, 0f), new Vector3(0.1f, 0.28f, 0.1f));
            }

            staged.Add(2, 3, spikes, "drop", null);
            return staged;
        }

        public static StagedBuild Gate(Transform pivot)
        {
            GameObject visual = Child(pivot, "StageVisual");
            StagedBuild staged = visual.AddComponent<StagedBuild>();
            Vector3 size = FortWorld.Config.defenses.gateSize;
            Material wood = FortWorld.Look.Wall;
            Material iron = FortWorld.Look.Iron;
            Material stone = FortWorld.Look.Stone;
            float half = size.x * 0.5f - 0.2f;

            Transform posts = Group(visual.transform, "FramePosts");
            Cylinder(posts, "Post", wood, new Vector3(-half, size.y * 0.5f, 0f), 0.34f, size.y);
            Cylinder(posts, "Post", wood, new Vector3(half, size.y * 0.5f, 0f), 0.34f, size.y);
            staged.Add(0, 0, posts, "rise", null);

            Transform beam = Group(visual.transform, "Crossbeam");
            Cube(beam, "Beam", wood, new Vector3(0f, size.y * 0.96f, 0f), new Vector3(size.x * 0.96f, 0.22f, 0.36f));
            staged.Add(0, 1, beam, "drop", null);

            Transform door = Group(visual.transform, "Door");
            door.transform.localPosition = new Vector3(-half + 0.15f, 0f, 0f);
            for (int i = 0; i < 6; i++)
            {
                Cube(door, "Plank", wood, new Vector3(0.35f + i * 0.85f, size.y * 0.42f, 0f), new Vector3(0.7f, size.y * 0.78f, 0.1f));
            }

            staged.Add(0, 2, door, "open", null);

            Transform hinges = Group(visual.transform, "Hinges");
            Cube(hinges, "Hinge", iron, new Vector3(-half + 0.05f, size.y * 0.3f, 0.12f), new Vector3(0.28f, 0.16f, 0.12f));
            Cube(hinges, "Hinge", iron, new Vector3(-half + 0.05f, size.y * 0.7f, 0.12f), new Vector3(0.28f, 0.16f, 0.12f));
            Cube(hinges, "Band", iron, new Vector3(0f, size.y * 0.48f, 0.14f), new Vector3(size.x * 0.7f, 0.1f, 0.08f));
            staged.Add(0, 3, hinges, "drop", null);

            Transform latch = Group(visual.transform, "Latch");
            Cube(latch, "Latch", iron, new Vector3(half - 0.15f, size.y * 0.5f, 0.16f), new Vector3(0.22f, 0.5f, 0.1f));
            staged.Add(0, 4, latch, "swing", door.transform);

            Transform plates = Group(visual.transform, "HingePlates");
            Cube(plates, "Plate", iron, new Vector3(-half, size.y * 0.5f, 0.2f), new Vector3(0.16f, size.y * 0.55f, 0.08f));
            Cube(plates, "Plate", iron, new Vector3(half, size.y * 0.5f, 0.2f), new Vector3(0.16f, size.y * 0.55f, 0.08f));
            staged.Add(1, 0, plates, "drop", null);
            Transform center = Group(visual.transform, "CenterBand");
            Cube(center, "Band", iron, new Vector3(0f, size.y * 0.62f, 0.2f), new Vector3(size.x * 0.8f, 0.14f, 0.08f));
            staged.Add(1, 1, center, "drop", null);
            Transform braces = Group(visual.transform, "CornerBraces");
            Cube(braces, "Brace", iron, new Vector3(-half + 0.4f, size.y * 0.82f, 0.18f), new Vector3(0.7f, 0.1f, 0.08f));
            Cube(braces, "Brace", iron, new Vector3(half - 0.4f, size.y * 0.82f, 0.18f), new Vector3(0.7f, 0.1f, 0.08f));
            staged.Add(1, 2, braces, "drop", null);
            Transform bar = Group(visual.transform, "LatchBar");
            Cube(bar, "Bar", iron, new Vector3(half - 0.2f, size.y * 0.5f, 0.24f), new Vector3(0.12f, 0.7f, 0.12f));
            staged.Add(1, 3, bar, "drop", null);

            Transform jambs = Group(visual.transform, "StoneJambs");
            Cube(jambs, "Jamb", stone, new Vector3(-half, size.y * 0.5f, -0.2f), new Vector3(0.4f, size.y, 0.28f));
            Cube(jambs, "Jamb", stone, new Vector3(half, size.y * 0.5f, -0.2f), new Vector3(0.4f, size.y, 0.28f));
            staged.Add(2, 0, jambs, "rise", null);
            Transform port = Group(visual.transform, "Portcullis");
            for (int i = 0; i < 5; i++)
            {
                float x = Mathf.Lerp(-half + 0.5f, half - 0.5f, i / 4f);
                Cube(port, "Bar", iron, new Vector3(x, size.y * 0.45f, 0.28f), new Vector3(0.08f, size.y * 0.7f, 0.08f));
            }

            staged.Add(2, 1, port, "drop", null);
            Transform tips = Group(visual.transform, "PortcullisTips");
            for (int i = 0; i < 5; i++)
            {
                float x = Mathf.Lerp(-half + 0.5f, half - 0.5f, i / 4f);
                Cube(tips, "Tip", FortWorld.Look.Spike, new Vector3(x, size.y * 0.12f, 0.28f), new Vector3(0.1f, 0.22f, 0.1f));
            }

            staged.Add(2, 2, tips, "drop", null);
            Transform cap = Group(visual.transform, "StoneCap");
            Cube(cap, "Cap", stone, new Vector3(0f, size.y * 1.08f, 0f), new Vector3(size.x, 0.2f, 0.5f));
            staged.Add(2, 3, cap, "drop", null);
            return staged;
        }

        public static StagedBuild Tower(Transform pivot, out Transform aim)
        {
            GameObject visual = Child(pivot, "StageVisual");
            StagedBuild staged = visual.AddComponent<StagedBuild>();
            FortIdleConfig.DefenseTuning defenses = FortWorld.Config.defenses;
            float height = defenses.towerHeight;
            Material wood = FortWorld.Look.Wall;
            Material iron = FortWorld.Look.Iron;
            Material stone = FortWorld.Look.Stone;
            float leg = 0.55f;

            Transform legs = Group(visual.transform, "Legs");
            Cylinder(legs, "Leg", wood, new Vector3(-leg, height * 0.42f, -leg), 0.16f, height * 0.84f);
            Cylinder(legs, "Leg", wood, new Vector3(leg, height * 0.42f, -leg), 0.16f, height * 0.84f);
            Cylinder(legs, "Leg", wood, new Vector3(-leg, height * 0.42f, leg), 0.16f, height * 0.84f);
            Cylinder(legs, "Leg", wood, new Vector3(leg, height * 0.42f, leg), 0.16f, height * 0.84f);
            staged.Add(0, 0, legs, "rise", null);

            Transform braces = Group(visual.transform, "Braces");
            Cube(braces, "Brace", wood, new Vector3(0f, height * 0.38f, -leg), new Vector3(leg * 2.1f, 0.08f, 0.08f));
            Cube(braces, "Brace", wood, new Vector3(0f, height * 0.38f, leg), new Vector3(leg * 2.1f, 0.08f, 0.08f));
            Cube(braces, "Brace", wood, new Vector3(-leg, height * 0.55f, 0f), new Vector3(0.08f, 0.08f, leg * 2.1f));
            Cube(braces, "Brace", wood, new Vector3(leg, height * 0.55f, 0f), new Vector3(0.08f, 0.08f, leg * 2.1f));
            staged.Add(0, 1, braces, "drop", null);

            Transform platform = Group(visual.transform, "Platform");
            Cube(platform, "Deck", wood, new Vector3(0f, height, 0f), new Vector3(1.55f, 0.12f, 1.55f));
            staged.Add(0, 2, platform, "drop", null);

            Transform rail = Group(visual.transform, "Railing");
            Cube(rail, "Rail", wood, new Vector3(0f, height + 0.45f, 0.72f), new Vector3(1.45f, 0.08f, 0.08f));
            Cube(rail, "Rail", wood, new Vector3(0f, height + 0.45f, -0.72f), new Vector3(1.45f, 0.08f, 0.08f));
            Cube(rail, "Rail", wood, new Vector3(0.72f, height + 0.45f, 0f), new Vector3(0.08f, 0.08f, 1.45f));
            Cube(rail, "Rail", wood, new Vector3(-0.72f, height + 0.45f, 0f), new Vector3(0.08f, 0.08f, 1.45f));
            staged.Add(0, 3, rail, "drop", null);

            Transform roof = Group(visual.transform, "Roof");
            Cube(roof, "Roof", FortWorld.Look.HutRoof, new Vector3(0f, height + 0.85f, 0f), new Vector3(1.7f, 0.14f, 1.7f));
            staged.Add(0, 4, roof, "drop", null);

            Transform archer = Group(visual.transform, "Archer");
            GameObject body = Prop.Primitive(PrimitiveType.Capsule, FortWorld.Look.HeroBody, archer, false);
            body.name = "Archer";
            body.transform.localScale = Prop.CapsuleScale(0.18f, 0.7f);
            body.transform.localPosition = new Vector3(0f, height + 0.15f, 0f);
            GameObject bow = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Turret, archer, false);
            bow.name = "Bow";
            bow.transform.localScale = new Vector3(0.08f, 0.4f, 0.08f);
            bow.transform.localPosition = new Vector3(0.2f, height + 0.2f, 0.2f);
            staged.Add(0, 5, archer, "climb", body.transform);
            aim = bow.transform;
            staged.Aim = aim;

            Transform shoes = Group(visual.transform, "StoneShoes");
            Cube(shoes, "Shoe", stone, new Vector3(-leg, 0.12f, -leg), new Vector3(0.36f, 0.22f, 0.36f));
            Cube(shoes, "Shoe", stone, new Vector3(leg, 0.12f, -leg), new Vector3(0.36f, 0.22f, 0.36f));
            Cube(shoes, "Shoe", stone, new Vector3(-leg, 0.12f, leg), new Vector3(0.36f, 0.22f, 0.36f));
            Cube(shoes, "Shoe", stone, new Vector3(leg, 0.12f, leg), new Vector3(0.36f, 0.22f, 0.36f));
            staged.Add(1, 0, shoes, "rise", null);
            Transform extra = Group(visual.transform, "ExtraBrace");
            Cube(extra, "Brace", wood, new Vector3(0f, height * 0.7f, 0f), new Vector3(leg * 2f, 0.08f, 0.08f));
            staged.Add(1, 1, extra, "drop", null);
            Transform mate = Group(visual.transform, "SecondArcher");
            GameObject mateBody = Prop.Primitive(PrimitiveType.Capsule, FortWorld.Look.HeroBody, mate, false);
            mateBody.name = "Archer";
            mateBody.transform.localScale = Prop.CapsuleScale(0.16f, 0.62f);
            mateBody.transform.localPosition = new Vector3(0.28f, height + 0.12f, -0.15f);
            staged.Add(1, 2, mate, "climb", mateBody.transform);
            Transform caps = Group(visual.transform, "RailCaps");
            Cube(caps, "Cap", iron, new Vector3(0.72f, height + 0.62f, 0.72f), new Vector3(0.12f, 0.22f, 0.12f));
            Cube(caps, "Cap", iron, new Vector3(-0.72f, height + 0.62f, -0.72f), new Vector3(0.12f, 0.22f, 0.12f));
            staged.Add(1, 3, caps, "drop", null);

            Transform band = Group(visual.transform, "RoofBand");
            Cube(band, "Band", iron, new Vector3(0f, height + 0.95f, 0f), new Vector3(1.55f, 0.08f, 1.55f));
            staged.Add(2, 0, band, "drop", null);
            Transform banner = Group(visual.transform, "Banner");
            Cube(banner, "Banner", FortWorld.Look.Fire, new Vector3(0f, height + 1.2f, 0f), new Vector3(0.5f, 0.28f, 0.06f));
            staged.Add(2, 1, banner, "drop", null);
            Transform bowl = Group(visual.transform, "FireBowl");
            Cube(bowl, "Bowl", iron, new Vector3(0.45f, height + 0.2f, 0f), new Vector3(0.28f, 0.1f, 0.28f));
            staged.Add(2, 2, bowl, "drop", null);
            Transform rim = Group(visual.transform, "RoofRim");
            Cube(rim, "Rim", stone, new Vector3(0f, height + 1.05f, 0f), new Vector3(1.85f, 0.1f, 1.85f));
            staged.Add(2, 3, rim, "drop", null);
            return staged;
        }

        public static StagedBuild Turret(Transform pivot, out Transform aim)
        {
            GameObject visual = Child(pivot, "StageVisual");
            StagedBuild staged = visual.AddComponent<StagedBuild>();
            FortIdleConfig.StructureTuning structures = FortWorld.Config.structures;
            Material body = FortWorld.Look.Turret;
            Material accent = FortWorld.Look.TurretAccent;
            Material iron = FortWorld.Look.Iron;

            Transform baseGroup = Group(visual.transform, "Base");
            Cylinder(baseGroup, "Base", body, new Vector3(0f, 0.22f, 0f), structures.turretBaseRadius * 2.4f, 0.44f);
            staged.Add(0, 0, baseGroup, "rise", null);

            Transform pivotGroup = Group(visual.transform, "Pivot");
            Cylinder(pivotGroup, "Pivot", body, new Vector3(0f, 0.7f, 0f), structures.turretBaseRadius * 1.1f, 0.55f);
            staged.Add(0, 1, pivotGroup, "drop", null);

            Transform frame = Group(visual.transform, "Frame");
            Cube(frame, "Frame", accent, new Vector3(0f, structures.turretCapHeight, 0f), new Vector3(0.72f, 0.32f, 0.72f));
            staged.Add(0, 2, frame, "drop", null);

            Transform arm = Group(visual.transform, "BoltArm");
            Cube(arm, "BoltArm", body, structures.turretBarrelLocalPosition, structures.turretBarrelSize);
            staged.Add(0, 3, arm, "drop", null);
            aim = arm.GetChild(0);

            Transform sight = Group(visual.transform, "Sight");
            Cube(sight, "Sight", accent, structures.turretBarrelLocalPosition + new Vector3(0f, 0.2f, 0f), new Vector3(0.12f, 0.12f, 0.3f));
            staged.Add(0, 4, sight, "spin", aim);

            Transform ring = Group(visual.transform, "IronRing");
            Cylinder(ring, "Ring", iron, new Vector3(0f, 0.48f, 0f), structures.turretBaseRadius * 2.1f, 0.1f);
            staged.Add(1, 0, ring, "drop", null);
            Transform gear = Group(visual.transform, "Gear");
            Cube(gear, "Gear", iron, new Vector3(0.35f, 0.85f, 0f), new Vector3(0.28f, 0.28f, 0.12f));
            staged.Add(1, 1, gear, "drop", null);
            Transform sleeve = Group(visual.transform, "Sleeve");
            Cube(sleeve, "Sleeve", iron, structures.turretBarrelLocalPosition + new Vector3(0f, 0f, 0.35f), new Vector3(0.28f, 0.28f, 0.4f));
            staged.Add(1, 2, sleeve, "drop", null);
            Transform crank = Group(visual.transform, "Crank");
            Cube(crank, "Crank", accent, new Vector3(-0.35f, 1.05f, 0f), new Vector3(0.1f, 0.28f, 0.1f));
            staged.Add(1, 3, crank, "drop", null);

            Transform cup = Group(visual.transform, "FlameCup");
            GameObject flame = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.Fire, cup, false);
            flame.name = "FlameCup";
            flame.transform.localScale = Prop.SphereScale(0.28f);
            flame.transform.localPosition = structures.turretBarrelLocalPosition + new Vector3(0f, 0.15f, 0.55f);
            staged.Add(2, 0, cup, "drop", null);
            staged.Aim = aim;
            return staged;
        }

        public static StagedBuild Spike(Transform pivot)
        {
            GameObject visual = Child(pivot, "StageVisual");
            StagedBuild staged = visual.AddComponent<StagedBuild>();
            float radius = FortWorld.Config.defenses.spikeRadius;
            Material dirt = FortWorld.Look.Dirt;
            Material wood = FortWorld.Look.Spike;
            Material bone = FortWorld.Look.Bone;
            Material iron = FortWorld.Look.Iron;

            Transform pit = Group(visual.transform, "Pit");
            Cylinder(pit, "Pit", dirt, new Vector3(0f, 0.04f, 0f), radius * 1.7f, 0.08f);
            staged.Add(0, 0, pit, "rise", null);

            Transform stakes = Group(visual.transform, "Stakes");
            for (int i = 0; i < 5; i++)
            {
                float x = (i - 2) * 0.42f;
                Cube(stakes, "Stake", wood, new Vector3(x, 0.22f, 0f), new Vector3(0.1f, 0.4f, 0.1f));
            }

            staged.Add(0, 1, stakes, "rise", null);

            Transform tips = Group(visual.transform, "Tips");
            for (int i = 0; i < 5; i++)
            {
                float x = (i - 2) * 0.42f;
                Cube(tips, "Tip", wood, new Vector3(x, 0.5f, 0f), new Vector3(0.06f, 0.22f, 0.06f));
            }

            staged.Add(0, 2, tips, "drop", null);

            Transform bones = Group(visual.transform, "BoneStakes");
            for (int i = 0; i < 4; i++)
            {
                float x = (i - 1.5f) * 0.4f;
                Cube(bones, "Bone", bone, new Vector3(x, 0.28f, 0.28f), new Vector3(0.08f, 0.46f, 0.08f));
            }

            staged.Add(1, 0, bones, "rise", null);
            Transform ironTips = Group(visual.transform, "IronTips");
            for (int i = 0; i < 5; i++)
            {
                float x = (i - 2) * 0.42f;
                Cube(ironTips, "Tip", iron, new Vector3(x, 0.66f, 0f), new Vector3(0.05f, 0.16f, 0.05f));
            }

            staged.Add(2, 0, ironTips, "drop", null);
            return staged;
        }

        public static StagedBuild Hut(Transform pivot, Transform body, Transform roof, Transform door)
        {
            GameObject visual = Child(pivot, "StageVisual");
            StagedBuild staged = visual.AddComponent<StagedBuild>();
            Vector3 size = FortWorld.Config.structures.hutBodySize;
            Material wood = FortWorld.Look.HutBody;
            Transform posts = Group(visual.transform, "Posts");
            float x = size.x * 0.42f;
            float z = size.z * 0.42f;
            Cylinder(posts, "Post", wood, new Vector3(-x, size.y * 0.5f, -z), 0.16f, size.y);
            Cylinder(posts, "Post", wood, new Vector3(x, size.y * 0.5f, -z), 0.16f, size.y);
            Cylinder(posts, "Post", wood, new Vector3(-x, size.y * 0.5f, z), 0.16f, size.y);
            Cylinder(posts, "Post", wood, new Vector3(x, size.y * 0.5f, z), 0.16f, size.y);
            staged.Add(0, 0, posts, "rise", null);
            Park(body, visual.transform);
            staged.Add(0, 1, body, "drop", null);
            Park(roof, visual.transform);
            staged.Add(0, 2, roof, "drop", null);
            Park(door, visual.transform);
            staged.Add(0, 3, door, "drop", null);
            return staged;
        }

        public static StagedBuild Fire(Transform root, out Transform flame)
        {
            GameObject visual = Child(root, "StageVisual");
            StagedBuild staged = visual.AddComponent<StagedBuild>();
            Material stone = FortWorld.Look.Stone;
            Material log = FortWorld.Look.FireLog;
            Material iron = FortWorld.Look.Iron;

            Transform ring = Group(visual.transform, "StoneRing");
            for (int i = 0; i < 8; i++)
            {
                float angle = i / 8f * Mathf.PI * 2f;
                Cube(ring, "Stone", stone, new Vector3(Mathf.Cos(angle) * 0.55f, 0.1f, Mathf.Sin(angle) * 0.55f), new Vector3(0.24f, 0.18f, 0.24f));
            }

            staged.Add(0, 0, ring, "rise", null);
            Transform logs = Group(visual.transform, "Logs");
            Cube(logs, "Log", log, new Vector3(0f, 0.12f, 0f), FortWorld.Config.fire.logSize);
            Cube(logs, "Log", log, new Vector3(0f, 0.22f, 0f), new Vector3(FortWorld.Config.fire.logSize.z, FortWorld.Config.fire.logSize.y, FortWorld.Config.fire.logSize.x));
            staged.Add(0, 1, logs, "drop", null);

            Transform flameGroup = Group(visual.transform, "Flame");
            GameObject flameObject = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.Fire, flameGroup, false);
            flameObject.name = "Flame";
            flameObject.transform.localScale = Prop.SphereScale(FortWorld.Config.fire.flameDiameter);
            flameObject.transform.localPosition = new Vector3(0f, FortWorld.Config.fire.flameHeight, 0f);
            flame = flameObject.transform;
            staged.Add(0, 2, flameGroup, "drop", null);

            Transform outer = Group(visual.transform, "OuterStones");
            for (int i = 0; i < 6; i++)
            {
                float angle = i / 6f * Mathf.PI * 2f;
                Cube(outer, "Stone", stone, new Vector3(Mathf.Cos(angle) * 0.85f, 0.12f, Mathf.Sin(angle) * 0.85f), new Vector3(0.28f, 0.2f, 0.28f));
            }

            staged.Add(1, 0, outer, "rise", null);
            Transform extra = Group(visual.transform, "ExtraLogs");
            Cube(extra, "Log", log, new Vector3(0.15f, 0.32f, 0f), new Vector3(0.7f, 0.14f, 0.18f));
            staged.Add(1, 1, extra, "drop", null);
            Transform embers = Group(visual.transform, "Embers");
            Cube(embers, "Ember", FortWorld.Look.Fire, new Vector3(0.2f, 0.16f, 0.15f), new Vector3(0.12f, 0.08f, 0.12f));
            Cube(embers, "Ember", FortWorld.Look.Fire, new Vector3(-0.18f, 0.16f, -0.1f), new Vector3(0.12f, 0.08f, 0.12f));
            staged.Add(1, 2, embers, "drop", null);
            Transform kindling = Group(visual.transform, "Kindling");
            Cube(kindling, "Stick", log, new Vector3(0f, 0.4f, 0.1f), new Vector3(0.5f, 0.08f, 0.08f));
            staged.Add(1, 3, kindling, "drop", null);

            Transform grate = Group(visual.transform, "Grate");
            Cube(grate, "Grate", iron, new Vector3(0f, 0.08f, 0f), new Vector3(0.7f, 0.06f, 0.7f));
            staged.Add(2, 0, grate, "rise", null);
            Transform bonfire = Group(visual.transform, "Bonfire");
            GameObject second = Prop.Primitive(PrimitiveType.Sphere, FortWorld.Look.Fire, bonfire, false);
            second.name = "Bonfire";
            second.transform.localScale = Prop.SphereScale(FortWorld.Config.fire.flameDiameter * 0.7f);
            second.transform.localPosition = new Vector3(0f, FortWorld.Config.fire.flameHeight + 0.35f, 0f);
            staged.Add(2, 1, bonfire, "drop", null);
            Transform sparks = Group(visual.transform, "SparkStones");
            Cube(sparks, "Stone", iron, new Vector3(0.4f, 0.2f, 0.4f), new Vector3(0.16f, 0.16f, 0.16f));
            staged.Add(2, 2, sparks, "drop", null);
            Transform crown = Group(visual.transform, "Crown");
            Cylinder(crown, "Crown", iron, new Vector3(0f, 0.28f, 0f), 1.15f, 0.08f);
            staged.Add(2, 3, crown, "drop", null);
            return staged;
        }

        static void FillBoards(Transform parent, Material material, Vector3 size, float y, float boardHeight)
        {
            int boards = 8;
            float span = size.x * 0.82f;
            for (int i = 0; i < boards; i++)
            {
                float x = Mathf.Lerp(-span * 0.5f, span * 0.5f, boards == 1 ? 0.5f : i / (float)(boards - 1));
                Cube(parent, "Plank", material, new Vector3(x, y, 0.02f), new Vector3(span / boards * 0.82f, boardHeight, 0.08f));
            }
        }

        static void Park(Transform piece, Transform visual)
        {
            if (piece == null)
                return;
            piece.SetParent(visual, true);
        }

        static GameObject Child(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        static Transform Group(Transform parent, string name)
        {
            return Child(parent, name).transform;
        }

        static void Cube(Transform parent, string name, Material material, Vector3 position, Vector3 scale)
        {
            GameObject go = Prop.Primitive(PrimitiveType.Cube, material, parent, false);
            go.name = name;
            go.transform.localPosition = position;
            go.transform.localScale = scale;
        }

        static void Cylinder(Transform parent, string name, Material material, Vector3 position, float diameter, float height)
        {
            GameObject go = Prop.Primitive(PrimitiveType.Cylinder, material, parent, false);
            go.name = name;
            go.transform.localPosition = position;
            go.transform.localScale = Prop.CylinderScale(diameter, height);
        }
    }
}
