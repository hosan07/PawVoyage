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
        [SerializeField, Range(0f, 1f)] private float healthPickupDropChance = 0.08f;
        [SerializeField] private int healthPickupHealAmount = 15;
        [SerializeField] private HealthPickup healthPickupPrefab = null;

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

            TryDropHealthPickup();
        }

        private ExperienceOrb CreateFallbackOrb()
        {
            GameObject orbObject = new GameObject("ExperienceOrb");
            orbObject.transform.position = transform.position;
            CircleCollider2D orbCollider = orbObject.AddComponent<CircleCollider2D>();
            orbCollider.radius = 0.25f;
            return orbObject.AddComponent<ExperienceOrb>();
        }

        /// <summary>
        /// 적 타입에 맞춰 체력 회복 아이템 드롭 확률을 설정합니다.
        /// </summary>
        public void SetHealthPickupDropChance(float dropChance)
        {
            healthPickupDropChance = Mathf.Clamp01(dropChance);
        }

        /// <summary>
        /// 적 타입에 맞춰 체력 회복 아이템의 회복량을 설정합니다.
        /// </summary>
        public void SetHealthPickupHealAmount(int amount)
        {
            healthPickupHealAmount = Mathf.Max(1, amount);
        }

        private void TryDropHealthPickup()
        {
            if (Random.value > healthPickupDropChance)
            {
                return;
            }

            HealthPickup pickup = healthPickupPrefab != null
                ? Instantiate(healthPickupPrefab, transform.position + Vector3.right * 0.28f, Quaternion.identity)
                : CreateFallbackHealthPickup();

            pickup.Initialize(healthPickupHealAmount);
        }

        private HealthPickup CreateFallbackHealthPickup()
        {
            GameObject pickupObject = new GameObject("HealthPickup");
            pickupObject.transform.position = transform.position + Vector3.right * 0.28f;
            CircleCollider2D pickupCollider = pickupObject.AddComponent<CircleCollider2D>();
            pickupCollider.radius = 0.25f;
            return pickupObject.AddComponent<HealthPickup>();
        }
    }
}
