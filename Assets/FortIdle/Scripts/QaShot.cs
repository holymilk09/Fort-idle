using System.Collections;
using UnityEngine;

namespace FortIdle
{
    // One built night-5 fight, plus a gameplay view for the HUD controls.
    public class QaShot : MonoBehaviour
    {
        FollowCamera rig;

        public static bool Requested()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-qashot")
                    return true;
            }

            return false;
        }

        public void Init(FollowCamera camera)
        {
            rig = camera;
        }

        IEnumerator Start()
        {
            if (FortWorld.Match != null)
                FortWorld.Match.ClockHeld = true;
            yield return null;
            if (rig != null && FortWorld.Hero != null)
                rig.Frame(FortWorld.Hero.position, 1f, 0f);
            yield return new WaitForSeconds(0.8f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot("/opt/cursor/artifacts/hud-controls-clear.png");
            Debug.Log("FORTIDLE qashot=hud");

            Restore(FortWorld.Wall);
            Restore(FortWorld.Gate);
            Restore(FortWorld.TurretPad);
            if (FortWorld.Match != null)
            {
                FortWorld.Match.ShowNight(5);
                FortWorld.Match.SpawnShownNight();
            }

            float wait = 0f;
            while (wait < 14f && !HordeClose())
            {
                wait += Time.deltaTime;
                yield return null;
            }

            if (rig != null && FortWorld.Wall != null)
                rig.Frame(FortWorld.Wall.transform.position + new Vector3(0f, 0f, 2.4f), 1.55f, 0f);
            yield return new WaitForSeconds(0.8f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot("/opt/cursor/artifacts/night5-midfight.png");
            Debug.Log("FORTIDLE qashot=fight horde=" + (FortWorld.Match != null ? FortWorld.Match.LivingMonsters : 0));
            TriReport.Log("fight");

            FrameMeter meter = FrameMeter.Current;
            if (meter != null)
                meter.Begin("night5");
            yield return new WaitForSeconds(4f);
            if (meter != null)
                meter.End("night5");
            TriReport.Log("fight-end");
            Debug.Log("FORTIDLE qashot=done");
            Application.Quit();
        }

        static void Restore(BuildPad pad)
        {
            if (pad == null)
                return;
            pad.Restore(0, pad.MaxHitPoints > 0f ? pad.MaxHitPoints : 1f, true);
        }

        static bool HordeClose()
        {
            if (FortWorld.Match == null || FortWorld.Match.LivingMonsters < 8)
                return false;
            for (int i = 0; i < FortWorld.Monsters.Count; i++)
            {
                Monster monster = FortWorld.Monsters[i];
                if (monster != null && monster.Alive && monster.transform.position.z < 9f)
                    return true;
            }

            return false;
        }
    }
}
