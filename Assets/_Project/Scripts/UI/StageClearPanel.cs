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

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                RestartScene();
            }
        }

        private void OnGUI()
        {
            if (!isOpen)
            {
                return;
            }

            EnsureStyles();

            Rect panelRect = new Rect(
                Screen.width * 0.5f - 190f,
                Screen.height * 0.5f - 130f,
                380f,
                260f);

            GUI.Box(panelRect, GUIContent.none);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 28f, panelRect.width - 48f, 38f), titleText, titleStyle);
            GUI.Label(
                new Rect(panelRect.x + 32f, panelRect.y + 78f, panelRect.width - 64f, 62f),
                $"Survived {FormatTime(runStats.ElapsedSeconds)}\nKills {runStats.KillCount}",
                bodyStyle);

            if (GUI.Button(new Rect(panelRect.x + 68f, panelRect.y + 170f, panelRect.width - 136f, 48f), retryText, buttonStyle))
            {
                RestartScene();
            }
        }

        private void OnRunCleared()
        {
            if (isOpen)
            {
                return;
            }

            isOpen = true;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
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
