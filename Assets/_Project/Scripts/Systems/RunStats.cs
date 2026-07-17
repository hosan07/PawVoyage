using UnityEngine;
using System;
using PawVoyage.Data;

namespace PawVoyage.Systems
{
    /// <summary>
    /// 한 번의 플레이 세션에서 누적되는 진행 정보를 관리합니다.
    /// </summary>
    public class RunStats : MonoBehaviour
    {
        [SerializeField] private float clearTimeSeconds = 30f;
        [SerializeField] private StageClearCondition clearCondition = StageClearCondition.SurviveTime;

        public static RunStats Instance { get; private set; }

        public event Action RunCleared;

        public float ElapsedSeconds { get; private set; }
        public int KillCount { get; private set; }
        public int CoinsCollected { get; private set; }
        public int BonusCoinsCollected { get; private set; }
        public float ClearTimeSeconds => Mathf.Max(1f, clearTimeSeconds);
        public StageClearCondition ClearCondition => clearCondition;
        public bool IsCleared { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (IsCleared)
            {
                return;
            }

            ElapsedSeconds += Time.deltaTime;

            if (clearCondition == StageClearCondition.SurviveTime && ElapsedSeconds >= ClearTimeSeconds)
            {
                CompleteRun();
            }
        }

        /// <summary>
        /// 스테이지 데이터 기준으로 클리어 시간과 조건을 갱신합니다.
        /// </summary>
        public void ConfigureStage(float targetClearTimeSeconds, StageClearCondition stageClearCondition)
        {
            clearTimeSeconds = Mathf.Max(1f, targetClearTimeSeconds);
            clearCondition = stageClearCondition;
        }

        /// <summary>
        /// 현재 런을 클리어 상태로 전환합니다.
        /// </summary>
        public void CompleteRun()
        {
            if (IsCleared)
            {
                return;
            }

            IsCleared = true;
            RunCleared?.Invoke();
        }

        /// <summary>
        /// 적 처치 수를 1 증가시킵니다.
        /// </summary>
        public void AddKill()
        {
            KillCount++;
        }

        /// <summary>
        /// 현재 런에서 획득한 코인 수를 증가시킵니다.
        /// </summary>
        public void AddCoins(int amount)
        {
            CoinsCollected += Mathf.Max(0, amount);
        }

        /// <summary>
        /// 목표 달성 등으로 지급되는 보너스 코인을 증가시킵니다.
        /// </summary>
        public void AddBonusCoins(int amount)
        {
            int safeAmount = Mathf.Max(0, amount);
            BonusCoinsCollected += safeAmount;
            AddCoins(safeAmount);
        }
    }
}
