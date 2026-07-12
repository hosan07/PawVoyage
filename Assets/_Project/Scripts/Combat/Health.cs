using UnityEngine;
using UnityEngine.Events;

namespace PawVoyage.Combat
{
    /// <summary>
    /// Reusable health component with damage reduction and death events.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHp = 3;
        [SerializeField] private CombatStats combatStats = new CombatStats();
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private int currentHp;
        [SerializeField] private UnityEvent onDamaged = null;
        [SerializeField] private UnityEvent onDeath = null;

        private bool isDead;

        /// <summary>
        /// Current hit points.
        /// </summary>
        public int CurrentHp => currentHp;

        /// <summary>
        /// Maximum hit points after stat modifiers.
        /// </summary>
        public int MaxHp => Mathf.Max(1, Mathf.FloorToInt((maxHp + combatStats.MaxHpBonus) * combatStats.MaxHpMult));

        public bool CanReceiveDamage => !isDead && currentHp > 0;

        private void Awake()
        {
            ResetHealth();
        }

        /// <summary>
        /// Applies damage using the common damage request structure.
        /// </summary>
        public void ApplyDamage(DamageRequest request)
        {
            if (!CanReceiveDamage)
            {
                return;
            }

            int finalDamage = combatStats.ReduceIncomingDamage(request.Amount);
            currentHp = Mathf.Max(0, currentHp - finalDamage);
            onDamaged?.Invoke();

            if (currentHp <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Compatibility method for SendMessage and simple callers.
        /// </summary>
        public void TakeDamage(int amount)
        {
            ApplyDamage(new DamageRequest(amount));
        }

        /// <summary>
        /// Restores health up to the maximum.
        /// </summary>
        public void Heal(int amount)
        {
            if (isDead)
            {
                return;
            }

            currentHp = Mathf.Min(MaxHp, currentHp + Mathf.Max(0, amount));
        }

        /// <summary>
        /// Restores this object to full health and clears death state.
        /// </summary>
        public void ResetHealth()
        {
            isDead = false;
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

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
