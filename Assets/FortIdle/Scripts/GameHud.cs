using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FortIdle
{
    public class GameHud : MonoBehaviour
    {
        public VirtualJoystick Joystick { get; private set; }

        Text stackText;
        Text supplyText;
        Text timerText;
        Text hintText;
        Text resultText;
        Text fireText;
        Text hordeText;
        Text goalText;
        Text bannerText;
        Text dawnText;
        Text bossText;
        Image bossFill;
        RectTransform arrow;
        RectTransform timerRect;
        RectTransform canvasRect;
        readonly List<HudFlyer> flyers = new List<HudFlyer>();
        float resultAge;
        float bannerAge;
        float dawnAge;
        int dawnSerial;
        bool resultShown;
        MatchPhase seenPhase;
        int buyQty = 1;
        HeroController hudHero;
        NightPlan planCache;
        int planKey = int.MinValue;
        int planCacheFrame = -100;
        int seenSeconds = -1;
        int seenNightHud = -1;
        int seenHordeNight = -1;
        int seenLiving = -1;
        int seenPower = int.MinValue;
        int seenThreat = int.MinValue;
        int seenHint = int.MinValue;
        int seenGoal = int.MinValue;
        string seenScarce;
        bool seenWarning;
        readonly List<RectTransform> controls = new List<RectTransform>();
        static readonly Vector3[] controlCorners = new Vector3[4];
        int seenWood = -1;
        int seenCoins = -1;
        int seenStone = -1;
        int seenIron = -1;
        int seenCrew = -1;
        int seenHp = -1;
        int seenMeat = -1;
        int seenBone = -1;
        int seenHide = -1;
        int seenCore = -1;
        int seenSap = -1;

        public static bool BlocksWorldLabel(Rect screen)
        {
            GameHud hud = FortWorld.Hud;
            if (hud == null)
                return false;
            for (int i = 0; i < hud.controls.Count; i++)
            {
                RectTransform rect = hud.controls[i];
                if (rect == null || !rect.gameObject.activeInHierarchy)
                    continue;
                rect.GetWorldCorners(controlCorners);
                float minX = controlCorners[0].x;
                float minY = controlCorners[0].y;
                float maxX = controlCorners[0].x;
                float maxY = controlCorners[0].y;
                for (int c = 1; c < 4; c++)
                {
                    minX = Mathf.Min(minX, controlCorners[c].x);
                    minY = Mathf.Min(minY, controlCorners[c].y);
                    maxX = Mathf.Max(maxX, controlCorners[c].x);
                    maxY = Mathf.Max(maxY, controlCorners[c].y);
                }

                Rect box = Rect.MinMaxRect(minX - 10f, minY - 10f, maxX + 10f, maxY + 10f);
                if (screen.Overlaps(box))
                    return true;
            }

            return false;
        }

        public void Build()
        {
            FortIdleConfig config = FortWorld.Config;
            EnsureEventSystem();

            GameObject canvasObject = new GameObject("HUD");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(config.hud.referenceWidth, config.hud.referenceHeight);
            scaler.matchWidthOrHeight = config.hud.matchWidthOrHeight;
            canvasObject.AddComponent<GraphicRaycaster>();
            canvasRect = canvasObject.GetComponent<RectTransform>();
            FortWorld.Hud = this;

stackText = MakeText(canvasObject.transform, "Stack", config.hud.stackFontSize, TextAnchor.UpperLeft,
    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(config.hud.labelMargin, -config.hud.labelMargin),
    new Vector2(config.hud.stackPanelWidth, config.hud.panelHeight));
fireText = MakeText(canvasObject.transform, "Fire", config.hud.fireFontSize, TextAnchor.UpperRight,
    new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-config.hud.labelMargin, -config.hud.labelMargin),
    new Vector2(config.hud.sidePanelWidth, config.hud.panelHeight));
float hordeDrop = config.hud.labelMargin + config.hud.panelHeight;
hordeText = MakeText(canvasObject.transform, "Horde", config.hud.fireFontSize, TextAnchor.UpperRight,
    new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-config.hud.labelMargin, -hordeDrop),
    new Vector2(config.hud.sidePanelWidth, config.hud.panelHeight));
timerText = MakeText(canvasObject.transform, "Timer", config.hud.timerFontSize, TextAnchor.UpperCenter,
    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -config.hud.labelMargin),
    new Vector2(config.hud.widePanelWidth, config.hud.timerPanelHeight));
