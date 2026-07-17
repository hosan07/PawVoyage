using PawVoyage.Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PawVoyage.UI
{
    /// <summary>
    /// 런 목표 달성 시 게임을 멈추고 클리어 결과를 표시합니다.
    /// </summary>
    [RequireComponent(typeof(RunStats))]
    public class StageClearPanel : MonoBehaviour
    {
        [SerializeField] private string titleText = "STAGE CLEAR";
        [SerializeField] private string retryText = "RETRY";
        [SerializeField] private string menuText = "MENU";
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private RunStats runStats;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;
        private bool isOpen;
        private float previousTimeScale = 1f;

        private void Awake()
        {
            runStats = GetComponent<RunStats>();
        }

        private void OnEnable()
        {
            runStats.RunCleared += OnRunCleared;
        }

        private void OnDisable()
        {
            runStats.RunCleared -= OnRunCleared;

            if (isOpen)
            {
                Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
            }
        }

        private void Update()
        {
            if (!isOpen)
            {
                return;
            }

            HandlePointerInput();

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                RestartScene();
            }
            else if (keyboard != null && keyboard.mKey.wasPressedThisFrame)
            {
                LoadMainMenu();
            }
        }

        private void OnGUI()
        {
            if (!isOpen)
            {
                return;
            }

            EnsureStyles();

            Rect panelRect = GetPanelRect();

            GUI.Box(panelRect, GUIContent.none);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 28f, panelRect.width - 48f, 38f), titleText, titleStyle);
            GUI.Label(
                new Rect(panelRect.x + 32f, panelRect.y + 78f, panelRect.width - 64f, 82f),
                GetRunSummaryText(),
                bodyStyle);

            if (GUI.Button(GetRetryButtonRect(), retryText, buttonStyle))
            {
                RestartScene();
            }

            if (GUI.Button(GetMenuButtonRect(), menuText, buttonStyle))
            {
                LoadMainMenu();
            }
        }

        private void HandlePointerInput()
        {
            if (!TryGetPressedScreenPosition(out Vector2 screenPosition))
            {
                return;
            }

            Vector2 guiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            if (GetRetryButtonRect().Contains(guiPosition))
            {
                RestartScene();
            }
            else if (GetMenuButtonRect().Contains(guiPosition))
            {
                LoadMainMenu();
            }
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

        private static Rect GetPanelRect()
        {
            return new Rect(
                Screen.width * 0.5f - 190f,
                Screen.height * 0.5f - 140f,
                380f,
                280f);
        }

        private static Rect GetRetryButtonRect()
        {
            Rect panelRect = GetPanelRect();
            return new Rect(panelRect.x + 48f, panelRect.y + 192f, 132f, 48f);
        }

        private static Rect GetMenuButtonRect()
        {
            Rect panelRect = GetPanelRect();
            return new Rect(panelRect.x + panelRect.width - 180f, panelRect.y + 192f, 132f, 48f);
        }

        private void OnRunCleared()
        {
            if (isOpen)
            {
                return;
            }

            isOpen = true;
            RunResultData.RecordResult(true, runStats.ElapsedSeconds, runStats.KillCount, runStats.CoinsCollected);
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        private void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private string GetRunSummaryText()
        {
            string bonusText = runStats.BonusCoinsCollected > 0 ? $" (+{runStats.BonusCoinsCollected} Bonus)" : string.Empty;
            return $"Survived {FormatTime(runStats.ElapsedSeconds)}\nKills {runStats.KillCount}\nCoins {runStats.CoinsCollected}{bonusText}";
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
                fontSize = 30,
                fontStyle = FontStyle.Bold,
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
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
        }

        private static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int minutes = totalSeconds / 60;
            int remainingSeconds = totalSeconds % 60;
            return $"{minutes:00}:{remainingSeconds:00}";
        }
    }
}
