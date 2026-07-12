using UnityEngine;

namespace PawVoyage.Combat
{
    public enum WeaponSlotType
    {
        Primary,
        Secondary
    }

    public enum WeaponAttackType
    {
        Projectile,
        Direct
    }

    /// <summary>
    /// Data source for a weapon's base combat values.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Paw Voyage/Combat/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [SerializeField] private string weaponId = "weapon_id";
        [SerializeField] private string displayName = "Weapon";
        [SerializeField] private WeaponSlotType slotType = WeaponSlotType.Primary;
        [SerializeField] private WeaponAttackType attackType = WeaponAttackType.Projectile;
        [SerializeField] private int baseDamage = 8;
        [SerializeField] private float baseCooldown = 0.45f;
        [SerializeField] private float baseRange = 7f;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private int baseProjectileCount = 1;
        [SerializeField] private int basePierce = 0;

        public string WeaponId => weaponId;
        public string DisplayName => displayName;
        public WeaponSlotType SlotType => slotType;
        public WeaponAttackType AttackType => attackType;
        public int BaseDamage => Mathf.Max(0, baseDamage);
        public float BaseCooldown => Mathf.Max(0.05f, baseCooldown);
        public float BaseRange => Mathf.Max(0.1f, baseRange);
        public float ProjectileSpeed => Mathf.Max(0f, projectileSpeed);
        public int BaseProjectileCount => Mathf.Max(1, baseProjectileCount);
        public int BasePierce => Mathf.Max(0, basePierce);
    }
}
