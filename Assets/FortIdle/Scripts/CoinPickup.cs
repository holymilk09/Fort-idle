using UnityEngine;

namespace FortIdle
{
    public enum DropKind
    {
        Coin,
        Iron,
        Meat,
        Bone,
        Hide,
        Core,
        Sap
    }

    public class CoinPickup : MonoBehaviour
    {
        static bool loggedFirst;

        Vector3 from;
        Vector3 rest;
        float age;
        bool settled;
        DropKind kind = DropKind.Coin;

        public int payout = 1;

        public static void Scatter(Vector3 origin)
        {
            FortIdleConfig config = FortWorld.Config;
            int count = config != null ? config.loot.coinsPerKill : 1;
            ScatterCoins(origin, count);
        }

        public static void ScatterCoins(Vector3 origin, int coins)
        {
            FortIdleConfig config = FortWorld.Config;
            if (config == null || FortWorld.Look == null || coins <= 0)
                return;

            int shown = coins < 12 ? coins : 12;
            int each = coins / shown;
            int extra = coins - each * shown;
            for (int i = 0; i < shown; i++)
            {
                float turns = shown <= 1 ? 0f : (i + 0.5f) / shown;
                float angle = turns * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * config.loot.scatterRadius;
                Vector3 restPoint = origin + offset;
                restPoint.y = config.loot.restHeight;
                Vector3 start = origin + Vector3.up * config.loot.dropHeight;

                int pay = each + (i < extra ? 1 : 0);
                GameObject coin = FortPool.Take(200, () =>
                {
                    GameObject made = Prop.Primitive(PrimitiveType.Cylinder, FortWorld.Look.Coin, null, false);
                    made.name = "Coin";
                    made.transform.localScale = Prop.CylinderScale(config.loot.coinDiameter, config.loot.coinThickness);
                    made.AddComponent<CoinPickup>();
                    return made;
                });
                coin.transform.position = start;
                CoinPickup pickup = coin.GetComponent<CoinPickup>();
                pickup.Arm(start, restPoint, DropKind.Coin, pay);
                FortWorld.Coins.Add(pickup);
            }
        }

        public static void ScatterIron(Vector3 origin, int count)
        {
            FortIdleConfig config = FortWorld.Config;
            if (config == null || count <= 0 || FortWorld.Look == null)
                return;

            for (int i = 0; i < count; i++)
            {
                float turns = count <= 1 ? 0f : (i + 0.5f) / count;
                float angle = turns * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * config.loot.scatterRadius;
                Vector3 restPoint = origin + offset;
                restPoint.y = config.loot.restHeight;
                Vector3 start = origin + Vector3.up * config.loot.dropHeight;

                GameObject bar = FortArt.Box(null, "IronRock", config.supplies.ironBarSize, FortArt.IronTint, 0.72f, true, false);
                if (bar == null)
                {
                    bar = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Iron, null, false);
                    bar.transform.localScale = config.supplies.ironBarSize;
                }

                bar.name = "Iron";
                bar.transform.position = start;
                CoinPickup pickup = bar.AddComponent<CoinPickup>();
                pickup.from = start;
                pickup.rest = restPoint;
                pickup.kind = DropKind.Iron;
                FortWorld.Coins.Add(pickup);
            }
        }

        public static void ScatterLoot(Vector3 origin, DropKind drop, int count)
        {
            FortIdleConfig config = FortWorld.Config;
            if (config == null || count <= 0 || FortWorld.Look == null)
                return;
            for (int i = 0; i < count; i++)
                SpawnOne(origin, drop, i, count);
        }

