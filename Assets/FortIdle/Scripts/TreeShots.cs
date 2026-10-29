using System.Collections;
using UnityEngine;

namespace FortIdle
{
    // Low shots of the treeline, day and night, then the gameplay camera.
    public class TreeShots : MonoBehaviour
    {
        FollowCamera rig;
        Camera view;

        public static bool Requested()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-treeshot")
                    return true;
            }

            return false;
        }

        public void Init(FollowCamera camera)
        {
            rig = camera;
            view = camera != null ? camera.GetComponent<Camera>() : FortWorld.Camera;
        }

        IEnumerator Start()
        {
            if (FortWorld.Match != null)
                FortWorld.Match.ClockHeld = true;
            yield return null;
            if (rig != null)
                rig.enabled = false;
            Look(new Vector3(-1.6f, 0.85f, 5.2f), new Vector3(0.6f, 1.45f, 10.5f));
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(PoseShot.ShotDir(), "tree-day-low.png"));
            Debug.Log("FORTIDLE treeshot=day-low");
            yield return new WaitForSeconds(8f);

            if (FortWorld.Match != null)
                FortWorld.Match.ShowNight(5);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(PoseShot.ShotDir(), "tree-night5-low.png"));
            Debug.Log("FORTIDLE treeshot=night-low");
            yield return new WaitForSeconds(8f);

            if (rig != null)
            {
                rig.enabled = true;
                rig.Frame(FortWorld.Hero != null ? FortWorld.Hero.position : Vector3.zero, 1f, 0f);
            }

            yield return new WaitForSeconds(1.2f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(PoseShot.ShotDir(), "tree-gameplay.png"));
            Debug.Log("FORTIDLE treeshot=gameplay");
            yield return new WaitForSeconds(6f);
            if (rig != null)
                rig.enabled = false;
            if (view != null)
                view.fieldOfView = 64f;
            Look(new Vector3(0f, 6.6f, 0.5f), new Vector3(0f, 0.7f, 3.6f));
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(PoseShot.ShotDir(), "wall-gate-labels.png"));
            Debug.Log("FORTIDLE treeshot=pads");
            yield return new WaitForSeconds(2f);
            Debug.Log("FORTIDLE treeshot=done");
        }

        void Look(Vector3 from, Vector3 at)
        {
            if (view == null)
                return;
            view.transform.position = from;
            view.transform.LookAt(at);
        }
    }
}
