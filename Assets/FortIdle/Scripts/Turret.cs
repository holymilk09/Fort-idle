using UnityEngine;

namespace FortIdle
{
    public class Turret : MonoBehaviour
    {
        Transform barrel;
        float cooldown;
        bool loggedShot;
        float range;
        float rate;
        float damage;
        float rateScale = 1f;
        int pierce = 1;
        float splash;
        bool fireBolt;
        float burnDamage;
        float burnSeconds;

        public int Level { get; private set; } = 1;

        public float DamagePerSecond
        {
            get
            {
                float oil = 1f;
                if (FortWorld.Progress != null)
                    oil += FortWorld.Progress.SapOil * FortWorld.Config.progression.sapOilBonus;
                return damage * rate * rateScale * oil;
            }
        }

        public void Init(Transform barrelTransform)
        {
            FortIdleConfig.TurretTuning tuning = FortWorld.Config.turret;
            Init(barrelTransform, tuning.range, tuning.shotsPerSecond, tuning.damage);
        }

        public void Init(Transform barrelTransform, float shotRange, float shotsPerSecond, float shotDamage)
        {
            barrel = barrelTransform;
            range = shotRange;
            rate = shotsPerSecond;
            damage = shotDamage;
            cooldown = 0f;
            enabled = false;
        }

        public void MultiplyRate(float factor)
        {
            if (factor > rateScale)
                rateScale = factor;
        }

        public void ApplyLevel(int next)
        {
            FortIdleConfig.DefenseTuning defenses = FortWorld.Config.defenses;
            if (next >= 2 && Level < 2)
            {
                range = Mathf.Max(range, defenses.turretLv2Range);
                MultiplyRate(defenses.turretRateMultiplier);
                pierce = Mathf.Max(1, defenses.turretPierce);
                Level = 2;
            }

            if (next >= 3 && Level < 3)
            {
                range = Mathf.Max(range, defenses.turretLv3Range);
                if (defenses.turretLv3RateScale > rateScale)
                    rateScale = defenses.turretLv3RateScale;
                splash = defenses.turretSplashRadius;
                fireBolt = true;
                burnDamage = defenses.towerBurnDamage;
                burnSeconds = defenses.towerBurnSeconds;
                Level = 3;
            }

            Dress();
        }

        public void RankUp(float nextRange, float rateFactor, float damageFactor, bool burning)
        {
            if (nextRange > range)
                range = nextRange;
            MultiplyRate(rateFactor);
            if (damageFactor > 0f)
                damage *= damageFactor;
            if (burning)
            {
                fireBolt = true;
                burnDamage = FortWorld.Config.defenses.towerBurnDamage;
                burnSeconds = FortWorld.Config.defenses.towerBurnSeconds;
            }

            Level += 1;
            Dress();
        }

        void Dress()
        {
            if (Level < 2 || transform.Find("Rank") != null || transform.Find("StageVisual") != null)
                return;
            Material bandMat = fireBolt ? FortWorld.Look.Fire : FortWorld.Look.Iron;
            GameObject band = Prop.Primitive(PrimitiveType.Cube, bandMat, transform, false);
            band.name = "Rank";
            band.transform.localScale = new Vector3(0.55f, 0.12f, 0.55f);
            band.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        }

        void Update()
        {
            FortIdleConfig config = FortWorld.Config;
            float delta = Time.deltaTime;
            Monster target = NearestInRange();
            if (target != null && barrel != null)
            {
                Vector3 to = target.Center - barrel.position;
                to.y = 0f;
                if (to.sqrMagnitude > 0f)
                {
                    Quaternion want = Quaternion.LookRotation(to.normalized, Vector3.up);
                    float blend = 1f - Mathf.Exp(-config.turret.yawSharpness * delta);
                    barrel.rotation = Quaternion.Slerp(barrel.rotation, want, blend);
                }
            }

            cooldown -= delta;
            if (target == null || cooldown > 0f)
                return;
            float shots = rate * rateScale;
            if (shots <= 0f)
                return;

            Fire(target);
            cooldown = 1f / shots;
            if (!loggedShot)
            {
                loggedShot = true;
                Debug.Log("FORTIDLE turret shot");
            }
        }

        Monster NearestInRange()
        {
            float best = range * range;
            Monster found = null;
            Vector3 origin = transform.position;
            for (int i = 0; i < FortWorld.Monsters.Count; i++)
            {
                Monster monster = FortWorld.Monsters[i];
                if (!monster.Alive)
                    continue;
                Vector3 delta = monster.Center - origin;
                delta.y = 0f;
                float sqr = delta.sqrMagnitude;
                if (sqr <= best)
                {
                    best = sqr;
                    found = monster;
                }
            }

            return found;
        }

        void Fire(Monster target)
        {
            FortIdleConfig config = FortWorld.Config;
            Vector3 muzzle = barrel.position + barrel.forward * config.turret.muzzleForward;
            Vector3 direction = target.Center - muzzle;
            if (direction.sqrMagnitude <= 0f)
                direction = barrel.forward;

            Material boltMaterial = fireBolt ? FortWorld.Look.Fire : FortWorld.Look.Bolt;
            GameObject go = FortPool.Take(310, () =>
            {
                GameObject made = Prop.Primitive(PrimitiveType.Sphere, boltMaterial, null, false);
                made.name = "Bolt";
                made.transform.localScale = Prop.SphereScale(config.turret.projectileRadius * 2f);
                return made;
            });
            go.transform.position = muzzle;
            Bolt bolt = go.GetComponent<Bolt>();
            if (bolt == null)
                bolt = go.AddComponent<Bolt>();
            float oil = 1f;
            if (FortWorld.Progress != null)
                oil += FortWorld.Progress.SapOil * config.progression.sapOilBonus;
            bolt.Init(direction, damage * oil, pierce, splash, burnDamage, burnSeconds);
        }
    }
}
