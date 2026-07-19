using PawVoyage.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace PawVoyage.UI
{
    /// <summary>
    /// 동료 선택, 영구 강화, 출전을 한 화면에서 제공하는 모바일 Canvas 메뉴입니다.
    /// </summary>
    public class MobileMainMenuCanvas : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color(0.06f, 0.09f, 0.14f, 0.92f);
        private MainMenuController menu;
        private Text coinText;
        private Text recordText;
        private Text companionText;
        private readonly Text[] farmerUpgradeTexts = new Text[5];
        private readonly Text[] farmUpgradeTexts = new Text[2];

        private readonly MetaUpgradeType[] farmerUpgrades =
        {
            MetaUpgradeType.Damage,
            MetaUpgradeType.MaxHp,
            MetaUpgradeType.AttackSpeed,
            MetaUpgradeType.MoveSpeed,
            MetaUpgradeType.PickupRadius
        };

        private readonly MetaUpgradeType[] farmUpgrades =
        {
            MetaUpgradeType.BarnMaxHp,
            MetaUpgradeType.BarnDefense
        };

        public static MobileMainMenuCanvas CreateOrGet(MainMenuController source)
        {
            MobileMainMenuCanvas existing = Object.FindFirstObjectByType<MobileMainMenuCanvas>();
            if (existing != null)
            {
                existing.menu = source;
                return existing;
            }

            GameObject root = new GameObject("MobileMainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            EnsureEventSystem();

            MobileMainMenuCanvas mobileMenu = root.AddComponent<MobileMainMenuCanvas>();
            mobileMenu.menu = source;
            mobileMenu.Build();
            return mobileMenu;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private void Update()
        {
            Refresh();
        }

        private void Build()
        {
            Text title = CreateText(transform, "Title", 58, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.38f, 1f));
            title.fontStyle = FontStyle.Bold;
            title.text = "Paw Voyage";
            SetAnchored(title.rectTransform, new Vector2(0f, -98f), new Vector2(900f, 76f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            Text subtitle = CreateText(transform, "Subtitle", 22, TextAnchor.MiddleCenter, new Color(0.82f, 0.88f, 0.95f, 1f));
            subtitle.text = "첫 번째 밤, 헛간을 지켜라";
            SetAnchored(subtitle.rectTransform, new Vector2(0f, -166f), new Vector2(900f, 36f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            CreateIcon(transform, "CoinIcon", UiIconDrawer.FarmCoin, new Vector2(-146f, -52f), new Vector2(36f, 36f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            coinText = CreateText(transform, "CoinText", 28, TextAnchor.MiddleRight, Color.white);
            SetAnchored(coinText.rectTransform, new Vector2(-186f, -52f), new Vector2(170f, 42f), new Vector2(1f, 1f), new Vector2(1f, 1f));

            RectTransform companionPanel = CreatePanel(transform, "CompanionPanel", new Vector2(0f, -280f), new Vector2(900f, 250f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            AddPanelTitle(companionPanel, "동료 선택");
            Button dogButton = CreateButton(companionPanel, "DogButton", new Vector2(-218f, -142f), new Vector2(360f, 108f), new Color(0.56f, 0.34f, 0.17f, 1f));
            CreateIcon(dogButton.transform, "DogIcon", UiIconDrawer.CompanionDog, new Vector2(34f, 0f), new Vector2(66f, 66f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            SetButtonText(dogButton, "DOG", 29);
            dogButton.onClick.AddListener(() => menu?.SelectCompanion(SelectedAnimalType.Dog));

            Button catButton = CreateButton(companionPanel, "CatButton", new Vector2(218f, -142f), new Vector2(360f, 108f), new Color(0.23f, 0.35f, 0.52f, 1f));
            CreateIcon(catButton.transform, "CatIcon", UiIconDrawer.CompanionCat, new Vector2(34f, 0f), new Vector2(66f, 66f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            SetButtonText(catButton, "CAT", 29);
            catButton.onClick.AddListener(() => menu?.SelectCompanion(SelectedAnimalType.Cat));

            companionText = CreateText(companionPanel, "CompanionStatus", 20, TextAnchor.MiddleCenter, new Color(0.86f, 0.91f, 0.98f, 1f));
            SetAnchored(companionText.rectTransform, new Vector2(0f, -208f), new Vector2(760f, 28f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            RectTransform farmerUpgradePanel = CreatePanel(transform, "FarmerUpgradePanel", new Vector2(0f, -520f), new Vector2(900f, 610f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            AddPanelTitle(farmerUpgradePanel, "농부 영구 강화");
            for (int i = 0; i < farmerUpgrades.Length; i++)
            {
                int index = i;
                Button upgradeButton = CreateButton(farmerUpgradePanel, "FarmerUpgradeButton", new Vector2(0f, -92f - i * 96f), new Vector2(820f, 78f), new Color(0.13f, 0.2f, 0.3f, 1f));
                farmerUpgradeTexts[i] = CreateText(upgradeButton.transform, "Label", 20, TextAnchor.MiddleLeft, Color.white);
                SetStretch(farmerUpgradeTexts[i].rectTransform, 26f, 8f, 26f, 8f);
                upgradeButton.onClick.AddListener(() => menu?.TryBuyUpgrade(farmerUpgrades[index]));
            }

            RectTransform farmUpgradePanel = CreatePanel(transform, "FarmUpgradePanel", new Vector2(0f, -1160f), new Vector2(900f, 300f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            AddPanelTitle(farmUpgradePanel, "농장 영구 강화");
            for (int i = 0; i < farmUpgrades.Length; i++)
            {
                int index = i;
                Button upgradeButton = CreateButton(farmUpgradePanel, "FarmUpgradeButton", new Vector2(0f, -92f - i * 92f), new Vector2(820f, 74f), new Color(0.28f, 0.18f, 0.12f, 1f));
                farmUpgradeTexts[i] = CreateText(upgradeButton.transform, "Label", 20, TextAnchor.MiddleLeft, Color.white);
                SetStretch(farmUpgradeTexts[i].rectTransform, 26f, 8f, 26f, 8f);
                upgradeButton.onClick.AddListener(() => menu?.TryBuyUpgrade(farmUpgrades[index]));
            }

            RectTransform recordPanel = CreatePanel(transform, "RecordPanel", new Vector2(0f, -1500f), new Vector2(900f, 270f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            AddPanelTitle(recordPanel, "최근 기록");
            recordText = CreateText(recordPanel, "RecordText", 21, TextAnchor.UpperLeft, new Color(0.84f, 0.9f, 0.97f, 1f));
            SetAnchored(recordText.rectTransform, new Vector2(42f, -76f), new Vector2(816f, 170f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            Button deployButton = CreateButton(transform, "DeployButton", Vector2.zero, new Vector2(900f, 112f), new Color(0.26f, 0.62f, 0.34f, 1f));
            SetAnchored(deployButton.GetComponent<RectTransform>(), new Vector2(0f, 100f), new Vector2(900f, 112f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            SetButtonText(deployButton, "STAGE 1 출전", 36);
            deployButton.onClick.AddListener(() => menu?.StartGame());
        }

        private void Refresh()
        {
            if (coinText == null)
            {
                return;
            }

            coinText.text = $"{RunResultData.TotalCoins}";
            companionText.text = $"선택한 동료: {AnimalSelectionData.SelectedCompanionName}";
            RefreshUpgradeLabels(farmerUpgrades, farmerUpgradeTexts);
            RefreshUpgradeLabels(farmUpgrades, farmUpgradeTexts);

            string clear = RunResultData.Stage1MvpCleared ? "클리어" : "도전 중";
            recordText.text = $"Stage 1: {clear}\n최고 생존 {FormatTime(RunResultData.BestElapsedSeconds)}   최고 처치 {RunResultData.BestKillCount}\n최근 코인 {RunResultData.LastCoinCount}   레벨업 {RunResultData.LastLevelUpCount}\n적 {RunResultData.LastTotalEnemiesSpawned}  헛간 타깃 {RunResultData.LastBarnTargetEnemiesSpawned}  최대 동시 {RunResultData.LastPeakAliveEnemies}";
        }

        private static void RefreshUpgradeLabels(MetaUpgradeType[] upgrades, Text[] labels)
        {
            for (int i = 0; i < upgrades.Length; i++)
            {
                MetaUpgradeType type = upgrades[i];
                int level = MetaProgressionData.GetLevel(type);
                string cost = MetaProgressionData.IsMaxLevel(type) ? "MAX" : $"{MetaProgressionData.GetCost(type)} 코인";
                string tier = MetaProgressionData.IsAdvancedLevel(type) ? "고급" : "기본";
                labels[i].text = $"{MetaProgressionData.GetDisplayName(type)}  Lv {level}/{MetaProgressionData.MaxLevel}  {tier}\n{MetaProgressionData.GetEffectText(type)}     {cost}";
            }
        }

        private static RectTransform CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot)
        {
            Image image = CreateImage(parent, name, PanelColor);
            SetAnchored(image.rectTransform, position, size, anchor, pivot);
            return image.rectTransform;
        }

        private static void AddPanelTitle(RectTransform panel, string value)
        {
            Text title = CreateText(panel, "Title", 28, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.38f, 1f));
            title.fontStyle = FontStyle.Bold;
            title.text = value;
            SetAnchored(title.rectTransform, new Vector2(0f, -38f), new Vector2(780f, 42f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
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

        private static Text CreateText(Transform parent, string name, int size, TextAnchor alignment, Color color)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            gameObject.transform.SetParent(parent, false);
            Text text = gameObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            Image image = CreateImage(parent, name, color);
            SetAnchored(image.rectTransform, position, size, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        private static void SetButtonText(Button button, string value, int size)
        {
            Text text = CreateText(button.transform, "Text", size, TextAnchor.MiddleCenter, Color.white);
            text.fontStyle = FontStyle.Bold;
            text.text = value;
            SetStretch(text.rectTransform, 0f, 0f, 0f, 0f);
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
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}
