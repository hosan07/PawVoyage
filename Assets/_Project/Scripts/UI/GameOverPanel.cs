using System;
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
        [SerializeField] private bool useCanvasUi = true;

        private Health health;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;
        private bool isGameOver;
        private RunFailureReason failureReason = RunFailureReason.None;
        private float previousTimeScale = 1f;

        public event Action<GameOverPanel> PanelOpened;
        public string CanvasTitle => GetCanvasTitle();
        public string CanvasSummary => GetCanvasSummary();

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health.Died += OnPlayerDied;
            if (RunStats.Instance != null)
            {
                RunStats.Instance.RunFailed += OnRunFailed;
            }
        }

        private void OnDisable()
        {
            health.Died -= OnPlayerDied;
            if (RunStats.Instance != null)
            {
                RunStats.Instance.RunFailed -= OnRunFailed;
            }

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

            if (!useCanvasUi)
            {
                HandlePointerInput();
            }

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
            if (!isGameOver || useCanvasUi)
            {
                return;
            }

            EnsureStyles();

            Rect panelRect = GetPanelRect();

            GUI.Box(panelRect, GUIContent.none);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 28f, panelRect.width - 48f, 38f), GetTitleText(), titleStyle);
            GUI.Label(new Rect(panelRect.x + 32f, panelRect.y + 76f, panelRect.width - 64f, 154f), GetRunSummaryText(), bodyStyle);

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
                Screen.height * 0.5f - 174f,
                350f,
                348f);
        }

        private static Rect GetRetryButtonRect()
        {
            Rect panelRect = GetPanelRect();
            return new Rect(panelRect.x + 38f, panelRect.y + 260f, 132f, 48f);
        }

        private static Rect GetMenuButtonRect()
        {
            Rect panelRect = GetPanelRect();
            return new Rect(panelRect.x + panelRect.width - 170f, panelRect.y + 260f, 132f, 48f);
        }

        private void OnPlayerDied(Health deadHealth)
        {
            RunStats.Instance?.FailRun(RunFailureReason.FarmerDeath);
        }

        private void OnRunFailed(RunFailureReason reason)
        {
            if (isGameOver)
            {
                return;
            }

            failureReason = reason;
            isGameOver = true;
            RecordResult(false, reason);
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            PanelOpened?.Invoke(this);
        }

        private static void RecordResult(bool cleared, RunFailureReason reason)
        {
            RunStats runStats = RunStats.Instance;
            if (runStats == null)
            {
                return;
            }

            RunResultData.RecordResult(
                cleared,
                runStats.ElapsedSeconds,
                runStats.KillCount,
                runStats.CoinsCollected,
                runStats.LevelUpCount,
                runStats.HitCount,
                runStats.DamageTaken,
                reason.ToString(),
                runStats.BarnDamageTaken,
                runStats.BarnCurrentHp,
                runStats.BarnMaxHp,
                runStats.BarnDestroyed,
                runStats.SelectedWeaponsSummary,
                runStats.MiniBossSeen,
                false,
                runStats.TotalEnemiesSpawned,
                runStats.BarnTargetEnemiesSpawned,
                runStats.PeakAliveEnemies);
        }

        public void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private static string GetRunSummaryText()
        {
            RunStats runStats = RunStats.Instance;
            if (runStats == null)
            {
                return "Run ended. Try again with a fresh start.";
            }

            string bonusText = runStats.BonusCoinsCollected > 0 ? $" (+{runStats.BonusCoinsCollected} Bonus)" : string.Empty;
            string bossText = runStats.MiniBossSeen ? "Seen" : "Not Seen";
            string barnText = runStats.BarnMaxHp > 0 ? $"Barn {runStats.BarnCurrentHp}/{runStats.BarnMaxHp}   Barn Damage {runStats.BarnDamageTaken}" : "Barn Not Found";
            return $"Survived {FormatTime(runStats.ElapsedSeconds)}\nKills {runStats.KillCount}   Coins {runStats.CoinsCollected}{bonusText}\nLevel Ups {runStats.LevelUpCount}   Mini Boss {bossText}\nDamage Taken {runStats.DamageTaken}   Hits {runStats.HitCount}\n{barnText}\nWeapons {runStats.SelectedWeaponsSummary}";
        }

        private string GetTitleText()
        {
            return failureReason switch
            {
                RunFailureReason.BarnDestroyed => "BARN DESTROYED",
                RunFailureReason.FarmerDeath => "FARMER DOWN",
                _ => titleText
            };
        }

        private string GetCanvasTitle()
        {
            return failureReason switch
            {
                RunFailureReason.BarnDestroyed => "헛간 파괴",
                RunFailureReason.FarmerDeath => "농부가 쓰러졌습니다",
                _ => "전투 실패"
            };
        }

        private static string GetCanvasSummary()
        {
            RunStats runStats = RunStats.Instance;
            if (runStats == null)
            {
                return "이번 전투 기록을 불러올 수 없습니다.";
            }

            string barnText = runStats.BarnMaxHp > 0
                ? $"헛간 {runStats.BarnCurrentHp}/{runStats.BarnMaxHp}  피해 {runStats.BarnDamageTaken}"
                : "헛간 기록 없음";
            return $"생존 {FormatTime(runStats.ElapsedSeconds)}   처치 {runStats.KillCount}\n" +
                   $"코인 {runStats.CoinsCollected}   레벨업 {runStats.LevelUpCount}\n" +
                   $"받은 피해 {runStats.DamageTaken}   피격 {runStats.HitCount}\n" +
                   $"{barnText}\n" +
                   $"압박 적 {runStats.TotalEnemiesSpawned}  헛간 타깃 {runStats.BarnTargetEnemiesSpawned}  최대 동시 {runStats.PeakAliveEnemies}\n" +
                   $"획득 무기 {runStats.SelectedWeaponsSummary}";
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
