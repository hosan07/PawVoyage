using UnityEngine;

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

        public int CurrentLevel => currentLevel;
        public int CurrentExp => currentExp;
        public int ExpToNextLevel => expToNextLevel;

        public void AddExperience(int amount)
        {
            currentExp += Mathf.Max(0, amount);

            while (currentExp >= expToNextLevel)
            {
                currentExp -= expToNextLevel;
                currentLevel++;
                expToNextLevel = Mathf.Max(expToNextLevel + 1, Mathf.CeilToInt(expToNextLevel * levelRequirementGrowth));
            }
        }
    }
}
