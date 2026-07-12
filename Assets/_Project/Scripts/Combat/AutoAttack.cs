using PawVoyage.Enemy;
using UnityEngine;

namespace PawVoyage.Combat
{
    /// <summary>
    /// Automatically targets the nearest enemy and attacks at a fixed interval.
    /// </summary>
    public class AutoAttack : MonoBehaviour
    {
        private const int MaxTargets = 32;

        [SerializeField] private float attackRange = 6f;
        [SerializeField] private float attacksPerSecond = 1f;
        [SerializeField] private int damage = 1;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private string targetTag = "Enemy";
        [SerializeField] private Projectile projectilePrefab = null;
        [SerializeField] private Transform projectileSpawnPoint = null;
        [SerializeField] private float projectileSpeed = 10f;

        private readonly Collider2D[] targetBuffer = new Collider2D[MaxTargets];
        private float nextAttackTime;
        private ContactFilter2D targetFilter;

        /// <summary>
        /// Current target selected by nearest-distance search.
        /// </summary>
        public Transform CurrentTarget { get; private set; }

        private void Awake()
        {
            targetFilter = new ContactFilter2D();
            targetFilter.SetLayerMask(targetLayers);
            targetFilter.useTriggers = true;
        }

        private void OnValidate()
        {
            targetFilter.SetLayerMask(targetLayers);
            targetFilter.useTriggers = true;
        }

        private void Update()
        {
            CurrentTarget = FindNearestTarget();

            if (CurrentTarget == null || Time.time < nextAttackTime)
            {
                return;
            }

            Attack(CurrentTarget);
            nextAttackTime = Time.time + GetAttackInterval();
        }

        private Transform FindNearestTarget()
        {
            int hitCount = Physics2D.OverlapCircle(
                transform.position,
                attackRange,
                targetFilter,
                targetBuffer);

            Transform nearestTarget = null;
            float nearestDistanceSqr = float.PositiveInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D candidate = targetBuffer[i];
                if (candidate == null || candidate.transform == transform || !MatchesTargetTag(candidate))
                {
                    continue;
                }

                float distanceSqr = ((Vector2)candidate.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestTarget = candidate.transform;
                }
            }

            return nearestTarget;
        }

        private void Attack(Transform target)
        {
            Vector2 origin = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
            Vector2 direction = ((Vector2)target.position - origin).normalized;

            if (projectilePrefab != null)
            {
                Projectile projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
                projectile.Initialize(direction, projectileSpeed, damage, targetLayers, targetTag);
                return;
            }

            target.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        private bool MatchesTargetTag(Collider2D candidate)
        {
            return candidate.GetComponent<EnemyController>() != null
                || string.IsNullOrWhiteSpace(targetTag)
                || candidate.gameObject.tag == targetTag;
        }

        private float GetAttackInterval()
        {
            return attacksPerSecond <= 0f ? float.PositiveInfinity : 1f / attacksPerSecond;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
