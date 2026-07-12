using UnityEngine;

namespace PawVoyage.Combat
{
    /// <summary>
    /// Bridges the player's equipped weapon data into the attack runtime.
    /// </summary>
    [RequireComponent(typeof(AutoAttack))]
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private WeaponData primaryWeapon = null;
        [SerializeField] private AutoAttack autoAttack = null;

        public WeaponData PrimaryWeapon => primaryWeapon;

        private void Awake()
        {
            ApplyWeapon();
        }

        private void OnValidate()
        {
            ApplyWeapon();
        }

        public void EquipPrimary(WeaponData weaponData)
        {
            primaryWeapon = weaponData;
            ApplyWeapon();
        }

        private void ApplyWeapon()
        {
            if (autoAttack == null)
            {
                autoAttack = GetComponent<AutoAttack>();
            }

            if (autoAttack != null)
            {
                autoAttack.SetWeapon(primaryWeapon);
            }
        }
    }
}
