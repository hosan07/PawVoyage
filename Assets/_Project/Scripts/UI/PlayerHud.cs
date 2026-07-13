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
        [SerializeField] private Vector2 barSize = new Vector2(280f, 18f);
        [SerializeField] private float rowSpacing = 34f;

        private static Texture2D whiteTexture;

        private Health health;
        private PlayerExperience playerExperience;
        private RunStats runStats;
        private GUIStyle labelStyle;

        private void Awake()
        {
            health = GetComponent<Health>();
            playerExperience = GetComponent<PlayerExperience>();
            runStats = GetComponent<RunStats>();
            EnsureTexture();
        }

        private void OnGUI()
        {
            EnsureStyle();

            float width = Mathf.Min(barSize.x, Screen.width - position.x * 2f);
            DrawStatusRow(
                new Rect(position.x, position.y, width, barSize.y),
                $"HP {health.CurrentHp}/{health.MaxHp}",
                health.CurrentHp,
                health.MaxHp,
                new Color(0.9f, 0.18f, 0.18f));

            DrawStatusRow(
                new Rect(position.x, position.y + rowSpacing, width, barSize.y),
                $"LV {playerExperience.CurrentLevel}  EXP {playerExperience.CurrentExp}/{playerExperience.ExpToNextLevel}",
                playerExperience.CurrentExp,
                playerExperience.ExpToNextLevel,
                new Color(0.18f, 0.85f, 0.3f));

            GUI.Label(
                new Rect(position.x, position.y + rowSpacing * 2f - 12f, width, 22f),
                $"TIME {FormatTime(runStats != null ? runStats.ElapsedSeconds : 0f)} / {FormatTime(runStats != null ? runStats.ClearTimeSeconds : 0f)}   KILLS {runStats?.KillCount ?? 0}",
                labelStyle);
        }

        private void DrawStatusRow(Rect barRect, string label, int current, int max, Color fillColor)
        {
            float fillRatio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * fillRatio, barRect.height);
            Rect labelRect = new Rect(barRect.x, barRect.y - 20f, barRect.width, 18f);

            DrawRect(new Rect(barRect.x - 2f, barRect.y - 2f, barRect.width + 4f, barRect.height + 4f), Color.black);
            DrawRect(barRect, new Color(0.12f, 0.12f, 0.12f, 0.9f));
            DrawRect(fillRect, fillColor);
            GUI.Label(labelRect, label, labelStyle);
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
    }
}
