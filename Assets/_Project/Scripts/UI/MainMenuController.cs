using PawVoyage.Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PawVoyage.UI
{
    /// <summary>
    /// 시작 화면과 저장된 런 기록 표시를 담당합니다.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private string titleText = "Paw Voyage";
        [SerializeField] private string subtitleText = "Survive the first trail with your tiny paw hero.";
        [SerializeField] private string startText = "START";
        [SerializeField] private string resetText = "RESET RECORDS";
        [SerializeField] private string dogText = "DOG";
        [SerializeField] private string catText = "CAT";

        private readonly MetaUpgradeType[] shopUpgrades =
        {
            MetaUpgradeType.Damage,
            MetaUpgradeType.MaxHp,
            MetaUpgradeType.AttackSpeed,
            MetaUpgradeType.MoveSpeed,
            MetaUpgradeType.PickupRadius
        };

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;
        private GUIStyle secondaryButtonStyle;
        private GUIStyle shopTitleStyle;
        private GUIStyle feedbackStyle;
        private string shopFeedbackText = string.Empty;
        private float shopFeedbackEndTime;
        private int lastShopActionFrame = -1;

        private void Awake()
        {
            Time.timeScale = 1f;
        }

        private void Update()
        {
            HandlePointerInput();

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                StartGame();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            GUI.Label(new Rect(0f, Screen.height * 0.16f, Screen.width, 54f), titleText, titleStyle);
            GUI.Label(new Rect(Screen.width * 0.5f - 210f, Screen.height * 0.16f + 58f, 420f, 48f), subtitleText, subtitleStyle);

            Rect infoRect = GetInfoRect();
            GUI.Box(infoRect, GUIContent.none);
            GUI.Label(new Rect(infoRect.x + 28f, infoRect.y + 22f, infoRect.width - 56f, infoRect.height - 44f), GetRecordText(), bodyStyle);

            DrawAnimalSelector();
            DrawShop();
            DrawShopFeedback();

            if (GUI.Button(GetStartButtonRect(), startText, buttonStyle))
            {
                StartGame();
            }

            if (GUI.Button(GetResetButtonRect(), resetText, secondaryButtonStyle))
            {
                RunResultData.ResetRecords();
            }
        }

        private void HandlePointerInput()
        {
            if (!TryGetPressedScreenPosition(out Vector2 screenPosition))
            {
                return;
            }

            Vector2 guiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            if (GetStartButtonRect().Contains(guiPosition))
            {
                StartGame();
            }
            else if (GetResetButtonRect().Contains(guiPosition))
            {
                RunResultData.ResetRecords();
            }
            else if (GetDogButtonRect().Contains(guiPosition))
            {
                AnimalSelectionData.SelectAnimal(SelectedAnimalType.Dog);
            }
            else if (GetCatButtonRect().Contains(guiPosition))
            {
                AnimalSelectionData.SelectAnimal(SelectedAnimalType.Cat);
            }

            for (int i = 0; i < shopUpgrades.Length; i++)
            {
                if (GetShopButtonRect(i).Contains(guiPosition))
                {
                    TryBuyUpgrade(shopUpgrades[i]);
                    return;
                }
            }
        }

        private void StartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneName);
        }

        private string GetRecordText()
        {
            if (!RunResultData.HasLastResult)
            {
                return $"Selected {AnimalSelectionData.SelectedAnimalName}   Stage 1 {(RunResultData.Stage1MvpCleared ? "Cleared" : "Uncleared")}\n\nLast Run\nNo run yet.\n\nBest\nSurvival 00:00   Kills 0\n\nCoins\nAvailable {RunResultData.TotalCoins}";
            }

            string result = RunResultData.LastCleared ? "Cleared" : "Failed";
            return
                $"Selected {AnimalSelectionData.SelectedAnimalName}   Stage 1 {(RunResultData.Stage1MvpCleared ? "Cleared" : "Uncleared")}\n\n" +
                $"Last Run\n{result}   Survival {FormatTime(RunResultData.LastElapsedSeconds)}   Kills {RunResultData.LastKillCount}   Coins {RunResultData.LastCoinCount}\n" +
                $"Level Ups {RunResultData.LastLevelUpCount}   Mini Boss {(RunResultData.LastMiniBossSeen ? "Seen" : "Not Seen")}\n" +
                $"Weapons {RunResultData.LastSelectedWeapons}\n\n" +
                $"Best\nSurvival {FormatTime(RunResultData.BestElapsedSeconds)}   Kills {RunResultData.BestKillCount}\n\n" +
                $"Coins\nAvailable {RunResultData.TotalCoins}";
        }

        private void DrawAnimalSelector()
        {
            Rect selectorRect = GetAnimalSelectorRect();
            GUI.Box(selectorRect, GUIContent.none);
            GUI.Label(new Rect(selectorRect.x + 16f, selectorRect.y + 10f, selectorRect.width - 32f, 22f), "CHARACTER", shopTitleStyle);

            SelectedAnimalType selectedAnimal = AnimalSelectionData.SelectedAnimal;
            string dogLabel = selectedAnimal == SelectedAnimalType.Dog ? $"{dogText} SELECTED" : dogText;
            string catLabel = selectedAnimal == SelectedAnimalType.Cat ? $"{catText} SELECTED" : catText;

            if (GUI.Button(GetDogButtonRect(), dogLabel, secondaryButtonStyle))
            {
                AnimalSelectionData.SelectAnimal(SelectedAnimalType.Dog);
            }

            if (GUI.Button(GetCatButtonRect(), catLabel, secondaryButtonStyle))
            {
                AnimalSelectionData.SelectAnimal(SelectedAnimalType.Cat);
            }
        }

        private void DrawShop()
        {
            Rect shopRect = GetShopRect();
            GUI.Box(shopRect, GUIContent.none);
            GUI.Label(new Rect(shopRect.x + 20f, shopRect.y + 12f, shopRect.width - 40f, 28f), "UPGRADES", shopTitleStyle);
            GUI.Label(new Rect(shopRect.x + 20f, shopRect.y + 40f, shopRect.width - 40f, 22f), $"Coins Available: {RunResultData.TotalCoins}", bodyStyle);

            for (int i = 0; i < shopUpgrades.Length; i++)
            {
                MetaUpgradeType upgradeType = shopUpgrades[i];
                int level = MetaProgressionData.GetLevel(upgradeType);
                string costText = MetaProgressionData.IsMaxLevel(upgradeType) ? "MAX" : $"{MetaProgressionData.GetCost(upgradeType)} Coins";
                string label = $"{MetaProgressionData.GetDisplayName(upgradeType)}  Lv {level}/{MetaProgressionData.MaxLevel}\n{MetaProgressionData.GetEffectText(upgradeType)}   {costText}";

                if (GUI.Button(GetShopButtonRect(i), label, secondaryButtonStyle))
                {
                    TryBuyUpgrade(upgradeType);
                }
            }
        }

        private void TryBuyUpgrade(MetaUpgradeType upgradeType)
        {
            if (lastShopActionFrame == Time.frameCount)
            {
                return;
            }

            lastShopActionFrame = Time.frameCount;

            if (MetaProgressionData.IsMaxLevel(upgradeType))
            {
                ShowShopFeedback($"{MetaProgressionData.GetDisplayName(upgradeType)} is already maxed.");
                return;
            }

            int cost = MetaProgressionData.GetCost(upgradeType);
            if (RunResultData.TotalCoins < cost)
            {
                ShowShopFeedback($"Need {cost - RunResultData.TotalCoins} more coins.");
                return;
            }

            if (MetaProgressionData.TryPurchase(upgradeType))
            {
                ShowShopFeedback($"{MetaProgressionData.GetDisplayName(upgradeType)} upgraded!");
            }
        }

        private void DrawShopFeedback()
        {
            if (Time.unscaledTime >= shopFeedbackEndTime || string.IsNullOrEmpty(shopFeedbackText))
            {
                return;
            }

            Rect shopRect = GetShopRect();
            GUI.Label(new Rect(shopRect.x + 20f, shopRect.y + shopRect.height + 8f, shopRect.width - 40f, 28f), shopFeedbackText, feedbackStyle);
        }

        private void ShowShopFeedback(string message)
        {
            shopFeedbackText = message;
            shopFeedbackEndTime = Time.unscaledTime + 2.2f;
        }

        private static bool TryGetPressedScreenPosition(out Vector2 screenPosition)
        {
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPosition = mouse.position.ReadValue();
                return true;
            }

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            {
                screenPosition = touchscreen.primaryTouch.position.ReadValue();
                return true;
            }

            screenPosition = Vector2.zero;
            return false;
        }

        private static Rect GetStartButtonRect()
        {
            return new Rect(Screen.width * 0.5f - 145f, Screen.height - 150f, 290f, 54f);
        }

        private static Rect GetResetButtonRect()
        {
            return new Rect(Screen.width * 0.5f - 145f, Screen.height - 86f, 290f, 42f);
        }

        private static Rect GetInfoRect()
        {
            float width = Mathf.Min(380f, Screen.width - 48f);
            if (IsWideLayout())
            {
                return new Rect(Screen.width * 0.5f - width - 24f, Screen.height * 0.28f, width, 242f);
            }

            return new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.26f, width, 242f);
        }

        private static Rect GetAnimalSelectorRect()
        {
            float width = Mathf.Min(380f, Screen.width - 48f);
            Rect infoRect = GetInfoRect();
            return new Rect(infoRect.x, infoRect.y + infoRect.height + 12f, width, 92f);
        }

        private static Rect GetDogButtonRect()
        {
            Rect selectorRect = GetAnimalSelectorRect();
            return new Rect(selectorRect.x + 20f, selectorRect.y + 42f, selectorRect.width * 0.5f - 28f, 36f);
        }

        private static Rect GetCatButtonRect()
        {
            Rect selectorRect = GetAnimalSelectorRect();
            return new Rect(selectorRect.x + selectorRect.width * 0.5f + 8f, selectorRect.y + 42f, selectorRect.width * 0.5f - 28f, 36f);
        }

        private static Rect GetShopRect()
        {
            float width = Mathf.Min(430f, Screen.width - 40f);
            if (IsWideLayout())
            {
                return new Rect(Screen.width * 0.5f + 24f, Screen.height * 0.32f, width, 314f);
            }

            return new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.50f, width, 314f);
        }

        private static Rect GetShopButtonRect(int index)
        {
            Rect shopRect = GetShopRect();
            return new Rect(shopRect.x + 20f, shopRect.y + 70f + index * 46f, shopRect.width - 40f, 40f);
        }

        private static bool IsWideLayout()
        {
            return Screen.width > Screen.height * 1.1f;
        }

        private static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int minutes = totalSeconds / 60;
            int remainingSeconds = totalSeconds % 60;
            return $"{minutes:00}:{remainingSeconds:00}";
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 42,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };

            secondaryButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            shopTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            feedbackStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.86f, 0.18f, 1f) }
            };
        }
    }
}
