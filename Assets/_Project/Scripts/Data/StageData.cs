using System;
using UnityEngine;

namespace PawVoyage.Data
{
    public enum StageRuntimeMode
    {
        Test,
        Mvp
    }

    public enum StageClearCondition
    {
        SurviveTime,
        MiniBossDefeat
    }

    public enum StageDurationPreset
    {
        Test30Seconds,
        Mvp180Seconds
    }

    /// <summary>
    /// 스테이지 모드별 시간, 페이즈, 스폰 압박, 클리어 조건을 담는 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "StageData", menuName = "Paw Voyage/Data/Stage Data")]
    public class StageData : ScriptableObject
    {
        [SerializeField] private string stageId = "stage_001";
        [SerializeField] private string displayName = "Stage 1";
        [SerializeField] private StageModeConfig testConfig = StageModeConfig.CreateTestDefault();
        [SerializeField] private StageModeConfig mvpConfig = StageModeConfig.CreateMvpDefault();

        public string StageId => stageId;
        public string DisplayName => displayName;

        public StageModeConfig GetConfig(StageRuntimeMode mode)
        {
            return mode == StageRuntimeMode.Mvp ? mvpConfig : testConfig;
        }
    }

    [Serializable]
    public struct StageModeConfig
    {
        public StageDurationPreset durationPreset;
        public float clearTimeSeconds;
        public float fastEnemyStartTime;
        public float tankEnemyStartTime;
        public float eliteSpawnTime;
        public float spawnIntervalStart;
        public float spawnIntervalEnd;
        public int enemyCapStart;
        public int enemyCapEnd;
        public StageClearCondition clearCondition;

        public static StageModeConfig CreateTestDefault()
        {
            return new StageModeConfig
            {
                durationPreset = StageDurationPreset.Test30Seconds,
                clearTimeSeconds = 30f,
                fastEnemyStartTime = 8f,
                tankEnemyStartTime = 16f,
                eliteSpawnTime = 18f,
                spawnIntervalStart = 1.5f,
                spawnIntervalEnd = 0.65f,
                enemyCapStart = 20,
                enemyCapEnd = 36,
                clearCondition = StageClearCondition.MiniBossDefeat
            };
        }

        public static StageModeConfig CreateMvpDefault()
        {
            return new StageModeConfig
            {
                durationPreset = StageDurationPreset.Mvp180Seconds,
                clearTimeSeconds = 180f,
                fastEnemyStartTime = 32f,
                tankEnemyStartTime = 75f,
                eliteSpawnTime = 150f,
                spawnIntervalStart = 1.25f,
                spawnIntervalEnd = 0.5f,
                enemyCapStart = 18,
                enemyCapEnd = 56,
                clearCondition = StageClearCondition.MiniBossDefeat
            };
        }
    }
}
