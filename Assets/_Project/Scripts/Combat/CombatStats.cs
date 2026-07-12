using UnityEngine;

namespace PawVoyage.Combat
{
    /// <summary>
    /// 밸런스 바이블 기준의 공통 전투 스탯 보정값입니다.
    /// </summary>
    [System.Serializable]
    public class CombatStats
    {
        [SerializeField] private float damageMult = 1f;
        [SerializeField] private float attackRateMult = 1f;
        [SerializeField] private int projectileBonus = 0;
        [SerializeField] private float rangeMult = 1f;
        [SerializeField] private int pierceBonus = 0;
        [SerializeField, Range(0f, 1f)] private float critChance = 0.05f;
        [SerializeField] private float critDamageMult = 1.5f;
        [SerializeField] private int maxHpBonus = 0;
        [SerializeField] private float maxHpMult = 1f;
        [SerializeField] private float moveSpeedMult = 1f;
        [SerializeField] private float pickupRadiusMult = 1f;
        [SerializeField, Range(0f, 0.6f)] private float damageReduction = 0f;

        public float DamageMult => Mathf.Max(0f, damageMult);
        public float AttackRateMult => Mathf.Max(0.01f, attackRateMult);
        public int ProjectileBonus => Mathf.Max(0, projectileBonus);
        public float RangeMult => Mathf.Max(0.01f, rangeMult);
        public int PierceBonus => Mathf.Max(0, pierceBonus);
        public float CritChance => Mathf.Clamp01(critChance);
        public float CritDamageMult => Mathf.Max(1f, critDamageMult);
        public int MaxHpBonus => Mathf.Max(0, maxHpBonus);
        public float MaxHpMult => Mathf.Max(0.01f, maxHpMult);
        public float MoveSpeedMult => Mathf.Max(0.01f, moveSpeedMult);
        public float PickupRadiusMult => Mathf.Max(0.01f, pickupRadiusMult);
        public float DamageReduction => Mathf.Clamp(damageReduction, 0f, 0.6f);

        /// <summary>
        /// 밸런스 바이블의 기본 피해 공식을 사용해 최종 타격 피해를 계산합니다.
        /// </summary>
        public int CalculateDamage(int baseDamage)
        {
            bool isCritical = Random.value < CritChance;
            return CalculateDamage(baseDamage, isCritical);
        }

        /// <summary>
        /// 명시된 치명타 결과를 사용해 최종 타격 피해를 계산합니다.
        /// </summary>
        public int CalculateDamage(int baseDamage, bool isCritical)
        {
            float criticalMultiplier = isCritical ? CritDamageMult : 1f;
            return Mathf.Max(0, Mathf.FloorToInt(baseDamage * DamageMult * criticalMultiplier));
        }

        /// <summary>
        /// 방어자 측 피해 감소를 적용합니다.
        /// </summary>
        public int ReduceIncomingDamage(int incomingDamage)
        {
            return Mathf.Max(0, Mathf.FloorToInt(incomingDamage * (1f - DamageReduction)));
        }
    }
}
