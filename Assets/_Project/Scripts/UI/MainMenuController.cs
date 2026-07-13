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

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;
        private GUIStyle secondaryButtonStyle;

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

            Rect infoRect = new Rect(Screen.width * 0.5f - 190f, Screen.height * 0.36f, 380f, 142f);
            GUI.Box(infoRect, GUIContent.none);
            GUI.Label(new Rect(infoRect.x + 28f, infoRect.y + 22f, infoRect.width - 56f, infoRect.height - 44f), GetRecordText(), bodyStyle);

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
                return "Last Run\nNo run yet.\n\nBest\nSurvival 00:00   Kills 0";
            }

            string result = RunResultData.LastCleared ? "Cleared" : "Failed";
            return
                $"Last Run\n{result}   Survival {FormatTime(RunResultData.LastElapsedSeconds)}   Kills {RunResultData.LastKillCount}\n\n" +
                $"Best\nSurvival {FormatTime(RunResultData.BestElapsedSeconds)}   Kills {RunResultData.BestKillCount}";
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
            return new Rect(Screen.width * 0.5f - 145f, Screen.height * 0.66f, 290f, 54f);
        }

        private static Rect GetResetButtonRect()
        {
            return new Rect(Screen.width * 0.5f - 145f, Screen.height * 0.66f + 68f, 290f, 42f);
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
        }
    }
}
