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
        public int LevelUpCount { get; private set; }
        public int HitCount { get; private set; }
        public int DamageTaken { get; private set; }
        public float ClearTimeSeconds => Mathf.Max(1f, clearTimeSeconds);
        public StageClearCondition ClearCondition => clearCondition;
        public bool IsCleared { get; private set; }
        public bool MiniBossSeen { get; private set; }
        public string StageId { get; private set; } = string.Empty;
        public StageRuntimeMode StageMode { get; private set; } = StageRuntimeMode.Test;
        public bool IsStage1MvpRun => StageId == "stage_001" && StageMode == StageRuntimeMode.Mvp;

        private readonly System.Collections.Generic.List<string> selectedWeapons = new System.Collections.Generic.List<string>();

        public string SelectedWeaponsSummary => selectedWeapons.Count == 0 ? "None" : string.Join(", ", selectedWeapons);

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
        /// 스테이지 데이터 기준으로 런 식별 정보와 클리어 조건을 갱신합니다.
        /// </summary>
        public void ConfigureStage(string targetStageId, StageRuntimeMode runtimeMode, float targetClearTimeSeconds, StageClearCondition stageClearCondition)
        {
            StageId = targetStageId ?? string.Empty;
            StageMode = runtimeMode;
            ConfigureStage(targetClearTimeSeconds, stageClearCondition);
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
        /// 이번 런에서 발생한 레벨업 횟수를 기록합니다.
        /// </summary>
        public void AddLevelUp()
        {
            LevelUpCount++;
        }

        /// <summary>
        /// 플레이어가 받은 피해 횟수와 총 피해량을 기록합니다.
        /// </summary>
        public void AddDamageTaken(int amount)
        {
            int safeAmount = Mathf.Max(0, amount);
            if (safeAmount <= 0)
            {
                return;
            }

            HitCount++;
            DamageTaken += safeAmount;
        }

        /// <summary>
        /// 이번 런에서 획득 또는 강화 선택한 보조무기 이름을 기록합니다.
        /// </summary>
        public void RegisterSelectedWeapon(string weaponName)
        {
            if (string.IsNullOrWhiteSpace(weaponName) || selectedWeapons.Contains(weaponName))
            {
                return;
            }

            selectedWeapons.Add(weaponName);
        }

        /// <summary>
        /// 이번 런에서 미니보스 등장 흐름을 봤다는 것을 기록합니다.
        /// </summary>
        public void RegisterMiniBossSeen()
        {
            MiniBossSeen = true;
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
