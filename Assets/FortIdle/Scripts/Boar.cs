using UnityEngine;

namespace FortIdle
{
    public class Boar : MonoBehaviour
    {
        public bool Alive => alive;

        Vector3 home;
        Vector3 wander;
        float hitPoints;
        float maxHitPoints;
        float respawn;
        bool alive = true;
        HpBar bar;

        public void Init(Vector3 origin)
        {
            home = origin;
            transform.position = origin;
            maxHitPoints = FortWorld.Config.drops.boarHitPoints;
            hitPoints = maxHitPoints;
            PickWander();
            BuildBody();
            bar = HpBar.Create(transform, 0.85f, 0.7f);
            bar.Read = () => maxHitPoints <= 0f ? 0f : hitPoints / maxHitPoints;
        }

        public void TakeDamage(float amount)
        {
            if (!alive || amount <= 0f)
                return;
            hitPoints -= amount;
            if (hitPoints > 0f)
                return;
            alive = false;
            hitPoints = 0f;
            CoinPickup.ScatterLoot(transform.position, DropKind.Meat, FortWorld.Config.drops.boarMeat);
            respawn = FortWorld.Config.drops.boarRespawnSeconds;
            if (transform.childCount > 0)
                transform.GetChild(0).gameObject.SetActive(false);
        }

        void BuildBody()
        {
            GameObject body = Prop.Primitive(PrimitiveType.Capsule, FortWorld.Look.Boar, transform, false);
            body.name = "Body";
            body.transform.localScale = new Vector3(0.55f, 0.28f, 0.7f);
            body.transform.localPosition = new Vector3(0f, 0.32f, 0f);
            GameObject snout = Prop.Primitive(PrimitiveType.Cube, FortWorld.Look.Boar, body.transform, false);
            snout.name = "Snout";
            snout.transform.localScale = new Vector3(0.35f, 0.28f, 0.4f);
            snout.transform.localPosition = new Vector3(0f, 0.05f, 0.7f);
        }

        void Update()
        {
            FortIdleConfig config = FortWorld.Config;
            if (config == null)
                return;
            float delta = Time.deltaTime;
            if (!alive)
            {
                respawn -= delta;
                if (respawn > 0f)
                    return;
                if (FortWorld.Match != null && FortWorld.Match.Phase != MatchPhase.Day)
                    return;
                alive = true;
                hitPoints = maxHitPoints;
                transform.position = home;
                if (transform.childCount > 0)
                    transform.GetChild(0).gameObject.SetActive(true);
                PickWander();
                return;
            }

            if (FortWorld.Match != null && FortWorld.Match.Phase != MatchPhase.Day)
                return;

            Vector3 to = wander - transform.position;
            to.y = 0f;
            if (to.magnitude < 0.4f)
                PickWander();
            else
            {
                Vector3 step = to.normalized * config.drops.boarSpeed * delta;
                transform.position += step;
                transform.rotation = Quaternion.LookRotation(to.normalized, Vector3.up);
            }
        }

        void PickWander()
        {
            Vector2 ring = Random.insideUnitCircle * 3.2f;
            wander = home + new Vector3(ring.x, 0f, ring.y);
        }
    }
}
