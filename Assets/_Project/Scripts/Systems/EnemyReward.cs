using PawVoyage.Combat;
using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// 적 사망 시 경험치 보상을 생성합니다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class EnemyReward : MonoBehaviour
    {
        [SerializeField] private int experienceAmount = 1;
        [SerializeField] private ExperienceOrb experienceOrbPrefab = null;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
        }

        /// <summary>
        /// 적 타입에 맞춰 사망 시 지급할 경험치량을 설정합니다.
        /// </summary>
        public void SetExperienceAmount(int amount)
        {
            experienceAmount = Mathf.Max(0, amount);
        }

        private void OnDied(Health deadHealth)
        {
            RunStats.Instance?.AddKill();

            ExperienceOrb orb = experienceOrbPrefab != null
                ? Instantiate(experienceOrbPrefab, transform.position, Quaternion.identity)
                : CreateFallbackOrb();

            orb.Initialize(experienceAmount);
        }

        private ExperienceOrb CreateFallbackOrb()
        {
            GameObject orbObject = new GameObject("ExperienceOrb");
            orbObject.transform.position = transform.position;
            CircleCollider2D orbCollider = orbObject.AddComponent<CircleCollider2D>();
            orbCollider.radius = 0.25f;
            return orbObject.AddComponent<ExperienceOrb>();
        }
    }
}
