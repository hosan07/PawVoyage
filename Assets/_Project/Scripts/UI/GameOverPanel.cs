using PawVoyage.Combat;
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

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                RestartScene();
            }
        }

        private void OnGUI()
        {
            if (!isGameOver)
            {
                return;
            }

            EnsureStyles();

            Rect panelRect = new Rect(
                Screen.width * 0.5f - 175f,
                Screen.height * 0.5f - 115f,
                350f,
                230f);

            GUI.Box(panelRect, GUIContent.none);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 28f, panelRect.width - 48f, 38f), titleText, titleStyle);
            GUI.Label(new Rect(panelRect.x + 32f, panelRect.y + 76f, panelRect.width - 64f, 42f), "Run ended. Try again with a fresh start.", bodyStyle);

            if (GUI.Button(new Rect(panelRect.x + 58f, panelRect.y + 145f, panelRect.width - 116f, 48f), retryText, buttonStyle))
            {
                RestartScene();
            }
        }

        private void OnPlayerDied(Health deadHealth)
        {
            if (isGameOver)
            {
                return;
            }

            isGameOver = true;
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
