using System.Collections.Generic;
using UnityEngine;

namespace FortIdle
{
    public class Bolt : MonoBehaviour
    {
        readonly List<Monster> hit = new List<Monster>();
        Vector3 direction;
        float damage;
        float age;
        int pierce = 1;
        float splash;
        float burnDamage;
        float burnSeconds;

        public void Init(Vector3 travelDirection, float shotDamage)
        {
            Init(travelDirection, shotDamage, 1, 0f, 0f, 0f);
        }

        public void Init(Vector3 travelDirection, float shotDamage, int pierceHits, float splashRadius, float burn, float burnTime)
        {
            direction = travelDirection.sqrMagnitude > 0f ? travelDirection.normalized : Vector3.forward;
            damage = shotDamage;
            pierce = pierceHits < 1 ? 1 : pierceHits;
            splash = splashRadius;
            burnDamage = burn;
            burnSeconds = burnTime;
            age = 0f;
            hit.Clear();
        }

        void Update()
        {
            FortIdleConfig config = FortWorld.Config;
            float delta = Time.deltaTime;
            age += delta;
            Vector3 start = transform.position;
            Vector3 end = start + direction * config.turret.projectileSpeed * delta;
            transform.position = end;
            if (age >= config.turret.projectileLifetime)
            {
                FortPool.Give(310, gameObject);
                return;
            }

            float hitRadius = config.turret.projectileHitRadius;
            for (int i = 0; i < FortWorld.Monsters.Count; i++)
            {
                Monster monster = FortWorld.Monsters[i];
                if (!monster.Alive || hit.Contains(monster))
                    continue;
                if (!FortMath.SegmentHitsSphere(start, end, monster.Center, hitRadius))
                    continue;
                Strike(monster);
                pierce -= 1;
                if (pierce > 0)
                    continue;
                Splash(monster.Center);
                FortPool.Give(310, gameObject);
                return;
            }
        }

        void Strike(Monster monster)
        {
            hit.Add(monster);
            monster.TakeDamage(damage);
            if (burnDamage > 0f)
                monster.Burn(burnDamage, burnSeconds);
        }

        void Splash(Vector3 center)
        {
            if (splash <= 0f)
                return;
            for (int i = 0; i < FortWorld.Monsters.Count; i++)
            {
                Monster monster = FortWorld.Monsters[i];
                if (!monster.Alive || hit.Contains(monster))
                    continue;
                if (FortMath.HorizontalDistance(monster.Center, center) > splash)
                    continue;
                Strike(monster);
            }
        }
    }
}
