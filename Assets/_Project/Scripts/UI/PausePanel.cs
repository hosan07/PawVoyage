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

            Rect pauseButtonRect = new Rect(Screen.width - 78f, 24f, 54f, 46f);
            if (!isPaused && GUI.Button(pauseButtonRect, pauseButtonText, smallButtonStyle))
            {
                Pause();
            }

            if (!isPaused)
            {
                return;
            }

            Rect panelRect = new Rect(
                Screen.width * 0.5f - 180f,
                Screen.height * 0.5f - 135f,
                360f,
                270f);

            GUI.Box(panelRect, GUIContent.none);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 30f, panelRect.width - 48f, 38f), titleText, titleStyle);
            GUI.Label(new Rect(panelRect.x + 32f, panelRect.y + 82f, panelRect.width - 64f, 34f), "Take a breath, then jump back in.", bodyStyle);

            if (GUI.Button(new Rect(panelRect.x + 58f, panelRect.y + 136f, panelRect.width - 116f, 46f), resumeText, buttonStyle))
            {
                Resume();
            }

            if (GUI.Button(new Rect(panelRect.x + 58f, panelRect.y + 194f, panelRect.width - 116f, 46f), retryText, buttonStyle))
            {
                RestartScene();
            }
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
