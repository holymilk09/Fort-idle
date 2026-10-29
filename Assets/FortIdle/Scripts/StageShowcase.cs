using System.Collections;
using UnityEngine;

namespace FortIdle
{
    // Close look at every stage of the wall, gate, watchtower, and turret.
    public class StageShowcase : MonoBehaviour
    {
        FollowCamera rig;
        bool tested;

        public static bool Requested()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-stages")
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
            FortIdleConfig.StageTuning tune = FortWorld.Config.stages;
            Debug.Log("FORTIDLE stages wall=" + tune.wall
                + " gate=" + tune.gate
                + " tower=" + tune.tower
                + " turret=" + tune.turret
                + " spike=" + tune.spike
                + " fire=" + tune.fire
                + " hut=" + tune.hut
                + " wallUp=" + tune.wallUpgrade
                + " gateUp=" + tune.gateUpgrade
                + " towerUp=" + tune.towerUpgrade
                + " turretUp=" + tune.turretUpgrade);
            bool math = StagedBuild.Reached(20, 100, 5) == 1
                && StagedBuild.Reached(19, 100, 5) == 0
                && StagedBuild.Reached(8, 40, 5) == 1
                && StagedBuild.Reached(40, 40, 5) == 5
                && StagedBuild.Reached(0, 40, 5) == 0;
            Debug.Log(math ? "FORTIDLE stage-math ok" : "FORTIDLE stage-math fail");
            yield return null;
            ProvePartial();
            Quiet(FortWorld.Wall);
            Quiet(FortWorld.Gate);
            Quiet(FortWorld.Tower);
            Quiet(FortWorld.TurretPad);
            Quiet(FortWorld.Hut);
            yield return new WaitForSeconds(0.3f);
            Debug.Log("FORTIDLE stages=begin");
            yield return Pour(FortWorld.Wall, 0.78f, 0f);
            yield return Pour(FortWorld.Gate, 0.72f, 180f);
            yield return Pour(FortWorld.Tower, 0.62f, 28f);
            yield return Pour(FortWorld.TurretPad, 0.52f, 20f);
            Debug.Log("FORTIDLE stages=done");
        }

        void ProvePartial()
        {
            BuildPad hut = FortWorld.Hut;
            if (hut == null || tested)
                return;
            tested = true;
            for (int i = 0; i < 40; i++)
                hut.AddWood();
            int shown = hut.StageShown;
            FortSave.Blob blob = FortSave.Capture();
            hut.ConcealStages();
            hut.ReadSite(blob.hut);
            bool ok = blob.hut != null && blob.hut.wood == hut.PouredWood && hut.StageShown == shown && shown > 0;
            Debug.Log(ok ? "FORTIDLE stage-save ok shown=" + hut.StageShown : "FORTIDLE stage-save fail shown=" + hut.StageShown);
        }

        static void Quiet(BuildPad pad)
        {
            if (pad != null)
                pad.QuietLabel = true;
        }

        IEnumerator Pour(BuildPad pad, float distanceScale, float yaw)
        {
            if (pad == null)
                yield break;
            pad.SetUnlockNight(1);
            Debug.Log("FORTIDLE stage-pour " + pad.PadKind + " complete=" + (pad.IsComplete ? 1 : 0));
            if (rig != null)
                rig.Frame(pad.DeliveryPoint(), distanceScale, yaw);
            yield return new WaitForSeconds(0.85f);
            int shown = pad.StageShown;
            while (!pad.IsComplete)
            {
                Vector3 from = FortWorld.Hero != null ? FortWorld.Hero.position + Vector3.up * 1.1f : pad.transform.position;
                if (FortWorld.Stack != null && FortWorld.Stack.Count > 0)
                    from = FortWorld.Stack.TopWorld();
                if (pad.WantsWood)
                    pad.AddWood();
                else if (pad.WantsStone)
                    pad.AddStone();
                else if (pad.WantsIron)
                    pad.AddIron();
                else
                    break;
                Flyer.Spawn(from, pad.DeliveryPoint(), 0.16f, 0.65f, FortWorld.Look.Plank, FortWorld.Config.stack.plankSize);
                if (pad.StageShown != shown)
                {
                    shown = pad.StageShown;
                    yield return new WaitForSeconds(0.85f);
                }
                else
                    yield return new WaitForSeconds(0.03f);
            }

            yield return new WaitForSeconds(0.7f);
        }
    }
}
