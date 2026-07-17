using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// 런 종료 결과와 최고 기록을 저장하고 읽어오는 정적 저장소입니다.
    /// </summary>
    public static class RunResultData
    {
        private const string HasLastResultKey = "RunResult.HasLastResult";
        private const string LastClearedKey = "RunResult.LastCleared";
        private const string LastElapsedSecondsKey = "RunResult.LastElapsedSeconds";
        private const string LastKillCountKey = "RunResult.LastKillCount";
        private const string LastCoinCountKey = "RunResult.LastCoinCount";
        private const string LastLevelUpCountKey = "RunResult.LastLevelUpCount";
        private const string LastHitCountKey = "RunResult.LastHitCount";
        private const string LastDamageTakenKey = "RunResult.LastDamageTaken";
        private const string LastSelectedWeaponsKey = "RunResult.LastSelectedWeapons";
        private const string LastMiniBossSeenKey = "RunResult.LastMiniBossSeen";
        private const string BestElapsedSecondsKey = "RunResult.BestElapsedSeconds";
        private const string BestKillCountKey = "RunResult.BestKillCount";
        private const string TotalCoinsKey = "RunResult.TotalCoins";
        private const string Stage1MvpClearedKey = "RunResult.Stage1MvpCleared";

        public static bool HasLastResult => PlayerPrefs.GetInt(HasLastResultKey, 0) == 1;
        public static bool LastCleared => PlayerPrefs.GetInt(LastClearedKey, 0) == 1;
        public static float LastElapsedSeconds => PlayerPrefs.GetFloat(LastElapsedSecondsKey, 0f);
        public static int LastKillCount => PlayerPrefs.GetInt(LastKillCountKey, 0);
        public static int LastCoinCount => PlayerPrefs.GetInt(LastCoinCountKey, 0);
        public static int LastLevelUpCount => PlayerPrefs.GetInt(LastLevelUpCountKey, 0);
        public static int LastHitCount => PlayerPrefs.GetInt(LastHitCountKey, 0);
        public static int LastDamageTaken => PlayerPrefs.GetInt(LastDamageTakenKey, 0);
        public static string LastSelectedWeapons => PlayerPrefs.GetString(LastSelectedWeaponsKey, "None");
        public static bool LastMiniBossSeen => PlayerPrefs.GetInt(LastMiniBossSeenKey, 0) == 1;
        public static float BestElapsedSeconds => PlayerPrefs.GetFloat(BestElapsedSecondsKey, 0f);
        public static int BestKillCount => PlayerPrefs.GetInt(BestKillCountKey, 0);
        public static int TotalCoins => PlayerPrefs.GetInt(TotalCoinsKey, 0);
        public static bool Stage1MvpCleared => PlayerPrefs.GetInt(Stage1MvpClearedKey, 0) == 1;

        /// <summary>
        /// 한 번의 런 결과를 저장하고 최고 기록을 갱신합니다.
        /// </summary>
        public static void RecordResult(
            bool cleared,
            float elapsedSeconds,
            int killCount,
            int coinCount,
            int levelUpCount = 0,
            int hitCount = 0,
            int damageTaken = 0,
            string selectedWeapons = "None",
            bool miniBossSeen = false,
            bool recordStage1MvpClear = false)
        {
            float safeElapsedSeconds = Mathf.Max(0f, elapsedSeconds);
            int safeKillCount = Mathf.Max(0, killCount);
            int safeCoinCount = Mathf.Max(0, coinCount);
            int safeLevelUpCount = Mathf.Max(0, levelUpCount);
            int safeHitCount = Mathf.Max(0, hitCount);
            int safeDamageTaken = Mathf.Max(0, damageTaken);
            string safeSelectedWeapons = string.IsNullOrWhiteSpace(selectedWeapons) ? "None" : selectedWeapons;

            PlayerPrefs.SetInt(HasLastResultKey, 1);
            PlayerPrefs.SetInt(LastClearedKey, cleared ? 1 : 0);
            PlayerPrefs.SetFloat(LastElapsedSecondsKey, safeElapsedSeconds);
            PlayerPrefs.SetInt(LastKillCountKey, safeKillCount);
            PlayerPrefs.SetInt(LastCoinCountKey, safeCoinCount);
            PlayerPrefs.SetInt(LastLevelUpCountKey, safeLevelUpCount);
            PlayerPrefs.SetInt(LastHitCountKey, safeHitCount);
            PlayerPrefs.SetInt(LastDamageTakenKey, safeDamageTaken);
            PlayerPrefs.SetString(LastSelectedWeaponsKey, safeSelectedWeapons);
            PlayerPrefs.SetInt(LastMiniBossSeenKey, miniBossSeen ? 1 : 0);
            AddCoins(safeCoinCount);

            if (cleared && recordStage1MvpClear)
            {
                PlayerPrefs.SetInt(Stage1MvpClearedKey, 1);
            }

            if (safeElapsedSeconds > BestElapsedSeconds)
            {
                PlayerPrefs.SetFloat(BestElapsedSecondsKey, safeElapsedSeconds);
            }

            if (safeKillCount > BestKillCount)
            {
                PlayerPrefs.SetInt(BestKillCountKey, safeKillCount);
            }

            PlayerPrefs.Save();
        }

        /// <summary>
        /// 저장된 런 결과와 최고 기록을 초기화합니다.
        /// </summary>
        public static void ResetRecords()
        {
            PlayerPrefs.DeleteKey(HasLastResultKey);
            PlayerPrefs.DeleteKey(LastClearedKey);
            PlayerPrefs.DeleteKey(LastElapsedSecondsKey);
            PlayerPrefs.DeleteKey(LastKillCountKey);
            PlayerPrefs.DeleteKey(LastCoinCountKey);
            PlayerPrefs.DeleteKey(LastLevelUpCountKey);
            PlayerPrefs.DeleteKey(LastHitCountKey);
            PlayerPrefs.DeleteKey(LastDamageTakenKey);
            PlayerPrefs.DeleteKey(LastSelectedWeaponsKey);
            PlayerPrefs.DeleteKey(LastMiniBossSeenKey);
            PlayerPrefs.DeleteKey(BestElapsedSecondsKey);
            PlayerPrefs.DeleteKey(BestKillCountKey);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 영구 성장 구매에 사용할 보유 코인을 증가시킵니다.
        /// </summary>
        public static void AddCoins(int amount)
        {
            PlayerPrefs.SetInt(TotalCoinsKey, TotalCoins + Mathf.Max(0, amount));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 보유 코인이 충분할 때 지정 수량을 차감합니다.
        /// </summary>
        public static bool TrySpendCoins(int amount)
        {
            int safeAmount = Mathf.Max(0, amount);
            if (TotalCoins < safeAmount)
            {
                return false;
            }

            PlayerPrefs.SetInt(TotalCoinsKey, TotalCoins - safeAmount);
            PlayerPrefs.Save();
            return true;
        }
    }
}
