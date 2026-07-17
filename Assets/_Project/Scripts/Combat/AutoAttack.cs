using PawVoyage.Enemy;
using PawVoyage.Systems;
using UnityEngine;

namespace PawVoyage.Combat
{
    /// <summary>
    /// 가장 가까운 적을 자동으로 조준하고 일정 간격으로 공격합니다.
    /// </summary>
    public class AutoAttack : MonoBehaviour
    {
        private const int MaxTargets = 32;

        [SerializeField] private float attackRange = 6f;
        [SerializeField] private float attacksPerSecond = 1f;
        [SerializeField] private int damage = 1;
        [SerializeField] private int damageBonus = 0;
        [SerializeField] private float attackRateBonusMult = 1f;
        [SerializeField] private CombatStats combatStats = new CombatStats();
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private string targetTag = "Enemy";
        [SerializeField] private Projectile projectilePrefab = null;
        [SerializeField] private Transform projectileSpawnPoint = null;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private WeaponData weaponData = null;

        private readonly Collider2D[] targetBuffer = new Collider2D[MaxTargets];
        private float nextAttackTime;
        private ContactFilter2D targetFilter;

        /// <summary>
        /// 가장 가까운 거리 탐색으로 선택된 현재 대상입니다.
        /// </summary>
        public Transform CurrentTarget { get; private set; }

        /// <summary>
        /// 현재 공격 수치를 결정하는 무기 데이터입니다. 없으면 직렬화된 기본값을 사용합니다.
        /// </summary>
        public WeaponData WeaponData => weaponData;

        /// <summary>
        /// 레벨업 보상으로 추가 피해를 누적합니다.
        /// </summary>
        public void AddDamageBonus(int amount)
        {
            damageBonus += Mathf.Max(0, amount);
        }

        /// <summary>
        /// 레벨업 보상으로 공격 속도 배율을 누적합니다.
        /// </summary>
        public void AddAttackRateMultiplier(float multiplierBonus)
        {
            attackRateBonusMult = Mathf.Max(0.01f, attackRateBonusMult + Mathf.Max(0f, multiplierBonus));
        }

        /// <summary>
        /// 레벨업 보상으로 추가 투사체 수를 누적합니다.
        /// </summary>
        public void AddProjectileBonus(int amount)
        {
            combatStats.AddProjectileBonus(amount);
        }

        /// <summary>
        /// 레벨업 보상으로 투사체 관통 수를 누적합니다.
        /// </summary>
        public void AddPierceBonus(int amount)
        {
            combatStats.AddPierceBonus(amount);
        }

        /// <summary>
        /// 레벨업 보상으로 공격 사거리 배율을 누적합니다.
        /// </summary>
        public void AddRangeMultiplier(float multiplierBonus)
        {
            combatStats.AddRangeMultiplier(multiplierBonus);
        }

        public void SetWeapon(WeaponData newWeaponData)
        {
            weaponData = newWeaponData;
        }

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
                GetAttackRange(),
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
            GameSfx.PlayAttack();

            Vector2 origin = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
            Vector2 direction = ((Vector2)target.position - origin).normalized;

            int projectileCount = GetProjectileCount();
            if (GetAttackType() == WeaponAttackType.Projectile)
            {
                FireProjectiles(origin, direction, projectileCount);
                return;
            }

            DamageRequest request = new DamageRequest(CalculateDamage(), gameObject);
            if (target.TryGetComponent(out IDamageable damageable))
            {
                damageable.ApplyDamage(request);
                GameSfx.PlayEnemyHit();
                return;
            }

            target.SendMessage("TakeDamage", request.Amount, SendMessageOptions.DontRequireReceiver);
            GameSfx.PlayEnemyHit();
        }

        private void FireProjectiles(Vector2 origin, Vector2 direction, int projectileCount)
        {
            int count = Mathf.Max(1, projectileCount);
            float spreadStep = count <= 1 ? 0f : 12f;
            float startAngle = -spreadStep * (count - 1) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + spreadStep * i;
                Vector2 fireDirection = Quaternion.Euler(0f, 0f, angle) * direction;
                Projectile projectile = projectilePrefab != null
                    ? Instantiate(projectilePrefab, origin, Quaternion.identity)
                    : CreateFallbackProjectile(origin);
                projectile.Initialize(fireDirection, GetProjectileSpeed(), CalculateDamage(), targetLayers, targetTag, GetPierceCount());
            }
        }

        private static Projectile CreateFallbackProjectile(Vector2 origin)
        {
            GameObject projectileObject = new GameObject("Projectile");
            projectileObject.transform.position = origin;
            CircleCollider2D projectileCollider = projectileObject.AddComponent<CircleCollider2D>();
            projectileCollider.radius = 0.14f;
            return projectileObject.AddComponent<Projectile>();
        }

        private bool MatchesTargetTag(Collider2D candidate)
        {
            return candidate.GetComponent<EnemyController>() != null
                || string.IsNullOrWhiteSpace(targetTag)
                || candidate.gameObject.tag == targetTag;
        }

        private float GetAttackInterval()
        {
            if (weaponData != null)
            {
                return weaponData.BaseCooldown / (combatStats.AttackRateMult * attackRateBonusMult);
            }

            float attackRate = attacksPerSecond * combatStats.AttackRateMult * attackRateBonusMult;
            return attackRate <= 0f ? float.PositiveInfinity : 1f / attackRate;
        }

        private int CalculateDamage()
        {
            int baseDamage = weaponData != null ? weaponData.BaseDamage : damage;
            return combatStats.CalculateDamage(baseDamage + damageBonus);
        }

        private float GetAttackRange()
        {
            float baseRange = weaponData != null ? weaponData.BaseRange : attackRange;
            return baseRange * combatStats.RangeMult;
        }

        private float GetProjectileSpeed()
        {
            return weaponData != null ? weaponData.ProjectileSpeed : projectileSpeed;
        }

        private int GetProjectileCount()
        {
            int baseCount = weaponData != null ? weaponData.BaseProjectileCount : 1;
            return baseCount + combatStats.ProjectileBonus;
        }

        private int GetPierceCount()
        {
            int basePierce = weaponData != null ? weaponData.BasePierce : 0;
            return basePierce + combatStats.PierceBonus;
        }

        private WeaponAttackType GetAttackType()
        {
            return weaponData != null ? weaponData.AttackType : WeaponAttackType.Projectile;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, GetAttackRange());
        }
    }
}
