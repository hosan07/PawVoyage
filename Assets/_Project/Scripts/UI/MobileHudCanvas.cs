using PawVoyage.Combat;
using PawVoyage.Player;
using PawVoyage.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace PawVoyage.UI
{
    /// <summary>
    /// 모바일 해상도에서 Farmer와 Barn 전투 정보를 표시하는 Canvas 기반 HUD입니다.
    /// </summary>
    public class MobileHudCanvas : MonoBehaviour
    {
        private const string RootName = "MobileHudCanvas";

        private static readonly Color PanelColor = new Color(0.055f, 0.08f, 0.12f, 0.86f);
        private static readonly Color BarBackgroundColor = new Color(0.02f, 0.03f, 0.05f, 0.9f);
        private static readonly Color FarmerBarColor = new Color(0.88f, 0.2f, 0.2f, 1f);
        private static readonly Color BarnBarColor = new Color(0.76f, 0.39f, 0.16f, 1f);
        private static readonly Color ExperienceBarColor = new Color(0.2f, 0.82f, 0.36f, 1f);
        private static readonly Color StageBarColor = new Color(0.18f, 0.55f, 0.94f, 1f);

        private PlayerHud playerHud;
        private Health farmerHealth;
        private PlayerExperience playerExperience;
        private RunStats runStats;
        private PausePanel pausePanel;
        private LevelUpPanel levelUpPanel;
        private GameOverPanel gameOverPanel;
        private StageClearPanel stageClearPanel;
        private BarnObjective barnObjective;
        private WaveSpawner waveSpawner;

        private Image farmerFill;
        private Image barnFill;
        private Image experienceFill;
        private Image stageFill;
        private Text farmerLabel;
        private Text barnLabel;
        private Text experienceLabel;
        private Text stageLabel;
        private Text combatLabel;
        private Text coinLabel;
        private GameObject pauseOverlay;
        private GameObject levelUpOverlay;
        private GameObject resultOverlay;

        public static MobileHudCanvas CreateOrGet(PlayerHud sourceHud)
        {
            MobileHudCanvas existing = Object.FindFirstObjectByType<MobileHudCanvas>();
            if (existing != null)
            {
                existing.Bind(sourceHud);
                return existing;
            }

            GameObject root = new GameObject(RootName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();
            MobileHudCanvas hud = root.AddComponent<MobileHudCanvas>();
            hud.Bind(sourceHud);
            return hud;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Object.DontDestroyOnLoad(eventSystem);
        }

        private void Bind(PlayerHud sourceHud)
        {
            playerHud = sourceHud;
            if (playerHud == null)
            {
                return;
            }

            farmerHealth = playerHud.GetComponent<Health>();
            playerExperience = playerHud.GetComponent<PlayerExperience>();
            runStats = playerHud.GetComponent<RunStats>();
            pausePanel = playerHud.GetComponent<PausePanel>();
            levelUpPanel = playerHud.GetComponent<LevelUpPanel>();
            gameOverPanel = playerHud.GetComponent<GameOverPanel>();
            stageClearPanel = playerHud.GetComponent<StageClearPanel>();

            if (farmerFill == null)
            {
                BuildHud();
            }

            if (pausePanel != null)
            {
                pausePanel.PauseStateChanged -= SetPauseOverlayVisible;
                pausePanel.PauseStateChanged += SetPauseOverlayVisible;
                SetPauseOverlayVisible(pausePanel.IsPaused);
            }

            if (levelUpPanel != null)
            {
                levelUpPanel.PanelOpened -= ShowLevelUp;
                levelUpPanel.PanelOpened += ShowLevelUp;
                levelUpPanel.PanelClosed -= HideLevelUp;
                levelUpPanel.PanelClosed += HideLevelUp;
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.PanelOpened -= ShowGameOver;
                gameOverPanel.PanelOpened += ShowGameOver;
            }

            if (stageClearPanel != null)
            {
                stageClearPanel.PanelOpened -= ShowStageClear;
                stageClearPanel.PanelOpened += ShowStageClear;
            }
        }

        private void OnDestroy()
        {
            if (pausePanel != null)
            {
                pausePanel.PauseStateChanged -= SetPauseOverlayVisible;
            }

            if (levelUpPanel != null)
            {
                levelUpPanel.PanelOpened -= ShowLevelUp;
                levelUpPanel.PanelClosed -= HideLevelUp;
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.PanelOpened -= ShowGameOver;
            }

            if (stageClearPanel != null)
            {
                stageClearPanel.PanelOpened -= ShowStageClear;
            }
        }

        private void Update()
        {
            if (playerHud == null)
            {
                return;
            }

            barnObjective ??= BarnObjective.Instance;
            waveSpawner ??= WaveSpawner.Instance;
            UpdateStatusRows();
            UpdateCombatSummary();
        }

        private void BuildHud()
        {
            RectTransform topLeft = CreatePanel(transform, "StatusPanel", new Vector2(34f, -42f), new Vector2(402f, 330f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            CreateStatusRow(topLeft, 30f, UiIconDrawer.FarmerHealth, "농부", FarmerBarColor, out farmerFill, out farmerLabel);
            CreateStatusRow(topLeft, 110f, UiIconDrawer.BarnHealth, "헛간", BarnBarColor, out barnFill, out barnLabel);
            CreateStatusRow(topLeft, 190f, null, "LV 1", ExperienceBarColor, out experienceFill, out experienceLabel);
            CreateStatusRow(topLeft, 270f, null, "STAGE 00:00", StageBarColor, out stageFill, out stageLabel);

            RectTransform summaryPanel = CreatePanel(transform, "CombatSummary", new Vector2(34f, -390f), new Vector2(402f, 72f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            combatLabel = CreateText(summaryPanel, "CombatText", 24, TextAnchor.UpperLeft, Color.white);
            SetStretch(combatLabel.rectTransform, 18f, 10f, 18f, 10f);

            coinLabel = CreateText(transform, "CoinText", 28, TextAnchor.MiddleRight, Color.white);
            SetAnchored(coinLabel.rectTransform, new Vector2(-122f, -48f), new Vector2(130f, 44f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            CreateIcon(transform, "CoinIcon", UiIconDrawer.FarmCoin, new Vector2(-108f, -48f), new Vector2(38f, 38f), new Vector2(1f, 1f), new Vector2(1f, 1f));

            Button pauseButton = CreateButton(transform, "PauseButton", new Vector2(-44f, -48f), new Vector2(72f, 72f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Color(0.1f, 0.14f, 0.2f, 0.94f));
            CreateIcon(pauseButton.transform, "PauseIcon", UiIconDrawer.Pause, Vector2.zero, new Vector2(34f, 34f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            pauseButton.onClick.AddListener(() => pausePanel?.TogglePause());

            BuildPauseOverlay();
        }

        private void BuildPauseOverlay()
        {
            pauseOverlay = CreateImage(transform, "PauseOverlay", new Color(0f, 0f, 0f, 0.64f)).gameObject;
            SetStretch(pauseOverlay.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);

            RectTransform panel = CreatePanel(pauseOverlay.transform, "PausePanel", Vector2.zero, new Vector2(620f, 480f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Text title = CreateText(panel, "Title", 46, TextAnchor.MiddleCenter, Color.white);
            title.fontStyle = FontStyle.Bold;
            title.text = "일시정지";
            SetAnchored(title.rectTransform, new Vector2(0f, -72f), new Vector2(520f, 62f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            Text body = CreateText(panel, "Body", 24, TextAnchor.MiddleCenter, new Color(0.82f, 0.87f, 0.93f, 1f));
            body.text = "전투를 다시 시작하거나 처음부터 도전할 수 있습니다.";
            SetAnchored(body.rectTransform, new Vector2(0f, -148f), new Vector2(520f, 60f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            Button resumeButton = CreateButton(panel, "ResumeButton", new Vector2(0f, -260f), new Vector2(460f, 72f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Color(0.2f, 0.52f, 0.34f, 1f));
            SetButtonText(resumeButton, "계속하기");
            resumeButton.onClick.AddListener(() => pausePanel?.Resume());

            Button retryButton = CreateButton(panel, "RetryButton", new Vector2(0f, -352f), new Vector2(460f, 72f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Color(0.34f, 0.19f, 0.18f, 1f));
            SetButtonText(retryButton, "다시 시작");
            retryButton.onClick.AddListener(() => pausePanel?.RestartScene());
            SetPauseOverlayVisible(false);
        }

        private void UpdateStatusRows()
        {
            if (farmerHealth != null)
            {
                SetBar(farmerFill, farmerHealth.CurrentHp, farmerHealth.MaxHp);
                farmerLabel.text = $"농부  {farmerHealth.CurrentHp}/{farmerHealth.MaxHp}";
            }

            int barnCurrentHp = barnObjective != null ? barnObjective.CurrentHp : 0;
            int barnMaxHp = barnObjective != null ? barnObjective.MaxHp : 1;
            SetBar(barnFill, barnCurrentHp, barnMaxHp);
            barnLabel.text = barnObjective != null ? $"헛간  {barnCurrentHp}/{barnMaxHp}" : "헛간  --/--";

            if (playerExperience != null)
            {
                SetBar(experienceFill, playerExperience.CurrentExp, playerExperience.ExpToNextLevel);
                experienceLabel.text = $"LV {playerExperience.CurrentLevel}  경험치 {playerExperience.CurrentExp}/{playerExperience.ExpToNextLevel}";
            }

            float elapsedSeconds = runStats != null ? runStats.ElapsedSeconds : 0f;
            float clearSeconds = runStats != null ? runStats.ClearTimeSeconds : 1f;
            SetBar(stageFill, Mathf.RoundToInt(elapsedSeconds), Mathf.RoundToInt(clearSeconds));
            stageLabel.text = $"스테이지  {FormatTime(elapsedSeconds)} / {FormatTime(clearSeconds)}";
        }

        private void UpdateCombatSummary()
        {
            int kills = runStats != null ? runStats.KillCount : 0;
            int enemies = waveSpawner != null ? waveSpawner.CurrentAliveEnemies : 0;
            int cap = waveSpawner != null ? waveSpawner.CurrentMaxAliveEnemies : 0;
            string phase = waveSpawner != null ? waveSpawner.CurrentPhaseName : "NORMAL";
            combatLabel.text = $"처치 {kills}     페이즈 {phase}\n적 {enemies}/{cap}";
            coinLabel.text = $"{runStats?.CoinsCollected ?? 0}";
        }

        private void SetPauseOverlayVisible(bool isVisible)
        {
            if (pauseOverlay != null)
            {
                pauseOverlay.SetActive(isVisible);
            }
        }

        private void ShowLevelUp(LevelUpPanel panel)
        {
            RemoveOverlay(ref levelUpOverlay);
            levelUpOverlay = CreateOverlay("LevelUpOverlay", new Color(0f, 0f, 0f, 0.62f));

            RectTransform panelRect = CreatePanel(levelUpOverlay.transform, "LevelUpPanel", Vector2.zero, new Vector2(760f, 760f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Text title = CreateText(panelRect, "Title", 44, TextAnchor.MiddleCenter, Color.white);
            title.fontStyle = FontStyle.Bold;
            title.text = "레벨 업";
            SetAnchored(title.rectTransform, new Vector2(0f, -66f), new Vector2(640f, 56f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            Text subtitle = CreateText(panelRect, "Subtitle", 22, TextAnchor.MiddleCenter, new Color(0.8f, 0.87f, 0.95f, 1f));
            subtitle.text = "원하는 성장 카드를 선택하세요";
            SetAnchored(subtitle.rectTransform, new Vector2(0f, -122f), new Vector2(640f, 38f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            int count = panel.VisibleRewardCount;
            for (int i = 0; i < count; i++)
            {
                int rewardIndex = i;
                float y = -210f - 148f * i;
                Button card = CreateButton(panelRect, "RewardCard", new Vector2(0f, y), new Vector2(650f, 120f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Color(0.15f, 0.22f, 0.32f, 1f));
                CreateIcon(card.transform, "Icon", panel.GetRewardIconPathForIndex(rewardIndex), new Vector2(38f, 0f), new Vector2(62f, 62f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
                Text label = CreateText(card.transform, "Label", 27, TextAnchor.MiddleLeft, Color.white);
                label.fontStyle = FontStyle.Bold;
                label.text = panel.GetRewardLabelForIndex(rewardIndex);
                SetAnchored(label.rectTransform, new Vector2(124f, 0f), new Vector2(480f, 72f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
                card.onClick.AddListener(() => panel.SelectReward(rewardIndex));
            }
        }

        private void HideLevelUp()
        {
            RemoveOverlay(ref levelUpOverlay);
        }

        private void ShowGameOver(GameOverPanel panel)
        {
            ShowResult(panel.CanvasTitle, panel.CanvasSummary, UiIconDrawer.FarmerHealth, new Color(0.56f, 0.16f, 0.16f, 1f), panel.RestartScene, panel.LoadMainMenu);
        }

        private void ShowStageClear(StageClearPanel panel)
        {
            ShowResult(panel.CanvasTitle, panel.CanvasSummary, UiIconDrawer.FarmBarn, new Color(0.18f, 0.48f, 0.28f, 1f), panel.RestartScene, panel.LoadMainMenu);
        }

        private void ShowResult(string titleValue, string summaryValue, string iconPath, Color titleColor, UnityEngine.Events.UnityAction retryAction, UnityEngine.Events.UnityAction menuAction)
        {
            RemoveOverlay(ref resultOverlay);
            resultOverlay = CreateOverlay("ResultOverlay", new Color(0f, 0f, 0f, 0.7f));
            RectTransform panel = CreatePanel(resultOverlay.transform, "ResultPanel", Vector2.zero, new Vector2(760f, 820f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            CreateIcon(panel, "ResultIcon", iconPath, new Vector2(0f, -76f), new Vector2(76f, 76f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            Text title = CreateText(panel, "Title", 42, TextAnchor.MiddleCenter, titleColor);
            title.fontStyle = FontStyle.Bold;
            title.text = titleValue;
            SetAnchored(title.rectTransform, new Vector2(0f, -160f), new Vector2(660f, 62f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            Text summary = CreateText(panel, "Summary", 23, TextAnchor.MiddleCenter, new Color(0.88f, 0.92f, 0.97f, 1f));
            summary.text = summaryValue;
            summary.lineSpacing = 1.25f;
            SetAnchored(summary.rectTransform, new Vector2(0f, -276f), new Vector2(660f, 220f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            Button retryButton = CreateButton(panel, "RetryButton", new Vector2(0f, -572f), new Vector2(560f, 72f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Color(0.2f, 0.48f, 0.32f, 1f));
            SetButtonText(retryButton, "다시 시작");
            retryButton.onClick.AddListener(retryAction);

            Button menuButton = CreateButton(panel, "MenuButton", new Vector2(0f, -668f), new Vector2(560f, 72f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Color(0.2f, 0.26f, 0.36f, 1f));
            SetButtonText(menuButton, "메인 메뉴");
            menuButton.onClick.AddListener(menuAction);
        }

        private GameObject CreateOverlay(string name, Color color)
        {
            GameObject overlay = CreateImage(transform, name, color).gameObject;
            SetStretch(overlay.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
            return overlay;
        }

        private static void RemoveOverlay(ref GameObject overlay)
        {
            if (overlay == null)
            {
                return;
            }

            overlay.SetActive(false);
            Object.Destroy(overlay);
            overlay = null;
        }

        private static void CreateStatusRow(RectTransform parent, float y, string iconPath, string initialLabel, Color fillColor, out Image fill, out Text label)
        {
            RectTransform row = CreatePanel(parent, "StatusRow", new Vector2(0f, -y), new Vector2(366f, 66f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            if (!string.IsNullOrEmpty(iconPath))
            {
                CreateIcon(row, "Icon", iconPath, new Vector2(28f, 0f), new Vector2(36f, 36f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            }

            label = CreateText(row, "Label", 23, TextAnchor.UpperLeft, Color.white);
            label.text = initialLabel;
            SetAnchored(label.rectTransform, new Vector2(string.IsNullOrEmpty(iconPath) ? 20f : 72f, -12f), new Vector2(276f, 26f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            Image background = CreateImage(row, "BarBackground", BarBackgroundColor);
            SetAnchored(background.rectTransform, new Vector2(20f, 12f), new Vector2(326f, 14f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            fill = CreateImage(background.transform, "Fill", fillColor);
            fill.type = Image.Type.Simple;
            SetStretch(fill.rectTransform, 0f, 0f, 0f, 0f);
        }

        private static RectTransform CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot)
        {
            Image image = CreateImage(parent, name, PanelColor);
            SetAnchored(image.rectTransform, position, size, anchor, pivot);
            return image.rectTransform;
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            Image image = gameObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static void CreateIcon(Transform parent, string name, string iconPath, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot)
        {
            Image image = CreateImage(parent, name, Color.white);
            image.sprite = UiIconDrawer.GetSprite(iconPath);
            image.preserveAspect = true;
            SetAnchored(image.rectTransform, position, size, anchor, pivot);
        }

        private static Text CreateText(Transform parent, string name, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            gameObject.transform.SetParent(parent, false);
            Text text = gameObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot, Color color)
        {
            Image image = CreateImage(parent, name, color);
            SetAnchored(image.rectTransform, position, size, anchor, pivot);
            Button button = image.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            button.colors = colors;
            return button;
        }

        private static void SetButtonText(Button button, string value)
        {
            Text text = CreateText(button.transform, "Text", 28, TextAnchor.MiddleCenter, Color.white);
            text.fontStyle = FontStyle.Bold;
            text.text = value;
            SetStretch(text.rectTransform, 0f, 0f, 0f, 0f);
        }

        private static void SetBar(Image fill, int current, int max)
        {
            if (fill == null)
            {
                return;
            }

            float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            RectTransform rectTransform = fill.rectTransform;
            rectTransform.anchorMax = new Vector2(ratio, 1f);
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void SetAnchored(RectTransform rectTransform, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot)
        {
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
        }

        private static void SetStretch(RectTransform rectTransform, float left, float bottom, float right, float top)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(left, bottom);
            rectTransform.offsetMax = new Vector2(-right, -top);
        }

        private static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }
    }
}
