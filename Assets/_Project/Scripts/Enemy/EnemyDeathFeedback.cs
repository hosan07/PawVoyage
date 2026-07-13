using PawVoyage.Combat;
using UnityEngine;

namespace PawVoyage.Enemy
{
    /// <summary>
    /// 적이 사망할 때 짧게 퍼지는 파편 효과를 생성합니다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class EnemyDeathFeedback : MonoBehaviour
    {
        [SerializeField] private int particleCount = 7;
        [SerializeField] private float particleSpeed = 2.4f;
        [SerializeField] private float particleLifetime = 0.38f;
        [SerializeField] private float particleScale = 0.18f;
        [SerializeField] private Color particleColor = new Color(1f, 0.35f, 0.15f, 1f);

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health.Died += OnDied;
        }

        private void OnDisable()
        {
            health.Died -= OnDied;
        }

        private void OnDied(Health deadHealth)
        {
            int count = Mathf.Max(1, particleCount);
            for (int i = 0; i < count; i++)
            {
                float angle = 360f * i / count + Random.Range(-16f, 16f);
                Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector2.right;
                float speed = particleSpeed * Random.Range(0.75f, 1.2f);
                DeathBurstParticle.Spawn(transform.position, direction * speed, particleLifetime, particleScale, particleColor);
            }
        }
    }
}
