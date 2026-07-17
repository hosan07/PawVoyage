using PawVoyage.Combat;
using PawVoyage.Data;
using PawVoyage.Player;
using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// 메인 메뉴에서 선택한 동물에 맞춰 기본 스탯과 주무기를 적용합니다.
    /// </summary>
    [RequireComponent(typeof(WeaponController))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAnimalLoadoutApplier : MonoBehaviour
    {
        [SerializeField] private AnimalData dogAnimalData = null;
        [SerializeField] private AnimalData catAnimalData = null;
        [SerializeField] private WeaponData dogPrimaryWeapon = null;
        [SerializeField] private WeaponData catPrimaryWeapon = null;

        public AnimalData CurrentAnimalData { get; private set; }

        private void Awake()
        {
            ApplySelectedAnimal();
        }

        /// <summary>
        /// 저장된 동물 선택값을 현재 Player 런타임 컴포넌트에 반영합니다.
        /// </summary>
        public void ApplySelectedAnimal()
        {
            bool useCat = AnimalSelectionData.SelectedAnimal == SelectedAnimalType.Cat;
            CurrentAnimalData = useCat ? catAnimalData : dogAnimalData;
            WeaponData primaryWeapon = useCat ? catPrimaryWeapon : dogPrimaryWeapon;

            if (CurrentAnimalData != null)
            {
                GetComponent<Health>().SetBaseMaxHp(CurrentAnimalData.BaseHp, true);
                GetComponent<PlayerController>().MoveSpeed = CurrentAnimalData.BaseSpeed;
            }

            GetComponent<WeaponController>().EquipPrimary(primaryWeapon);
        }
    }
}
