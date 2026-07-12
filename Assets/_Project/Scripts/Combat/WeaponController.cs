using UnityEngine;

namespace PawVoyage.Combat
{
    /// <summary>
    /// 플레이어가 장착한 무기 데이터를 공격 런타임에 연결합니다.
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