        static void SpawnOne(Vector3 origin, DropKind drop, int index, int count)
        {
            FortIdleConfig config = FortWorld.Config;
            float turns = count <= 1 ? 0f : (index + 0.5f) / count;
            float angle = turns * Mathf.PI * 2f;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * config.loot.scatterRadius;
            Vector3 restPoint = origin + offset;
            restPoint.y = config.loot.restHeight;
            Vector3 start = origin + Vector3.up * config.loot.dropHeight;

            Material material = FortWorld.Look.Coin;
            PrimitiveType shape = PrimitiveType.Cylinder;
            Vector3 scale = Prop.CylinderScale(config.loot.coinDiameter, config.loot.coinThickness);
            if (drop == DropKind.Iron)
            {
                ScatterIron(origin, 1);
                return;
            }

            if (drop == DropKind.Meat)
            {
                material = FortWorld.Look.Meat;
                shape = PrimitiveType.Cube;
                scale = new Vector3(0.28f, 0.16f, 0.22f);
            }
            else if (drop == DropKind.Bone)
            {
                material = FortWorld.Look.Bone;
                shape = PrimitiveType.Cube;
                scale = new Vector3(0.12f, 0.12f, 0.42f);
            }
            else if (drop == DropKind.Hide)
            {
                material = FortWorld.Look.Hide;
                shape = PrimitiveType.Cube;
                scale = new Vector3(0.34f, 0.08f, 0.28f);
            }
            else if (drop == DropKind.Core)
            {
                material = FortWorld.Look.Core;
                shape = PrimitiveType.Sphere;
                scale = Prop.SphereScale(0.28f);
            }
            else if (drop == DropKind.Sap)
            {
                material = FortWorld.Look.Sap;
                shape = PrimitiveType.Sphere;
                scale = Prop.SphereScale(0.24f);
            }

            int key = 200 + (int)drop;
            PrimitiveType madeShape = shape;
            Material madeMaterial = material;
            Vector3 madeScale = scale;
            GameObject go = FortPool.Take(key, () =>
            {
                GameObject made = Prop.Primitive(madeShape, madeMaterial, null, false);
                made.name = drop.ToString();
                made.transform.localScale = madeScale;
                made.AddComponent<CoinPickup>();
                return made;
            });
            go.transform.position = start;
            CoinPickup pickup = go.GetComponent<CoinPickup>();
            pickup.Arm(start, restPoint, drop, 1);
            FortWorld.Coins.Add(pickup);
        }

        public void Arm(Vector3 start, Vector3 restPoint, DropKind drop, int coins)
        {
            from = start;
            rest = restPoint;
            kind = drop;
            payout = coins < 1 ? 1 : coins;
            age = 0f;
            settled = false;
            transform.position = start;
        }

        void Update()
        {
            FortIdleConfig config = FortWorld.Config;
            if (config == null)
                return;

            float delta = Time.deltaTime;
            if (!settled)
            {
                age += delta;
                float span = config.loot.popDuration <= 0f ? 1f : config.loot.popDuration;
                float u = Mathf.Clamp01(age / span);
                float arc = 4f * u * (1f - u) * config.loot.popArcHeight;
                transform.position = Vector3.Lerp(from, rest, u) + Vector3.up * arc;
                if (u >= 1f)
                {
                    settled = true;
                    transform.position = rest;
                }
            }

            transform.Rotate(0f, config.loot.spinDegreesPerSecond * delta, 0f, Space.World);

            if (FortWorld.Hero == null)
                return;

            float distance = FortMath.HorizontalDistance(transform.position, FortWorld.Hero.position);
            if (settled && distance <= config.loot.magnetRadius && distance > config.loot.pickupRadius)
            {
                Vector3 target = FortWorld.Hero.position;
                target.y = config.loot.restHeight;
                transform.position = Vector3.MoveTowards(transform.position, target, config.loot.magnetSpeed * delta);
                distance = FortMath.HorizontalDistance(transform.position, FortWorld.Hero.position);
            }

            if (distance <= config.loot.pickupRadius)
                Collect();
        }

        void Collect()
        {
            if (FortWorld.Progress != null)
            {
                if (kind == DropKind.Iron)
                    FortWorld.Progress.AddIron(1);
                else if (kind == DropKind.Meat)
                    FortWorld.Progress.AddMeat(1);
                else if (kind == DropKind.Bone)
                    FortWorld.Progress.AddBones(1);
                else if (kind == DropKind.Hide)
                    FortWorld.Progress.AddHide(1);
                else if (kind == DropKind.Core)
                    FortWorld.Progress.AddCores(1);
                else if (kind == DropKind.Sap)
                    FortWorld.Progress.AddSap(1);
                else
                    FortWorld.Progress.AddCoins(payout < 1 ? 1 : payout);
            }

            if (FortWorld.Hud != null)
                FortWorld.Hud.Fly(transform.position, kind);
            if (!loggedFirst)
            {
                loggedFirst = true;
                int coins = FortWorld.Progress != null ? FortWorld.Progress.Coins : 0;
                Debug.Log("FORTIDLE coins=" + coins);
            }

            FortWorld.Coins.Remove(this);
            FortPool.Give(200 + (int)kind, gameObject);
        }
    }
}
