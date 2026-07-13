using UnityEngine;
using System;

namespace PawVoyage.Systems
{
    /// <summary>
    /// 플레이어의 세션 경험치와 레벨을 관리합니다.
    /// </summary>
    public class PlayerExperience : MonoBehaviour
    {
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int currentExp = 0;
        [SerializeField] private int expToNextLevel = 10;
        [SerializeField] private float levelRequirementGrowth = 1.25f;
        [SerializeField] private float pickupRadiusMultiplier = 1f;

        public event Action<int> LevelGained;

        public int CurrentLevel => currentLevel;
        public int CurrentExp => currentExp;
        public int ExpToNextLevel => expToNextLevel;
        public float PickupRadiusMultiplier => Mathf.Max(0.1f, pickupRadiusMultiplier);

        /// <summary>
        /// 경험치 구슬 자석 반경 배율을 증가시킵니다.
        /// </summary>
        public void AddPickupRadiusMultiplier(float multiplierBonus)
        {
            pickupRadiusMultiplier = Mathf.Max(0.1f, pickupRadiusMultiplier + Mathf.Max(0f, multiplierBonus));
        }

        public void AddExperience(int amount)
        {
            currentExp += Mathf.Max(0, amount);

            while (currentExp >= expToNextLevel)
            {
                currentExp -= expToNextLevel;
                currentLevel++;
                expToNextLevel = Mathf.Max(expToNextLevel + 1, Mathf.CeilToInt(expToNextLevel * levelRequirementGrowth));
                LevelGained?.Invoke(currentLevel);
            }
        }
    }
}
