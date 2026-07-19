using PawVoyage.Combat;
using PawVoyage.Data;
using PawVoyage.Player;
using PawVoyage.UI;
using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// Farmer 본체와 메인 메뉴에서 선택한 동행 펫을 전투 시작 시 구성합니다.
    /// </summary>
    [RequireComponent(typeof(WeaponController))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAnimalLoadoutApplier : MonoBehaviour
    {
        [Header("Farmer")]
        [SerializeField] private int farmerBaseHp = 120;
        [SerializeField] private float farmerMoveSpeed = 5f;
        [SerializeField] private WeaponData farmerPrimaryWeapon = null;
        [SerializeField] private float farmerVisualScale = 2.15f;

        [Header("Pet")]
        [SerializeField] private AnimalData dogAnimalData = null;
        [SerializeField] private AnimalData catAnimalData = null;
        [SerializeField] private WeaponData dogPetWeapon = null;
        [SerializeField] private WeaponData catPetWeapon = null;
        [SerializeField] private Vector2 petSpawnOffset = new Vector2(-0.65f, -0.35f);

        public AnimalData CurrentPetData { get; private set; }
        public PetCompanionController CurrentPet { get; private set; }

        private void Awake()
        {
            ApplyFarmerAndSelectedPet();
        }

        /// <summary>
        /// 저장된 동료 선택값을 Farmer와 별도 펫 오브젝트에 반영합니다.
        /// </summary>
        public void ApplyFarmerAndSelectedPet()
        {
            ApplyFarmerLoadout();
            SpawnSelectedPet();
        }

        private void ApplyFarmerLoadout()
        {
            GetComponent<Health>().SetBaseMaxHp(farmerBaseHp, true);
            GetComponent<PlayerController>().MoveSpeed = farmerMoveSpeed;
            GetComponent<WeaponController>().EquipPrimary(farmerPrimaryWeapon);

            FarmerVisualController visualController = GetComponent<FarmerVisualController>();
            if (visualController == null)
            {
                visualController = gameObject.AddComponent<FarmerVisualController>();
            }

            visualController.Configure(UiIconDrawer.GetSprite(UiIconDrawer.FarmerAvatar), farmerVisualScale);
        }

        private void SpawnSelectedPet()
        {
            if (CurrentPet != null)
            {
                Destroy(CurrentPet.gameObject);
            }

            SelectedAnimalType selectedAnimal = AnimalSelectionData.SelectedAnimal;
            bool useCat = selectedAnimal == SelectedAnimalType.Cat;
            CurrentPetData = useCat ? catAnimalData : dogAnimalData;
            WeaponData petWeapon = useCat ? catPetWeapon : dogPetWeapon;

            GameObject petObject = new GameObject(useCat ? "Pet_Cat" : "Pet_Dog");
            petObject.transform.position = (Vector2)transform.position + petSpawnOffset;

            Rigidbody2D rigidbody2D = petObject.AddComponent<Rigidbody2D>();
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            rigidbody2D.gravityScale = 0f;

            AutoAttack autoAttack = petObject.AddComponent<AutoAttack>();
            autoAttack.SetWeapon(petWeapon);

            petObject.AddComponent<WeaponController>().EquipPrimary(petWeapon);
            petObject.AddComponent<SpriteRenderer>();
            CurrentPet = petObject.AddComponent<PetCompanionController>();
            CurrentPet.Initialize(transform, CurrentPetData, petWeapon, selectedAnimal);
        }
    }
}
