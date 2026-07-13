using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PawVoyage.UI
{
    /// <summary>
    /// 모바일 테스트와 에디터 테스트를 위한 일시정지 패널입니다.
    /// </summary>
    public class PausePanel : MonoBehaviour
    {
        [SerializeField] private string pauseButtonText = "II";
        [SerializeField] private string titleText = "PAUSED";
        [SerializeField] private string resumeText = "RESUME";
        [SerializeField] private string retryText = "RETRY";

        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;
        private GUIStyle smallButtonStyle;
        private bool isPaused;
        private float previousTimeScale = 1f;

        private void OnDisable()
        {
            if (isPaused)
            {
                Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
            }
        }

        private void Update()
        {
            HandlePointerInput();

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame)
            {
                TogglePause();
            }

            if (isPaused && keyboard.rKey.wasPressedThisFrame)
            {
                RestartScene();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (!isPaused && GUI.Button(GetPauseButtonRect(), pauseButtonText, smallButtonStyle))
            {
                Pause();
            }

            if (!isPaused)
            {
                return;
            }

            Rect panelRect = GetPanelRect();

            GUI.Box(panelRect, GUIContent.none);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 30f, panelRect.width - 48f, 38f), titleText, titleStyle);
            GUI.Label(new Rect(panelRect.x + 32f, panelRect.y + 82f, panelRect.width - 64f, 34f), "Take a breath, then jump back in.", bodyStyle);

            if (GUI.Button(GetResumeButtonRect(), resumeText, buttonStyle))
            {
                Resume();
            }

            if (GUI.Button(GetRetryButtonRect(), retryText, buttonStyle))
            {
                RestartScene();
            }
        }

        private void HandlePointerInput()
        {
            if (!TryGetPressedScreenPosition(out Vector2 screenPosition))
            {
                return;
            }

            Vector2 guiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            if (!isPaused)
            {
                if (GetPauseButtonRect().Contains(guiPosition))
                {
                    Pause();
                }

                return;
            }

            if (GetResumeButtonRect().Contains(guiPosition))
            {
                Resume();
            }
            else if (GetRetryButtonRect().Contains(guiPosition))
            {
                RestartScene();
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

        private static Rect GetPauseButtonRect()
        {
            return new Rect(Screen.width - 78f, 24f, 54f, 46f);
        }

        private static Rect GetPanelRect()
        {
            return new Rect(
                Screen.width * 0.5f - 180f,
                Screen.height * 0.5f - 135f,
                360f,
                270f);
        }

        private static Rect GetResumeButtonRect()
        {
            Rect panelRect = GetPanelRect();
            return new Rect(panelRect.x + 58f, panelRect.y + 136f, panelRect.width - 116f, 46f);
        }

        private static Rect GetRetryButtonRect()
        {
            Rect panelRect = GetPanelRect();
            return new Rect(panelRect.x + 58f, panelRect.y + 194f, panelRect.width - 116f, 46f);
        }

        private void TogglePause()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        private void Pause()
        {
            if (isPaused || Time.timeScale <= 0f)
            {
                return;
            }

            isPaused = true;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        private void Resume()
        {
            if (!isPaused)
            {
                return;
            }

            isPaused = false;
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        }

        private void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

            smallButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
        }
    }
}
