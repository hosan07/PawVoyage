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
        private const string BestElapsedSecondsKey = "RunResult.BestElapsedSeconds";
        private const string BestKillCountKey = "RunResult.BestKillCount";
        private const string TotalCoinsKey = "RunResult.TotalCoins";

        public static bool HasLastResult => PlayerPrefs.GetInt(HasLastResultKey, 0) == 1;
        public static bool LastCleared => PlayerPrefs.GetInt(LastClearedKey, 0) == 1;
        public static float LastElapsedSeconds => PlayerPrefs.GetFloat(LastElapsedSecondsKey, 0f);
        public static int LastKillCount => PlayerPrefs.GetInt(LastKillCountKey, 0);
        public static int LastCoinCount => PlayerPrefs.GetInt(LastCoinCountKey, 0);
        public static float BestElapsedSeconds => PlayerPrefs.GetFloat(BestElapsedSecondsKey, 0f);
        public static int BestKillCount => PlayerPrefs.GetInt(BestKillCountKey, 0);
        public static int TotalCoins => PlayerPrefs.GetInt(TotalCoinsKey, 0);

        /// <summary>
        /// 한 번의 런 결과를 저장하고 최고 기록을 갱신합니다.
        /// </summary>
        public static void RecordResult(bool cleared, float elapsedSeconds, int killCount, int coinCount)
        {
            float safeElapsedSeconds = Mathf.Max(0f, elapsedSeconds);
            int safeKillCount = Mathf.Max(0, killCount);
            int safeCoinCount = Mathf.Max(0, coinCount);

            PlayerPrefs.SetInt(HasLastResultKey, 1);
            PlayerPrefs.SetInt(LastClearedKey, cleared ? 1 : 0);
            PlayerPrefs.SetFloat(LastElapsedSecondsKey, safeElapsedSeconds);
            PlayerPrefs.SetInt(LastKillCountKey, safeKillCount);
            PlayerPrefs.SetInt(LastCoinCountKey, safeCoinCount);
            PlayerPrefs.SetInt(TotalCoinsKey, TotalCoins + safeCoinCount);

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
            PlayerPrefs.DeleteKey(BestElapsedSecondsKey);
            PlayerPrefs.DeleteKey(BestKillCountKey);
            PlayerPrefs.DeleteKey(TotalCoinsKey);
            PlayerPrefs.Save();
        }
    }
}
