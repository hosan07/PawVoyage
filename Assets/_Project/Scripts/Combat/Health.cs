using UnityEngine;
using UnityEngine.Events;
using System;

namespace PawVoyage.Combat
{
    /// <summary>
    /// 피해 감소와 사망 이벤트를 포함한 재사용 가능한 체력 컴포넌트입니다.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHp = 30;
        [SerializeField] private CombatStats combatStats = new CombatStats();
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private float invulnerabilityDuration = 0f;
        [SerializeField] private int currentHp;
        [SerializeField] private UnityEvent onDamaged = null;
        [SerializeField] private UnityEvent onDeath = null;

        private bool isDead;
        private float nextDamageTime;

        public event Action<Health, int, bool> Damaged;
        public event Action<Health, int> Healed;
        public event Action<Health> Died;

        /// <summary>
        /// 현재 체력입니다.
        /// </summary>
        public int CurrentHp => currentHp;

        /// <summary>
        /// 스탯 보정이 적용된 최대 체력입니다.
        /// </summary>
        public int MaxHp => Mathf.Max(1, Mathf.FloorToInt((maxHp + combatStats.MaxHpBonus) * combatStats.MaxHpMult));

        public bool CanReceiveDamage => !isDead && currentHp > 0 && Time.time >= nextDamageTime;

        private void Awake()
        {
            ResetHealth();
        }

        /// <summary>
        /// 공통 피해 요청 구조를 사용해 피해를 적용합니다.
        /// </summary>
        public void ApplyDamage(DamageRequest request)
        {
            if (!CanReceiveDamage)
            {
                return;
            }

            int finalDamage = combatStats.ReduceIncomingDamage(request.Amount);
            currentHp = Mathf.Max(0, currentHp - finalDamage);
            nextDamageTime = Time.time + Mathf.Max(0f, invulnerabilityDuration);
            Damaged?.Invoke(this, finalDamage, request.IsCritical);
            onDamaged?.Invoke();

            if (currentHp <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// SendMessage와 단순 호출자를 위한 호환 메서드입니다.
        /// </summary>
        public void TakeDamage(int amount)
        {
            ApplyDamage(new DamageRequest(amount));
        }

        /// <summary>
        /// 체력을 최대치까지 회복합니다.
        /// </summary>
        public void Heal(int amount)
        {
            if (isDead)
            {
                return;
            }

            int previousHp = currentHp;
            currentHp = Mathf.Min(MaxHp, currentHp + Mathf.Max(0, amount));
            int healedAmount = currentHp - previousHp;

            if (healedAmount > 0)
            {
                Healed?.Invoke(this, healedAmount);
            }
        }

        /// <summary>
        /// 레벨업 보상으로 기본 최대 체력을 증가시키고 선택적으로 즉시 회복합니다.
        /// </summary>
        public void AddMaxHpBonus(int amount, bool healByAddedAmount)
        {
            int bonus = Mathf.Max(0, amount);
            maxHp += bonus;

            if (healByAddedAmount)
            {
                Heal(bonus);
            }
            else
            {
                currentHp = Mathf.Min(currentHp, MaxHp);
            }
        }

        /// <summary>
        /// 런타임에 생성되는 대상의 기본 최대 체력을 설정합니다.
        /// </summary>
        public void SetBaseMaxHp(int value, bool resetCurrentHp)
        {
            maxHp = Mathf.Max(1, value);

            if (resetCurrentHp)
            {
                ResetHealth();
            }
            else
            {
                currentHp = Mathf.Min(currentHp, MaxHp);
            }
        }

        /// <summary>
        /// 이 오브젝트의 체력을 가득 채우고 사망 상태를 해제합니다.
        /// </summary>
        public void ResetHealth()
        {
            isDead = false;
            nextDamageTime = 0f;
            currentHp = MaxHp;
        }

        private void Die()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            onDeath?.Invoke();
            Died?.Invoke(this);

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
