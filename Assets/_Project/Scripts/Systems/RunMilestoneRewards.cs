using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// 런 중 처치 수와 생존 시간 목표 달성 시 보너스 코인을 지급합니다.
    /// </summary>
    public class RunMilestoneRewards : MonoBehaviour
    {
        [SerializeField] private int[] killMilestones = { 5, 15, 30 };
        [SerializeField] private int[] killBonusCoins = { 10, 20, 35 };
        [SerializeField] private float[] timeMilestones = { 10f, 20f, 30f };
        [SerializeField] private int[] timeBonusCoins = { 8, 12, 20 };
        [SerializeField] private float noticeDuration = 2.2f;
        [SerializeField] private Vector2 trackerOffset = new Vector2(24f, 36f);
        [SerializeField] private Vector2 trackerSize = new Vector2(310f, 126f);

        private bool[] claimedKillMilestones;
        private bool[] claimedTimeMilestones;
        private RunStats runStats;
        private GUIStyle noticeStyle;
        private GUIStyle trackerStyle;
        private GUIStyle titleStyle;
        private GUIStyle smallStyle;
        private Texture2D whiteTexture;
        private string noticeText = string.Empty;
        private string lastRewardText = "No rewards yet";
        private float noticeEndTime;

        private void Awake()
        {
            claimedKillMilestones = new bool[killMilestones.Length];
            claimedTimeMilestones = new bool[timeMilestones.Length];
        }

        private void Update()
        {
            FindRunStatsIfNeeded();
            if (runStats == null || runStats.IsCleared)
            {
                return;
            }

            CheckKillMilestones();
            CheckTimeMilestones();
        }

        private void OnGUI()
        {
            FindRunStatsIfNeeded();
            if (runStats == null)
            {
                return;
            }

            EnsureStyles();
            DrawTrackerCard();

            if (Time.unscaledTime < noticeEndTime)
            {
                GUI.Label(new Rect(Screen.width * 0.5f - 180f, 132f, 360f, 44f), noticeText, noticeStyle);
            }
        }

        private void CheckKillMilestones()
        {
            for (int i = 0; i < killMilestones.Length; i++)
            {
                if (claimedKillMilestones[i] || runStats.KillCount < killMilestones[i])
                {
                    continue;
                }

                claimedKillMilestones[i] = true;
                GrantBonus(GetArrayValue(killBonusCoins, i), $"{killMilestones[i]} Kills");
            }
        }

        private void CheckTimeMilestones()
        {
            for (int i = 0; i < timeMilestones.Length; i++)
            {
                if (claimedTimeMilestones[i] || runStats.ElapsedSeconds < timeMilestones[i])
                {
                    continue;
                }

                claimedTimeMilestones[i] = true;
                GrantBonus(GetArrayValue(timeBonusCoins, i), $"{FormatTime(timeMilestones[i])} Survival");
            }
        }

        private void GrantBonus(int coinAmount, string reason)
        {
            int safeCoinAmount = Mathf.Max(0, coinAmount);
            if (safeCoinAmount <= 0)
            {
                return;
            }

            runStats.AddBonusCoins(safeCoinAmount);
            GameSfx.PlayCoin();
            noticeText = $"{reason}  +{safeCoinAmount} Coins";
            lastRewardText = $"+{safeCoinAmount} Coins  {reason}";
            noticeEndTime = Time.unscaledTime + Mathf.Max(0.1f, noticeDuration);
        }

        private void DrawTrackerCard()
        {
            Rect cardRect = GetTrackerRect();
            DrawRect(cardRect, new Color(0.04f, 0.05f, 0.06f, 0.78f));
            DrawRect(new Rect(cardRect.x, cardRect.y, 4f, cardRect.height), new Color(1f, 0.82f, 0.16f, 0.95f));

            GUI.Label(new Rect(cardRect.x + 14f, cardRect.y + 8f, cardRect.width - 28f, 20f), "RUN GOALS", titleStyle);
            DrawGoalRow(new Rect(cardRect.x + 14f, cardRect.y + 36f, cardRect.width - 28f, 28f), GetNextKillGoalLabel(), GetNextKillGoalProgress());
            DrawGoalRow(new Rect(cardRect.x + 14f, cardRect.y + 70f, cardRect.width - 28f, 28f), GetNextTimeGoalLabel(), GetNextTimeGoalProgress());

            GUI.Label(
                new Rect(cardRect.x + 14f, cardRect.y + cardRect.height - 24f, cardRect.width - 28f, 18f),
                $"BONUS {runStats.BonusCoinsCollected}   LAST {lastRewardText}",
                smallStyle);
        }

        private void DrawGoalRow(Rect rowRect, string label, float progress)
        {
            Rect barRect = new Rect(rowRect.x, rowRect.y + 17f, rowRect.width, 8f);
            DrawRect(barRect, new Color(0.16f, 0.16f, 0.16f, 0.9f));
            DrawRect(new Rect(barRect.x, barRect.y, barRect.width * Mathf.Clamp01(progress), barRect.height), new Color(0.22f, 0.72f, 1f, 0.95f));

            GUI.Label(new Rect(rowRect.x, rowRect.y - 2f, rowRect.width, 18f), label, trackerStyle);
        }

        private string GetNextKillGoalLabel()
        {
            for (int i = 0; i < killMilestones.Length; i++)
            {
                if (!claimedKillMilestones[i])
                {
                    return $"KILL GOAL {runStats.KillCount}/{killMilestones[i]}  +{GetArrayValue(killBonusCoins, i)} Coins";
                }
            }

            return "KILL GOALS COMPLETE";
        }

        private string GetNextTimeGoalLabel()
        {
            for (int i = 0; i < timeMilestones.Length; i++)
            {
                if (!claimedTimeMilestones[i])
                {
                    return $"TIME GOAL {FormatTime(runStats.ElapsedSeconds)}/{FormatTime(timeMilestones[i])}  +{GetArrayValue(timeBonusCoins, i)} Coins";
                }
            }

            return "TIME GOALS COMPLETE";
        }

        private float GetNextKillGoalProgress()
        {
            for (int i = 0; i < killMilestones.Length; i++)
            {
                if (!claimedKillMilestones[i])
                {
                    return killMilestones[i] <= 0 ? 1f : (float)runStats.KillCount / killMilestones[i];
                }
            }

            return 1f;
        }

        private float GetNextTimeGoalProgress()
        {
            for (int i = 0; i < timeMilestones.Length; i++)
            {
                if (!claimedTimeMilestones[i])
                {
                    return timeMilestones[i] <= 0f ? 1f : runStats.ElapsedSeconds / timeMilestones[i];
                }
            }

            return 1f;
        }

        private Rect GetTrackerRect()
        {
            float width = Mathf.Min(trackerSize.x, Screen.width - trackerOffset.x * 2f);
            float height = trackerSize.y;
            return new Rect(Screen.width - width - trackerOffset.x, trackerOffset.y, width, height);
        }

        private void FindRunStatsIfNeeded()
        {
            if (runStats == null)
            {
                runStats = RunStats.Instance;
            }
        }

        private void EnsureStyles()
        {
            if (noticeStyle != null)
            {
                return;
            }

            noticeStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.88f, 0.18f, 1f) }
            };

            trackerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.86f, 0.18f, 1f) }
            };

            smallStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.86f, 0.92f, 1f, 1f) }
            };

            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }

        private void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previousColor;
        }

        private static int GetArrayValue(int[] values, int index)
        {
            return values != null && index >= 0 && index < values.Length ? values[index] : 0;
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
