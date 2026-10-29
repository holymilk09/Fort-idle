using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FortIdle
{
    public class FollowCamera : MonoBehaviour
    {
        enum Mode
        {
            Follow,
            Out,
            Hold,
            Return
        }

        Transform target;
        Vector3 followVelocity;
        Vector3 lookVelocity;
        Vector3 lookPoint;
        bool snapped;
        Mode mode;
        float teaseAge;
        Vector3 fromPos;
        Vector3 fromLook;
        Vector3 toPos;
        Vector3 toLook;
        bool frameLock;
        Vector3 nudge;
        float nudgeLeft;
        float nudgeDuration;
        float pullback;
        float pullbackVelocity;
        Monster bossNear;
        float bossPeak;

        public bool Teasing => mode != Mode.Follow;

        public void Configure(Camera camera)
        {
            FortIdleConfig.CameraTuning tuning = FortWorld.Config.cameraRig;
            camera.fieldOfView = tuning.fieldOfView;
            camera.nearClipPlane = tuning.nearClip;
            camera.farClipPlane = tuning.farClip;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = FortWorld.Config.presentation.daySky;
            UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
            data.renderShadows = FortLook.SoftShadows;
            data.renderPostProcessing = true;
        }

        public void SetTarget(Transform hero)
        {
            target = hero;
            snapped = false;
        }

        public void BeginTease(Vector3 worldPoint)
        {
            if (mode != Mode.Follow)
                return;

            FortIdleConfig.CameraTuning tuning = FortWorld.Config.cameraRig;
            fromPos = transform.position;
            fromLook = lookPoint;
            toLook = worldPoint + Vector3.up * tuning.teaseLookHeight;
            toPos = worldPoint + Offset(tuning);
            mode = Mode.Out;
            teaseAge = 0f;
            frameLock = false;
            Debug.Log("FORTIDLE tease=start");
        }

        public void Frame(Vector3 worldPoint, float distanceScale)
        {
            Frame(worldPoint, distanceScale, 0f);
        }

        public void Frame(Vector3 worldPoint, float distanceScale, float extraYaw)
        {
            FortIdleConfig.CameraTuning tuning = FortWorld.Config.cameraRig;
            if (distanceScale < 0.2f)
                distanceScale = 0.2f;
            fromPos = transform.position;
            fromLook = lookPoint;
            toLook = worldPoint + Vector3.up * 1.05f;
            Vector3 offset = Offset(tuning) * distanceScale;
            if (Mathf.Abs(extraYaw) > 0.1f)
                offset = Quaternion.AngleAxis(extraYaw, Vector3.up) * offset;
            toPos = worldPoint + offset;
            mode = Mode.Out;
            teaseAge = 0f;
            frameLock = true;
        }

        public void Nudge(Vector3 worldPoint)
        {
            FortIdleConfig.StageTuning tune = FortWorld.Config.stages;
            Vector3 toward = worldPoint - transform.position;
            toward.y *= 0.25f;
            if (toward.sqrMagnitude < 0.01f)
                return;
            float meters = tune.nudgeMeters;
            nudge = toward.normalized * meters;
            nudgeDuration = tune.nudgeSeconds <= 0f ? 0.2f : tune.nudgeSeconds;
            nudgeLeft = nudgeDuration;
        }

        void LateUpdate()
        {
            if (target == null)
                return;

            if (mode == Mode.Follow)
            {
                Follow();
                return;
            }

            Tease();
        }

        void Follow()
        {
            FortIdleConfig.CameraTuning tuning = FortWorld.Config.cameraRig;
            UpdatePullback(tuning);
            Vector3 want = target.position + Offset(tuning) * (1f + pullback);
            Vector3 look = target.position + Vector3.up * tuning.lookHeight;
            if (!snapped || tuning.followSmoothTime <= 0f)
            {
                transform.position = want;
                lookPoint = look;
                snapped = true;
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, want, ref followVelocity, tuning.followSmoothTime);
                lookPoint = tuning.lookSmoothTime <= 0f
                    ? look
                    : Vector3.SmoothDamp(lookPoint, look, ref lookVelocity, tuning.lookSmoothTime);
            }

            transform.LookAt(lookPoint);
            ApplyNudge();
            WatchBoss();
        }

        void UpdatePullback(FortIdleConfig.CameraTuning tuning)
        {
            Monster boss = NearBoss(tuning.bossCamRange);
            if (boss != bossNear)
            {
                if (bossNear != null)
                    Debug.Log("FORTIDLE boss-frame peak=" + bossPeak.ToString("0.00") + " pullback=" + pullback.ToString("0.00"));
                bossNear = boss;
                bossPeak = 0f;
                if (boss != null)
                    Debug.Log("FORTIDLE boss-cam=near");
            }

            float goal = boss != null && tuning.bossCamPullback > 0f ? tuning.bossCamPullback : 0f;
            if (!snapped || tuning.bossCamBlendTime <= 0f)
            {
                pullback = goal;
                pullbackVelocity = 0f;
                return;
            }

            pullback = Mathf.SmoothDamp(pullback, goal, ref pullbackVelocity, tuning.bossCamBlendTime);
        }

        Monster NearBoss(float range)
        {
            if (range <= 0f)
                return null;
            Monster best = null;
            float bestDistance = range;
            for (int i = 0; i < FortWorld.Monsters.Count; i++)
            {
                Monster monster = FortWorld.Monsters[i];
                if (monster == null || !monster.IsBoss)
                    continue;
                float distance = FortMath.HorizontalDistance(monster.transform.position, target.position);
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = monster;
                }
            }

            return best;
        }

        void WatchBoss()
        {
            if (bossNear == null)
                return;
            float fraction = bossNear.ScreenFraction(FortWorld.Camera);
            if (fraction > bossPeak)
                bossPeak = fraction;
        }

        void ApplyNudge()
        {
            if (nudgeLeft <= 0f)
                return;
            nudgeLeft -= Time.deltaTime;
            float duration = nudgeDuration <= 0f ? 0.2f : nudgeDuration;
            float u = Mathf.Clamp01(1f - nudgeLeft / duration);
            float bell = Mathf.Sin(u * Mathf.PI);
            transform.position += nudge * bell;
        }

        void Tease()
        {
            FortIdleConfig.CameraTuning tuning = FortWorld.Config.cameraRig;
            teaseAge += Time.deltaTime;
            float duration = mode == Mode.Out
                ? (frameLock ? 0.4f : tuning.teaseDuration)
                : mode == Mode.Hold ? tuning.teaseHold : tuning.teaseReturnDuration;
            float u = duration <= 0f ? 1f : Mathf.Clamp01(teaseAge / duration);
            float smooth = u * u * (3f - 2f * u);
            if (mode == Mode.Hold)
            {
                transform.position = toPos;
                lookPoint = toLook;
            }
            else if (mode == Mode.Out)
            {
                transform.position = Vector3.Lerp(fromPos, toPos, smooth);
                lookPoint = Vector3.Lerp(fromLook, toLook, smooth);
            }
            else
            {
                Vector3 backPos = target.position + Offset(tuning) * (1f + pullback);
                Vector3 backLook = target.position + Vector3.up * tuning.lookHeight;
                transform.position = Vector3.Lerp(toPos, backPos, smooth);
                lookPoint = Vector3.Lerp(toLook, backLook, smooth);
            }

            transform.LookAt(lookPoint);
            ApplyNudge();
            if (u < 1f)
                return;

            teaseAge = 0f;
            if (mode == Mode.Out)
            {
                mode = Mode.Hold;
                Debug.Log("FORTIDLE tease=hold");
            }
            else if (mode == Mode.Hold && frameLock)
            {
                teaseAge = 0f;
            }
            else if (mode == Mode.Hold)
            {
                mode = Mode.Return;
            }
            else
            {
                mode = Mode.Follow;
                snapped = true;
                followVelocity = Vector3.zero;
                lookVelocity = Vector3.zero;
                Debug.Log("FORTIDLE tease=done");
            }
        }

        public static Vector3 Offset(FortIdleConfig.CameraTuning tuning)
        {
            float pitch = tuning.pitchDegrees * Mathf.Deg2Rad;
            Vector3 local = new Vector3(0f, Mathf.Sin(pitch), -Mathf.Cos(pitch)) * tuning.distance;
            return Quaternion.AngleAxis(tuning.yawDegrees, Vector3.up) * local;
        }
    }
}
