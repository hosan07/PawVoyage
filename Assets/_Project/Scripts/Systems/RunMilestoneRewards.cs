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

        private bool[] claimedKillMilestones;
        private bool[] claimedTimeMilestones;
        private RunStats runStats;
        private GUIStyle noticeStyle;
        private GUIStyle trackerStyle;
        private string noticeText = string.Empty;
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
            GUI.Label(new Rect(Screen.width * 0.5f - 170f, 84f, 340f, 42f), GetNextGoalText(), trackerStyle);

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

            runStats.AddCoins(safeCoinAmount);
            GameSfx.PlayCoin();
            noticeText = $"{reason}  +{safeCoinAmount} Coins";
            noticeEndTime = Time.unscaledTime + Mathf.Max(0.1f, noticeDuration);
        }

        private string GetNextGoalText()
        {
            string killGoal = GetNextKillGoalText();
            string timeGoal = GetNextTimeGoalText();

            if (string.IsNullOrEmpty(killGoal) && string.IsNullOrEmpty(timeGoal))
            {
                return "GOALS COMPLETE";
            }

            if (string.IsNullOrEmpty(killGoal))
            {
                return timeGoal;
            }

            if (string.IsNullOrEmpty(timeGoal))
            {
                return killGoal;
            }

            return $"{killGoal}   |   {timeGoal}";
        }

        private string GetNextKillGoalText()
        {
            for (int i = 0; i < killMilestones.Length; i++)
            {
                if (!claimedKillMilestones[i])
                {
                    return $"KILL GOAL {runStats.KillCount}/{killMilestones[i]}";
                }
            }

            return string.Empty;
        }

        private string GetNextTimeGoalText()
        {
            for (int i = 0; i < timeMilestones.Length; i++)
            {
                if (!claimedTimeMilestones[i])
                {
                    return $"TIME GOAL {FormatTime(runStats.ElapsedSeconds)}/{FormatTime(timeMilestones[i])}";
                }
            }

            return string.Empty;
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
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
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