timerRect = timerText.rectTransform;
hintText = MakeText(canvasObject.transform, "Hint", config.hud.hintFontSize, TextAnchor.LowerCenter,
    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
    new Vector2(0f, config.joystick.margin + config.joystick.baseSize + config.hud.labelMargin),
    new Vector2(config.hud.widePanelWidth, config.hud.panelHeight));
resultText = MakeText(canvasObject.transform, "Result", config.hud.resultFontSize, TextAnchor.MiddleCenter,
    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
    new Vector2(config.hud.widePanelWidth, config.hud.resultPanelHeight));
            // Right edge sits just left of the buy column so SPEED does not cover the words.
            float controlsClear = 180f + 36f + 28f;
MakeText(canvasObject.transform, "Controls", config.hud.controlsFontSize, TextAnchor.LowerRight,
    new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-controlsClear, config.hud.labelMargin),
    new Vector2(520f, config.hud.controlsPanelHeight))
    .text = config.copy.controls;

            supplyText = MakeText(canvasObject.transform, "Supplies", 28, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(config.hud.labelMargin, -150f),
                new Vector2(760f, 70f));
            goalText = MakeText(canvasObject.transform, "Goal", 32, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f),
                new Vector2(760f, 88f));
            bannerText = MakeText(canvasObject.transform, "Banner", 72, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero,
                new Vector2(900f, 120f));
            dawnText = MakeText(canvasObject.transform, "Dawn", 28, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), Vector2.zero,
                new Vector2(980f, 160f));
            // Goal occupies y -130 through -218. The boss slot starts below that, with a gap.
            bossText = MakeText(canvasObject.transform, "Boss", 26, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -258f),
                new Vector2(520f, 36f));
            bossFill = MakeBar(canvasObject.transform, new Vector2(0f, -302f));
            arrow = MakeArrow(canvasObject.transform);
            BuildPanels(canvasObject.transform);
            Joystick = BuildJoystick(canvasObject.transform);
        }

        public void Fly(Vector3 world, DropKind kind)
        {
            if (FortWorld.Camera == null || canvasRect == null)
                return;
            Vector3 screen = FortWorld.Camera.WorldToScreenPoint(world);
            if (screen.z < 0f)
                return;
            FortIdleConfig config = FortWorld.Config;
            Color color = config.presentation.coin;
            if (kind == DropKind.Meat)
                color = config.presentation.meat;
            else if (kind == DropKind.Bone)
                color = config.presentation.bone;
            else if (kind == DropKind.Hide)
                color = config.presentation.hide;
            else if (kind == DropKind.Core)
                color = config.presentation.core;
            else if (kind == DropKind.Sap)
                color = config.presentation.sap;
            else if (kind == DropKind.Iron)
                color = config.presentation.iron;

            GameObject go = new GameObject("Fly");
            go.transform.SetParent(canvasRect, false);
            Image image = go.AddComponent<Image>();
            image.sprite = FortWorld.Look.Circle;
            image.color = color;
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(18f, 18f);
            Vector2 from = ScreenToCanvas(new Vector2(screen.x, screen.y));
            rect.anchoredPosition = from;
            HudFlyer flyer = go.AddComponent<HudFlyer>();
            flyer.From = from;
            flyer.To = new Vector2(-config.hud.referenceWidth * 0.5f + 180f, config.hud.referenceHeight * 0.5f - 70f);
            flyers.Add(flyer);
        }

        void LateUpdate()
        {
            FortIdleConfig config = FortWorld.Config;
            MatchDirector match = FortWorld.Match;
            if (config == null || match == null || stackText == null)
                return;

            int wood = FortWorld.Stack != null ? FortWorld.Stack.Count : 0;
            int cap = FortWorld.Stack != null ? FortWorld.Stack.Capacity : config.stack.capacity;
            FortProgress progress = FortWorld.Progress;
            int coins = progress != null ? progress.Coins : 0;
            int stone = progress != null ? progress.CarriedStone : 0;
            int iron = progress != null ? progress.Iron : 0;
            float speed = progress != null ? progress.MoveSpeed : config.player.moveSpeed;
            int crew = FortWorld.Workers.Count;
            if (hudHero == null && FortWorld.Hero != null)
                hudHero = FortWorld.Hero.GetComponent<HeroController>();
            int hpNow = hudHero != null ? Mathf.CeilToInt(hudHero.HitPoints) : 0;
            if (wood != seenWood || coins != seenCoins || stone != seenStone || iron != seenIron || crew != seenCrew || hpNow != seenHp)
            {
                seenWood = wood;
                seenCoins = coins;
                seenStone = stone;
                seenIron = iron;
                seenCrew = crew;
                seenHp = hpNow;
                string full = wood >= cap ? "  " + config.copy.maxBadge : "";
                string hp = hudHero != null ? "    HP " + hpNow + "/" + Mathf.CeilToInt(hudHero.MaxHitPoints) : "";
                stackText.text = config.copy.woodLabel + "  " + wood + "/" + cap + full
                    + "\n" + config.copy.coinsLabel + "  " + coins
                    + "    " + config.copy.stoneLabel + "  " + stone
                    + "    " + config.copy.ironLabel + "  " + iron
                    + "\n" + config.copy.speedLabel + "  " + speed.ToString("0.0")
                    + "    " + config.copy.crewLabel + "  " + crew
                    + hp;
            }

            if (supplyText != null && progress != null
                && (progress.Meat != seenMeat || progress.Bones != seenBone || progress.Hide != seenHide || progress.Cores != seenCore || progress.Sap != seenSap))
            {
                seenMeat = progress.Meat;
                seenBone = progress.Bones;
                seenHide = progress.Hide;
                seenCore = progress.Cores;
                seenSap = progress.Sap;
                supplyText.text = config.copy.meatLabel + " " + progress.Meat
                    + "   " + config.copy.boneLabel + " " + progress.Bones
                    + "   " + config.copy.hideLabel + " " + progress.Hide
                    + "   " + config.copy.coreLabel + " " + progress.Cores
                    + "   " + config.copy.sapLabel + " " + progress.Sap;
            }

            int seconds = Mathf.CeilToInt(Mathf.Max(0f, match.ClockRemaining));
            bool warning = match.Phase == MatchPhase.Day && match.ClockRemaining <= config.wave.nightWarningSeconds && match.ClockRemaining > 0f;
            if (seconds != seenSeconds || warning != seenWarning || match.NightIndex != seenNightHud)
            {
                seenSeconds = seconds;
                seenWarning = warning;
                seenNightHud = match.NightIndex;
                int minutes = seconds / 60;
                int remainder = seconds % 60;
                string clock = minutes.ToString("00") + ":" + remainder.ToString("00");
                string nightMark = config.copy.nightPrefix + match.NightIndex + "  " + clock;
                timerText.text = warning ? nightMark + "  " + config.copy.untilNight : nightMark;
                timerText.color = warning ? config.presentation.warningText : config.presentation.hudText;
            }
            float pulse = 1f;
            if (warning)
            {
                float wave = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * config.hud.timerPulseSpeed);
                pulse = Mathf.Lerp(1f, config.hud.timerWarningScale, wave);
            }

            timerRect.localScale = new Vector3(pulse, pulse, 1f);

            if (FortWorld.Fire != null)
                fireText.text = config.copy.fireLabel + "  " + Mathf.CeilToInt(FortWorld.Fire.HitPoints) + "/" + Mathf.CeilToInt(FortWorld.Fire.MaxHitPoints);
            NightPlan shown = NightPlanFor(match);
            int power = Mathf.RoundToInt(shown.Power);
            int threat = Mathf.RoundToInt(shown.Threat);
            if (power != seenPower || threat != seenThreat || match.LivingMonsters != seenLiving || match.NightIndex != seenHordeNight || match.Phase != seenPhase)
            {
                seenPower = power;
                seenThreat = threat;
                seenLiving = match.LivingMonsters;
                seenHordeNight = match.NightIndex;
                string powerLine = config.copy.powerLabel + "  " + power
                    + "    " + config.copy.threatLabel + "  " + threat;
                hordeText.text = match.Phase == MatchPhase.Day
                    ? powerLine
                    : config.copy.waveLabel + "  " + match.NightIndex + "    " + config.copy.hordeLabel + "  " + match.LivingMonsters + "\n" + powerLine;
            }

            hordeText.color = ThreatColor(shown.Ratio);
            PaintGoal(config, match);
            PaintBanner(config, match);
            PaintBoss(config);
            PaintDawn(config, match);
            PaintArrow(config, match);

            if (match.Phase == MatchPhase.Resolved)
            {
                if (!resultShown)
                {
                    resultShown = true;
                    resultAge = 0f;
                }

                resultAge += Time.deltaTime;
                if (resultAge <= config.wave.resultHoldSeconds)
                {
                    resultText.text = match.Victory ? config.copy.resultClear : config.copy.resultFail;
                    resultText.color = match.Victory ? config.presentation.resultWin : config.presentation.resultFail;
                }
                else
                {
                    resultText.text = "";
                }
            }
            else
            {
                resultShown = false;
                resultAge = 0f;
                if (resultText.text.Length > 0)
                    resultText.text = "";
            }

            int hint = HintCode(match);
            if (hint != seenHint)
            {
                seenHint = hint;
                hintText.text = Hint(config, match);
            }
        }

        NightPlan NightPlanFor(MatchDirector match)
        {
            if (match.Phase == MatchPhase.Night && match.Plan != null && match.Plan.Night == match.NightIndex)
                return match.Plan;
            int key = match.NightIndex + match.LossStreak * 64;
            if (planCache == null || planKey != key || planCacheFrame + 30 < Time.frameCount)
            {
                planCache = ThreatMath.Build(match.NightIndex, match.LossStreak);
                planKey = key;
                planCacheFrame = Time.frameCount;
            }

            return planCache;
        }

        void PaintGoal(FortIdleConfig config, MatchDirector match)
        {
            if (goalText == null)
                return;
            int goal = GoalCode(match);
            string scarce = ThreatMath.Scarce();
            if (goal != seenGoal || scarce != seenScarce)
            {
                seenGoal = goal;
                seenScarce = scarce;
                goalText.text = GoalChip(config, match);
            }
        }

        static int GoalCode(MatchDirector match)
        {
            if (match.Phase == MatchPhase.Night)
                return 1;
            if (FortWorld.Wall != null && FortWorld.Wall.NeedsRepair)
                return 2;
            if (FortWorld.Wall != null && !FortWorld.Wall.IsComplete)
                return 3;
            if (FortWorld.TurretPad != null && !FortWorld.TurretPad.IsComplete)
                return 4;
            if (FortWorld.Progress != null && !FortWorld.Progress.HeartyMeal && FortWorld.Progress.Meat >= FortWorld.Config.drops.dawnMealMeat)
                return 5;
            if (FortWorld.Hut != null && !FortWorld.Hut.IsComplete)
                return 6;
            if (FortWorld.Progress != null && FortWorld.Progress.MeleeLevel <= 0)
                return 7;
            return 8;
        }

        static string GoalChip(FortIdleConfig config, MatchDirector match)
        {
            if (match.Phase == MatchPhase.Night)
                return WithNeed(config, config.copy.goalNight);
            if (FortWorld.Wall != null && FortWorld.Wall.NeedsRepair)
                return WithNeed(config, config.copy.goalRepair);
            if (FortWorld.Wall != null && !FortWorld.Wall.IsComplete)
                return WithNeed(config, config.copy.goalWall);
            if (FortWorld.TurretPad != null && !FortWorld.TurretPad.IsComplete)
                return WithNeed(config, config.copy.goalTurret);
            if (FortWorld.Progress != null && !FortWorld.Progress.HeartyMeal && FortWorld.Progress.Meat >= config.drops.dawnMealMeat)
                return WithNeed(config, config.copy.goalCook);
            if (FortWorld.Hut != null && !FortWorld.Hut.IsComplete)
                return WithNeed(config, config.copy.goalHut);
            if (FortWorld.Progress != null && FortWorld.Progress.MeleeLevel <= 0)
                return WithNeed(config, config.copy.goalHero);
            return WithNeed(config, config.copy.goalUpgrade);
        }

        static string WithNeed(FortIdleConfig config, string goal)
        {
            return goal + "\n" + config.copy.needLabel + "  " + ThreatMath.Scarce();
        }

        static Color ThreatColor(float ratio)
        {
            if (ratio < 0.75f)
                return new Color(0.45f, 0.9f, 0.42f, 1f);
            if (ratio < 1f)
                return new Color(0.95f, 0.82f, 0.28f, 1f);
            return new Color(0.92f, 0.38f, 0.28f, 1f);
        }

        void PaintBanner(FortIdleConfig config, MatchDirector match)
        {
            if (bannerText == null)
                return;
            if (match.Phase == MatchPhase.Night && seenPhase != MatchPhase.Night)
                bannerAge = 3.2f;
            seenPhase = match.Phase;
            if (bannerAge > 0f)
            {
                bannerAge -= Time.deltaTime;
                bannerText.text = config.copy.nightBanner + "  " + match.NightIndex;
                bannerText.color = config.presentation.warningText;
            }
            else
                bannerText.text = "";
        }

        void PaintBoss(FortIdleConfig config)
        {
            Monster boss = null;
            for (int i = 0; i < FortWorld.Monsters.Count; i++)
            {
                if (FortWorld.Monsters[i].IsBoss)
                    boss = FortWorld.Monsters[i];
            }

            bool show = boss != null;
            if (bossText != null)
                bossText.text = show ? config.copy.bossName : "";
            if (bossFill != null)
            {
                bossFill.transform.parent.gameObject.SetActive(show);
                RectTransform rect = bossFill.rectTransform;
                rect.anchorMax = new Vector2(Mathf.Clamp01(show ? boss.HealthFraction : 0f), 1f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        void PaintDawn(FortIdleConfig config, MatchDirector match)
        {
            if (dawnText == null || FortWorld.Progress == null)
                return;
            if (FortWorld.Progress.DawnSerial != dawnSerial)
            {
                dawnSerial = FortWorld.Progress.DawnSerial;
                dawnAge = 5.5f;
            }

            if (dawnAge <= 0f)
            {
                if (match.Phase != MatchPhase.Resolved)
                    dawnText.text = "";
                return;
            }

            dawnAge -= Time.deltaTime;
            string line = FortWorld.Progress.DawnLine.Replace("FORTIDLE ledger ", "");
            dawnText.text = config.copy.dawnTitle + "\n" + line;
            if (dawnAge <= 0f)
                dawnText.text = "";
        }

        void PaintArrow(FortIdleConfig config, MatchDirector match)
        {
            if (arrow == null || FortWorld.Camera == null)
                return;
            Vector3 world = ArrowTarget(config, match);
            Vector3 screen = FortWorld.Camera.WorldToScreenPoint(world);
            bool behind = screen.z < 0f;
            Vector2 canvasPoint = ScreenToCanvas(new Vector2(screen.x, screen.y));
            if (behind)
                canvasPoint = -canvasPoint;
            float limitX = config.hud.referenceWidth * 0.46f;
            float limitY = config.hud.referenceHeight * 0.42f;
            bool off = behind || Mathf.Abs(canvasPoint.x) > limitX || Mathf.Abs(canvasPoint.y) > limitY;
            arrow.gameObject.SetActive(off);
            if (!off)
                return;
            Vector2 edge = canvasPoint;
            edge.x = Mathf.Clamp(edge.x, -limitX, limitX);
            edge.y = Mathf.Clamp(edge.y, -limitY, limitY);
            arrow.anchoredPosition = edge;
            float angle = Mathf.Atan2(canvasPoint.y, canvasPoint.x) * Mathf.Rad2Deg;
            arrow.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        static Vector3 ArrowTarget(FortIdleConfig config, MatchDirector match)
        {
            if (match.Phase == MatchPhase.Night && FortWorld.Wall != null)
                return FortWorld.Wall.transform.position;
            if (FortWorld.Wall != null && !FortWorld.Wall.IsComplete)
                return FortWorld.Wall.transform.position;
            if (FortWorld.TurretPad != null && !FortWorld.TurretPad.IsComplete)
                return FortWorld.TurretPad.transform.position;
            if (FortWorld.Fire != null && FortWorld.Progress != null && !FortWorld.Progress.HeartyMeal && FortWorld.Progress.Meat >= config.drops.dawnMealMeat)
                return FortWorld.Fire.transform.position;
            if (FortWorld.MeleePad != null && FortWorld.Progress != null && FortWorld.Progress.MeleeLevel <= 0)
                return FortWorld.MeleePad.transform.position;
            return FortWorld.Hero != null ? FortWorld.Hero.position : Vector3.zero;
        }

        Vector2 ScreenToCanvas(Vector2 screen)
        {
            FortIdleConfig config = FortWorld.Config;
            float x = (screen.x / Mathf.Max(1f, Screen.width) - 0.5f) * config.hud.referenceWidth;
            float y = (screen.y / Mathf.Max(1f, Screen.height) - 0.5f) * config.hud.referenceHeight;
            return new Vector2(x, y);
        }

        Image MakeBar(Transform parent, Vector2 anchored)
        {
            GameObject frame = new GameObject("BossFrame");
            frame.transform.SetParent(parent, false);
            Image back = frame.AddComponent<Image>();
            back.color = new Color(0.1f, 0.08f, 0.1f, 0.85f);
            RectTransform frameRect = back.rectTransform;
            frameRect.anchorMin = new Vector2(0.5f, 1f);
            frameRect.anchorMax = new Vector2(0.5f, 1f);
            frameRect.pivot = new Vector2(0.5f, 1f);
            frameRect.anchoredPosition = anchored;
            frameRect.sizeDelta = new Vector2(420f, 18f);
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(frame.transform, false);
            Image image = fill.AddComponent<Image>();
            image.color = FortWorld.Config.presentation.bossBar;
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            frame.SetActive(false);
            return image;
        }

        RectTransform MakeArrow(Transform parent)
        {
            GameObject go = new GameObject("Arrow");
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = FortWorld.Look.Circle;
            image.color = FortWorld.Config.presentation.warningText;
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(28f, 36f);
            go.SetActive(false);
            return rect;
        }

        void BuildPanels(Transform parent)
        {
            MakeButton(parent, "Qty1", "x1", new Vector2(-420f, 290f), () => buyQty = 1);
            MakeButton(parent, "Qty10", "x10", new Vector2(-230f, 290f), () => buyQty = 10);
            MakeButton(parent, "QtyMax", "MAX", new Vector2(-36f, 290f), () => buyQty = -1);
            MakeButton(parent, "HireButton", "HIRE", new Vector2(-36f, 220f), () =>
            {
                if (FortWorld.Progress == null)
                    return;
                int hired = FortWorld.Progress.BuyHire(buyQty);
                for (int i = 0; i < hired; i++)
                    WorkerAgent.SpawnNext();
            });
            MakeButton(parent, "MeleeButton", "MELEE", new Vector2(-36f, 150f), () =>
            {
                if (FortWorld.Progress != null)
                    FortWorld.Progress.BuyMelee(buyQty);
            });
            MakeButton(parent, "BagButton", "BAG", new Vector2(-36f, 80f), () =>
            {
                if (FortWorld.Progress != null)
                    FortWorld.Progress.BuyStack(buyQty);
            });
            MakeButton(parent, "SpeedButton", "SPEED", new Vector2(-36f, 10f), () =>
            {
                if (FortWorld.Progress != null)
                    FortWorld.Progress.BuySpeed(buyQty);
            });
        }

        void MakeButton(Transform parent, string name, string caption, Vector2 anchored, UnityEngine.Events.UnityAction action)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = new Color(0.12f, 0.1f, 0.08f, 1f);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = anchored;
            rect.sizeDelta = new Vector2(180f, 58f);
            controls.Add(rect);
            Text label = MakeText(go.transform, "Label", 24, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            label.text = caption;
            label.raycastTarget = false;
        }

        static int HintCode(MatchDirector match)
        {
            if (match.Phase == MatchPhase.Resolved && !match.Victory)
                return 0;
            if (FortWorld.Wall != null && FortWorld.Wall.NeedsRepair)
                return 1;
            bool wallUp = FortWorld.Wall != null && FortWorld.Wall.IsComplete;
            bool turretUp = FortWorld.TurretPad != null && FortWorld.TurretPad.IsComplete;
            bool hutUp = FortWorld.Hut != null && FortWorld.Hut.IsComplete;
            bool full = FortWorld.Stack != null && FortWorld.Stack.IsFull;
            bool carrying = FortWorld.Stack != null && FortWorld.Stack.Count > 0;
            if (match.Phase == MatchPhase.Night && !turretUp && match.LivingMonsters > 0)
                return 2;
            if (match.Phase == MatchPhase.Night && turretUp && !hutUp)
                return 3;
            if (match.Phase == MatchPhase.Night && !turretUp)
                return 4;
            if (FortWorld.Workers.Count > 0)
                return 5;
            if (hutUp && FortWorld.Progress != null && FortWorld.Progress.CanHire)
                return 6;
            if (full)
                return 7;
            if (!wallUp && carrying)
                return 8;
            if (wallUp && !turretUp && carrying)
                return 9;
            if (wallUp && turretUp && !hutUp)
                return 10;
            if (!wallUp)
                return 11;
            if (FortWorld.Progress != null && FortWorld.Progress.StackLevel <= 0)
                return 12;
            if (FortWorld.Progress != null && FortWorld.Progress.SpeedLevel <= 0)
                return 13;
            return 14;
        }

        static string Hint(FortIdleConfig config, MatchDirector match)
        {
            if (match.Phase == MatchPhase.Resolved && !match.Victory)
                return "";
            if (FortWorld.Wall != null && FortWorld.Wall.NeedsRepair)
                return config.copy.hintRepair;
            bool wallUp = FortWorld.Wall != null && FortWorld.Wall.IsComplete;
            bool turretUp = FortWorld.TurretPad != null && FortWorld.TurretPad.IsComplete;
            bool hutUp = FortWorld.Hut != null && FortWorld.Hut.IsComplete;
            bool full = FortWorld.Stack != null && FortWorld.Stack.IsFull;
            bool carrying = FortWorld.Stack != null && FortWorld.Stack.Count > 0;
            if (match.Phase == MatchPhase.Night && !turretUp && match.LivingMonsters > 0)
                return config.copy.hintNoTurret;
            if (match.Phase == MatchPhase.Night && turretUp && !hutUp)
                return config.copy.hintNight;
            if (match.Phase == MatchPhase.Night && !turretUp)
                return config.copy.hintPourTurret;
            if (FortWorld.Workers.Count > 0)
                return config.copy.hintWorkers;
            if (hutUp && FortWorld.Progress != null && FortWorld.Progress.CanHire)
                return config.copy.hintHire;
            if (full)
                return config.copy.hintStackFull;
            if (!wallUp && carrying)
                return config.copy.hintPourWall;
            if (wallUp && !turretUp && carrying)
                return config.copy.hintPourTurret;
            if (wallUp && turretUp && !hutUp)
                return config.copy.hintPourHut;
            if (!wallUp)
                return config.copy.hintChop;
            if (FortWorld.Progress != null && FortWorld.Progress.StackLevel <= 0)
                return config.copy.hintUpgradeBag;
            if (FortWorld.Progress != null && FortWorld.Progress.SpeedLevel <= 0)
                return config.copy.hintUpgradeSpeed;
            return config.copy.hintReady;
        }

        VirtualJoystick BuildJoystick(Transform parent)
        {
            FortIdleConfig config = FortWorld.Config;
            GameObject root = new GameObject("Joystick");
            root.transform.SetParent(parent, false);
            RectTransform baseRect = root.AddComponent<RectTransform>();
            baseRect.anchorMin = new Vector2(0f, 0f);
            baseRect.anchorMax = new Vector2(0f, 0f);
            baseRect.pivot = new Vector2(0.5f, 0.5f);
            baseRect.sizeDelta = new Vector2(config.joystick.baseSize, config.joystick.baseSize);
            baseRect.anchoredPosition = new Vector2(
                config.joystick.margin + config.joystick.baseSize * 0.5f,
                config.joystick.margin + config.joystick.baseSize * 0.5f);
            Image baseImage = root.AddComponent<Image>();
            baseImage.sprite = FortWorld.Look.Circle;
            baseImage.color = WithAlpha(config.presentation.joystickBase, config.joystick.baseAlpha);
            baseImage.raycastTarget = true;

            GameObject knobObject = new GameObject("Knob");
            knobObject.transform.SetParent(root.transform, false);
            RectTransform knobRect = knobObject.AddComponent<RectTransform>();
            knobRect.anchorMin = new Vector2(0.5f, 0.5f);
            knobRect.anchorMax = new Vector2(0.5f, 0.5f);
            knobRect.pivot = new Vector2(0.5f, 0.5f);
            knobRect.sizeDelta = new Vector2(config.joystick.knobSize, config.joystick.knobSize);
            knobRect.anchoredPosition = Vector2.zero;
            Image knobImage = knobObject.AddComponent<Image>();
            knobImage.sprite = FortWorld.Look.Circle;
            knobImage.color = WithAlpha(config.presentation.joystickKnob, config.joystick.knobAlpha);
            knobImage.raycastTarget = true;

            VirtualJoystick stick = root.AddComponent<VirtualJoystick>();
            stick.Configure(baseRect, knobRect, config.joystick.travel, config.joystick.deadzone);
            controls.Add(baseRect);
            return stick;
        }

        Text MakeText(Transform parent, string name, int fontSize, TextAnchor anchor, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchored, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = FortWorld.Look.Font;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = FortWorld.Config.presentation.hudText;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            if (anchor == TextAnchor.UpperCenter || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.LowerCenter)
                rect.pivot = new Vector2(0.5f, anchorMin.y);
            if (anchor == TextAnchor.UpperRight || anchor == TextAnchor.MiddleRight || anchor == TextAnchor.LowerRight)
                rect.pivot = new Vector2(1f, anchorMin.y);
            rect.anchoredPosition = anchored;
            rect.sizeDelta = size;
            return text;
        }

        static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;
            GameObject events = new GameObject("EventSystem");
            events.AddComponent<EventSystem>();
            events.AddComponent<StandaloneInputModule>();
        }

        static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }

    public class HudFlyer : MonoBehaviour
    {
        public Vector2 From;
        public Vector2 To;
        float age;

        void Update()
        {
            age += Time.deltaTime;
            float u = Mathf.Clamp01(age / 0.45f);
            RectTransform rect = (RectTransform)transform;
            rect.anchoredPosition = Vector2.Lerp(From, To, u);
            if (u >= 1f)
                Destroy(gameObject);
        }
    }
}
