using PawVoyage.Combat;
using PawVoyage.Systems;
using UnityEngine;

namespace PawVoyage.UI
{
    /// <summary>
    /// 초기 전투 루프 확인을 위한 플레이어 상태 HUD입니다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerExperience))]
    public class PlayerHud : MonoBehaviour
    {
        [SerializeField] private Vector2 position = new Vector2(24f, 36f);
        [SerializeField] private Vector2 barSize = new Vector2(300f, 22f);
        [SerializeField] private float rowSpacing = 40f;
        [SerializeField] private bool useCanvasUi = true;

        private static Texture2D whiteTexture;

        private Health health;
        private PlayerExperience playerExperience;
        private RunStats runStats;
        private WaveSpawner waveSpawner;
        private BarnObjective barnObjective;
        private GUIStyle labelStyle;

        private void Awake()
        {
            health = GetComponent<Health>();
            playerExperience = GetComponent<PlayerExperience>();
            runStats = GetComponent<RunStats>();
            EnsureTexture();

            if (useCanvasUi)
            {
                MobileHudCanvas.CreateOrGet(this);
            }
        }

        private void OnGUI()
        {
            if (useCanvasUi)
            {
                return;
            }

            EnsureStyle();

            float width = Mathf.Min(barSize.x, Screen.width - position.x * 2f);
            DrawStatusRow(
                new Rect(position.x, position.y, width, barSize.y),
                $"농부 {health.CurrentHp}/{health.MaxHp}",
                health.CurrentHp,
                health.MaxHp,
                new Color(0.9f, 0.18f, 0.18f),
                UiIconDrawer.FarmerHealth);

            DrawStatusRow(
                new Rect(position.x, position.y + rowSpacing, width, barSize.y),
                GetBarnLabel(),
                GetBarnCurrentHp(),
                GetBarnMaxHp(),
                new Color(0.72f, 0.28f, 0.12f),
                UiIconDrawer.BarnHealth);

            DrawStatusRow(
                new Rect(position.x, position.y + rowSpacing * 2f, width, barSize.y),
                $"LV {playerExperience.CurrentLevel}  EXP {playerExperience.CurrentExp}/{playerExperience.ExpToNextLevel}",
                playerExperience.CurrentExp,
                playerExperience.ExpToNextLevel,
                new Color(0.18f, 0.85f, 0.3f));

            DrawProgressRow(
                new Rect(position.x, position.y + rowSpacing * 3f, width, barSize.y),
                $"STAGE {FormatTime(runStats != null ? runStats.ElapsedSeconds : 0f)} / {FormatTime(runStats != null ? runStats.ClearTimeSeconds : 0f)}",
                GetStageProgress(),
                new Color(0.18f, 0.52f, 0.95f));

            WaveSpawner currentWaveSpawner = GetWaveSpawner();
            DrawCombatSummary(
                new Rect(position.x, position.y + rowSpacing * 4f - 10f, Mathf.Max(width, 360f), 50f),
                currentWaveSpawner);
        }

        private void DrawStatusRow(Rect barRect, string label, int current, int max, Color fillColor, string iconPath = null)
        {
            float fillRatio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * fillRatio, barRect.height);
            Rect iconRect = new Rect(barRect.x, barRect.y - 25f, 22f, 22f);
            Rect labelRect = new Rect(barRect.x + 28f, barRect.y - 25f, barRect.width - 28f, 22f);

            DrawRect(new Rect(barRect.x - 2f, barRect.y - 2f, barRect.width + 4f, barRect.height + 4f), Color.black);
            DrawRect(barRect, new Color(0.12f, 0.12f, 0.12f, 0.9f));
            DrawRect(fillRect, fillColor);
            UiIconDrawer.Draw(iconPath, iconRect, Color.white);
            GUI.Label(labelRect, label, labelStyle);
        }

        private void DrawProgressRow(Rect barRect, string label, float fillRatio, Color fillColor)
        {
            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * Mathf.Clamp01(fillRatio), barRect.height);
            Rect labelRect = new Rect(barRect.x, barRect.y - 20f, barRect.width, 18f);

            DrawRect(new Rect(barRect.x - 2f, barRect.y - 2f, barRect.width + 4f, barRect.height + 4f), Color.black);
            DrawRect(barRect, new Color(0.12f, 0.12f, 0.12f, 0.9f));
            DrawRect(fillRect, fillColor);
            GUI.Label(labelRect, label, labelStyle);
        }

        private void DrawCombatSummary(Rect rect, WaveSpawner currentWaveSpawner)
        {
            float coinIconSize = 24f;
            Rect killRect = new Rect(rect.x, rect.y, 124f, 24f);
            Rect coinIconRect = new Rect(rect.x + 130f, rect.y, coinIconSize, coinIconSize);
            Rect coinTextRect = new Rect(coinIconRect.xMax + 6f, rect.y, 110f, 24f);
            Rect phaseRect = new Rect(rect.x, rect.y + 25f, rect.width, 24f);

            GUI.Label(killRect, $"KILLS {runStats?.KillCount ?? 0}", labelStyle);
            UiIconDrawer.Draw(UiIconDrawer.FarmCoin, coinIconRect, Color.white);
            GUI.Label(coinTextRect, $"{runStats?.CoinsCollected ?? 0}", labelStyle);
            GUI.Label(
                phaseRect,
                $"PHASE {GetPhaseText(currentWaveSpawner)}   ENEMIES {GetEnemyCountText(currentWaveSpawner)}",
                labelStyle);
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previousColor;
        }

        private static void EnsureTexture()
        {
            if (whiteTexture != null)
            {
                return;
            }

            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }

        private void EnsureStyle()
        {
            if (labelStyle != null)
            {
                return;
            }

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }

        private static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int minutes = totalSeconds / 60;
            int remainingSeconds = totalSeconds % 60;
            return $"{minutes:00}:{remainingSeconds:00}";
        }

        private float GetStageProgress()
        {
            if (runStats == null || runStats.ClearTimeSeconds <= 0f)
            {
                return 0f;
            }

            return runStats.ElapsedSeconds / runStats.ClearTimeSeconds;
        }

        private WaveSpawner GetWaveSpawner()
        {
            if (waveSpawner == null)
            {
                waveSpawner = WaveSpawner.Instance;
            }

            return waveSpawner;
        }

        private BarnObjective GetBarnObjective()
        {
            if (barnObjective == null)
            {
                barnObjective = BarnObjective.Instance;
            }

            return barnObjective;
        }

        private string GetBarnLabel()
        {
            BarnObjective barn = GetBarnObjective();
            return barn != null ? $"헛간 {barn.CurrentHp}/{barn.MaxHp}" : "헛간 --/--";
        }

        private int GetBarnCurrentHp()
        {
            BarnObjective barn = GetBarnObjective();
            return barn != null ? barn.CurrentHp : 0;
        }

        private int GetBarnMaxHp()
        {
            BarnObjective barn = GetBarnObjective();
            return barn != null ? barn.MaxHp : 1;
        }

        private static string GetPhaseText(WaveSpawner currentWaveSpawner)
        {
            return currentWaveSpawner != null ? currentWaveSpawner.CurrentPhaseName : "NORMAL";
        }

        private static string GetEnemyCountText(WaveSpawner currentWaveSpawner)
        {
            return currentWaveSpawner != null
                ? $"{currentWaveSpawner.CurrentAliveEnemies}/{currentWaveSpawner.CurrentMaxAliveEnemies}"
                : "0/0";
        }
    }
}
