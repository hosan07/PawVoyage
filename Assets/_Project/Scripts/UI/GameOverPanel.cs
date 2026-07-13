using PawVoyage.Combat;
using PawVoyage.Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PawVoyage.UI
{
    /// <summary>
    /// 플레이어 사망 시 게임을 멈추고 재시작 선택지를 표시합니다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private string titleText = "GAME OVER";
        [SerializeField] private string retryText = "RETRY";
        [SerializeField] private string menuText = "MENU";
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private Health health;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;
        private bool isGameOver;
        private float previousTimeScale = 1f;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health.Died += OnPlayerDied;
        }

        private void OnDisable()
        {
            health.Died -= OnPlayerDied;

            if (isGameOver)
            {
                Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
            }
        }

        private void Update()
        {
            if (!isGameOver)
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
            if (!isGameOver)
            {
                return;
            }

            EnsureStyles();

            Rect panelRect = GetPanelRect();

            GUI.Box(panelRect, GUIContent.none);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 28f, panelRect.width - 48f, 38f), titleText, titleStyle);
            GUI.Label(new Rect(panelRect.x + 32f, panelRect.y + 76f, panelRect.width - 64f, 42f), "Run ended. Try again with a fresh start.", bodyStyle);

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
                Screen.width * 0.5f - 175f,
                Screen.height * 0.5f - 115f,
                350f,
                230f);
        }

        private static Rect GetRetryButtonRect()
        {
            Rect panelRect = GetPanelRect();
            return new Rect(panelRect.x + 38f, panelRect.y + 145f, 132f, 48f);
        }

        private static Rect GetMenuButtonRect()
        {
            Rect panelRect = GetPanelRect();
            return new Rect(panelRect.x + panelRect.width - 170f, panelRect.y + 145f, 132f, 48f);
        }

        private void OnPlayerDied(Health deadHealth)
        {
            if (isGameOver)
            {
                return;
            }

            isGameOver = true;
            RecordResult(false);
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        private static void RecordResult(bool cleared)
        {
            RunStats runStats = RunStats.Instance;
            if (runStats == null)
            {
                return;
            }

            RunResultData.RecordResult(cleared, runStats.ElapsedSeconds, runStats.KillCount);
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
                fontSize = 15,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
        }
    }
}
