using System.Collections;
using System.IO;
using UnityEngine;

namespace FortIdle
{
    // Evidence only. Uses the gameplay camera as it already is.
    public class PoseShot : MonoBehaviour
    {
        public static bool Requested()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-poseshot")
                    return true;
            }

            return false;
        }

        // -shotdir <path> picks the output folder; otherwise the player's persistent data folder.
        public static string ShotDir()
        {
            string dir = Application.persistentDataPath;
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-shotdir" && !string.IsNullOrEmpty(args[i + 1]))
                    dir = args[i + 1];
            }

            Directory.CreateDirectory(dir);
            return dir;
        }

        void Awake()
        {
            if (FortWorld.Match != null)
                FortWorld.Match.ClockHeld = true;
            WorkerAgent.SpawnNext();
        }

        IEnumerator Start()
        {
            string dir = ShotDir();
            Debug.Log("FORTIDLE poseshot dir=" + dir);
            yield return new WaitForSeconds(1.25f);
            if (FortWorld.Hero != null)
            {
                FortWorld.Hero.position = new Vector3(0f, 0.15f, 5.5f);
                FortWorld.Hero.localScale = new Vector3(2.4f, 2.4f, 2.4f);
            }
            yield return new WaitForSeconds(0.8f);
            yield return new WaitForEndOfFrame();
            Texture2D screen = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(dir, "hero-idle-n1.png"), screen.EncodeToPNG());
            Destroy(screen);
            Debug.Log("FORTIDLE poseshot=idle");
            if (FortWorld.Hero != null)
                FortWorld.Hero.localScale = Vector3.one;
            Vector3 hero = FortWorld.Hero != null ? FortWorld.Hero.position : Vector3.zero;
            Renderer body = FortWorld.Hero != null ? FortWorld.Hero.GetComponentInChildren<Renderer>() : null;
            Vector3 focus = body != null ? body.bounds.center : hero + Vector3.up;
            Debug.Log("FORTIDLE hero-focus " + focus.ToString("0.00") + " size " + (body != null ? body.bounds.size.ToString("0.00") : "none"));
            if (body != null)
            {
                SkinnedMeshRenderer skin = body as SkinnedMeshRenderer;
                if (skin == null)
                    skin = FortWorld.Hero.GetComponentInChildren<SkinnedMeshRenderer>();
                if (skin != null)
                    skin.updateWhenOffscreen = true;
            }

            Camera source = FortWorld.Camera;
            if (source != null)
            {
                GameObject host = new GameObject("IdleCam");
                Camera idle = host.AddComponent<Camera>();
                idle.CopyFrom(source);
                Vector3 pos = focus + new Vector3(1.35f, 0.55f, 1.35f);
                Quaternion rot = Quaternion.LookRotation(focus - pos, Vector3.up);
                idle.transform.SetPositionAndRotation(pos, rot);
                idle.fieldOfView = 45f;
                idle.nearClipPlane = 0.05f;
                idle.clearFlags = CameraClearFlags.SolidColor;
                idle.backgroundColor = new Color(0.55f, 0.7f, 0.8f);
                RenderTexture rt = new RenderTexture(1280, 720, 24);
                idle.targetTexture = rt;
                idle.Render();
                RenderTexture.active = rt;
                Texture2D shot = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                shot.ReadPixels(new Rect(0f, 0f, 1280f, 720f), 0, 0);
                shot.Apply();
                File.WriteAllBytes(Path.Combine(dir, "hero-idle-close.png"), shot.EncodeToPNG());
                RenderTexture.active = null;
                Destroy(shot);
                Destroy(rt);
                Destroy(host);
            }

            Debug.Log("FORTIDLE poseshot=idle");

            if (FortWorld.Match != null)
                FortWorld.Match.ShowNight(5);
            GameObject treant = new GameObject("Treant");
            treant.transform.position = new Vector3(0f, 0f, 8f);
            Monster boss = treant.AddComponent<Monster>();
            boss.Init(MonsterKind.Treant, 0, 1);
            FortWorld.Monsters.Add(boss);
            yield return new WaitForSeconds(4.2f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(dir, "boss-bar-n5.png"));
            Debug.Log("FORTIDLE poseshot=boss frame=" + boss.ScreenFraction(FortWorld.Camera).ToString("0.00"));
            yield return new WaitForSeconds(0.4f);
            Application.Quit();
        }
    }
}
