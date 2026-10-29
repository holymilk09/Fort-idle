using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace FortIdle
{
    public class OfflineEarnings : MonoBehaviour
    {
        GameObject popup;
        int pendingCoins;
        int pendingPlanks;
        long pausedTicks;

        void Start()
        {
            float faked;
            if (FakedSeconds(out faked))
            {
                if (!PerfHarness.Requested())
                    Offer(faked, true);
                return;
            }

            if (Suppressed() || FortSave.SavedTicks <= 0)
                return;
            Offer(SecondsSince(FortSave.SavedTicks), false);
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                pausedTicks = DateTime.UtcNow.Ticks;
                Flush();
                return;
            }

            long since = pausedTicks;
            pausedTicks = 0;
            if (since > 0 && popup == null && !Suppressed())
                Offer(SecondsSince(since), false);
        }

        void OnApplicationQuit()
        {
            Flush();
        }

        void Flush()
        {
            if (popup != null)
                Collect();
            else if (!Suppressed())
                FortSave.WritePlayer();
        }

        static bool Suppressed()
        {
            return FortSave.DemoSession()
                || ElementSheet.Requested()
                || StageShowcase.Requested()
                || LookReel.Requested()
                || PerfHarness.Requested()
                || TreeShots.Requested()
                || QaShot.Requested()
                || PoseShot.Requested()
                || PoseCheck.Requested();
        }

        static bool FakedSeconds(out float seconds)
        {
            seconds = 0f;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-offline")
                    return float.TryParse(args[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out seconds);
            }

            return false;
        }

        static float SecondsSince(long ticks)
        {
            double seconds = (DateTime.UtcNow.Ticks - ticks) / (double)TimeSpan.TicksPerSecond;
            if (seconds < 0d)
                return 0f;
            return (float)seconds;
        }

        void Offer(float rawSeconds, bool faked)
        {
            FortIdleConfig config = FortWorld.Config;
            FortProgress progress = FortWorld.Progress;
            if (config == null || progress == null)
                return;
            if (config.offline == null)
                config.offline = new FortIdleConfig.OfflineTuning();
            FortIdleConfig.OfflineTuning tune = config.offline;
            if (!tune.enabled)
                return;

            float elapsed = Mathf.Clamp(rawSeconds, 0f, tune.capSeconds);
            int workers = progress.WorkersHired;
            if (faked && workers < tune.qaMinWorkers)
                workers = tune.qaMinWorkers;
            int coins = 0;
            int planks = 0;
            if (elapsed >= tune.minAwaySeconds)
            {
                coins = Mathf.FloorToInt(elapsed * workers * tune.coinsPerWorkerSecond);
                planks = Mathf.Min(Mathf.FloorToInt(elapsed * workers * tune.planksPerWorkerSecond), tune.maxPlanks);
                if (FortWorld.Stock == null)
                    planks = 0;
            }

            Debug.Log("FORTIDLE offline elapsed=" + Mathf.RoundToInt(rawSeconds)
                + " coins=" + coins
                + " planks=" + planks
                + " credited=" + Mathf.RoundToInt(elapsed)
                + " workers=" + workers
                + (faked ? " faked=1" : ""));
            if (coins <= 0 && planks <= 0)
                return;
            if (popup != null)
                Collect();
            pendingCoins = coins;
            pendingPlanks = planks;
            BuildPopup(config, elapsed);
        }

        void Collect()
        {
            FortProgress progress = FortWorld.Progress;
            int coins = pendingCoins;
            int planks = pendingPlanks;
            pendingCoins = 0;
            pendingPlanks = 0;
            if (popup != null)
            {
                Destroy(popup);
                popup = null;
            }

            if (progress == null)
                return;
            progress.AddCoins(coins);
            if (FortWorld.Stock != null)
            {
                for (int i = 0; i < planks; i++)
                    FortWorld.Stock.Deposit();
            }

            Debug.Log("FORTIDLE offline collected coins=" + coins + " planks=" + planks);
            progress.NoteReward("offline");
        }

        void WatchAd()
        {
            Debug.Log("FORTIDLE rv=offline_x3");
            Collect();
        }

        void BuildPopup(FortIdleConfig config, float elapsed)
        {
            FortIdleConfig.OfflineTuning tune = config.offline;
            FortIdleConfig.CopyTuning copy = config.copy;

            popup = new GameObject("OfflinePopup");
            Canvas canvas = popup.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = tune.sortingOrder;
            CanvasScaler scaler = popup.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(config.hud.referenceWidth, config.hud.referenceHeight);
            scaler.matchWidthOrHeight = config.hud.matchWidthOrHeight;
            popup.AddComponent<GraphicRaycaster>();

            Image dim = MakeBox(popup.transform, "Dim", tune.dimColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            dim.raycastTarget = true;

            Vector2 middle = new Vector2(0.5f, 0.5f);
            Image panel = MakeBox(popup.transform, "Panel", tune.panelColor, middle, middle, Vector2.zero, tune.panelSize);
            Transform body = panel.transform;
            float top = tune.panelSize.y * 0.5f;
            float line = tune.bodyFontSize * 1.4f;

            MakeLabel(body, "Title", tune.titleFontSize, new Vector2(0f, top - tune.titleFontSize * 1.1f), tune.panelSize.x).text = copy.offlineTitle;
            float y = top - tune.titleFontSize * 1.1f - line * 1.3f;
            MakeLabel(body, "Coins", tune.bodyFontSize, new Vector2(0f, y), tune.panelSize.x).text = "+" + pendingCoins + " " + copy.offlineCoins;
            if (pendingPlanks > 0)
            {
                y -= line;
                MakeLabel(body, "Planks", tune.bodyFontSize, new Vector2(0f, y), tune.panelSize.x).text = "+" + pendingPlanks + " " + copy.offlinePlanks;
            }

            y -= line;
            MakeLabel(body, "Away", tune.awayFontSize, new Vector2(0f, y), tune.panelSize.x).text = copy.offlineAway + " " + AwayText(copy, elapsed);

            float buttonY = -top + tune.buttonLift;
            if (tune.showAdButton)
            {
                float offset = (tune.buttonSize.x + tune.buttonGap) * 0.5f;
                MakeButton(body, "CollectButton", copy.offlineCollect, tune.buttonColor, new Vector2(-offset, buttonY), tune, Collect);
                MakeButton(body, "AdButton", copy.offlineTriple, tune.adButtonColor, new Vector2(offset, buttonY), tune, WatchAd);
            }
            else
            {
                MakeButton(body, "CollectButton", copy.offlineCollect, tune.buttonColor, new Vector2(0f, buttonY), tune, Collect);
            }
        }

        static string AwayText(FortIdleConfig.CopyTuning copy, float elapsed)
        {
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int hours = minutes / 60;
            minutes -= hours * 60;
            if (hours <= 0)
                return minutes + copy.offlineMinutes;
            return hours + copy.offlineHours + " " + minutes + copy.offlineMinutes;
        }

        static Image MakeBox(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchored, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchored;
            rect.sizeDelta = size;
            return image;
        }

        static Text MakeLabel(Transform parent, string name, int fontSize, Vector2 anchored, float width)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = FortWorld.Look.Font;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = FortWorld.Config.presentation.hudText;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchored;
            rect.sizeDelta = new Vector2(width, fontSize * 1.3f);
            return text;
        }

        static void MakeButton(Transform parent, string name, string caption, Color color, Vector2 anchored, FortIdleConfig.OfflineTuning tune, UnityEngine.Events.UnityAction action)
        {
            Vector2 middle = new Vector2(0.5f, 0.5f);
            Image image = MakeBox(parent, name, color, middle, middle, anchored, tune.buttonSize);
            image.raycastTarget = true;
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            Text label = MakeLabel(image.transform, "Label", tune.buttonFontSize, Vector2.zero, tune.buttonSize.x);
            label.text = caption;
        }
    }
}
