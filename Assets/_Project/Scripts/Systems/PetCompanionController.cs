using PawVoyage.Combat;
using PawVoyage.Data;
using PawVoyage.UI;
using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// Farmer를 따라다니며 선택된 동료 무기로 자동 공격하는 임시 펫 컨트롤러입니다.
    /// </summary>
    [RequireComponent(typeof(AutoAttack))]
    [RequireComponent(typeof(WeaponController))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PetCompanionController : MonoBehaviour
    {
        [SerializeField] private Transform followTarget = null;
        [SerializeField] private Vector2 followOffset = new Vector2(-0.65f, -0.35f);
        [SerializeField] private float followSmooth = 12f;
        [SerializeField] private float bobAmplitude = 0.08f;
        [SerializeField] private float bobSpeed = 5f;
        [SerializeField] private Color dogColor = new Color(0.9f, 0.58f, 0.28f, 1f);
        [SerializeField] private Color catColor = new Color(0.55f, 0.6f, 0.68f, 1f);

        private SpriteRenderer spriteRenderer;
        private AutoAttack autoAttack;
        private SelectedAnimalType animalType = SelectedAnimalType.Dog;
        private Vector2 smoothedPosition;
        private Vector3 baseScale;
        private float attackPulse;

        public SelectedAnimalType AnimalType => animalType;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            autoAttack = GetComponent<AutoAttack>();
            smoothedPosition = transform.position;
        }

        private void OnEnable()
        {
            if (autoAttack != null)
            {
                autoAttack.AttackPerformed += PlayAttackFeedback;
            }
        }

        private void OnDisable()
        {
            if (autoAttack != null)
            {
                autoAttack.AttackPerformed -= PlayAttackFeedback;
            }
        }

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                return;
            }

            Vector2 targetPosition = (Vector2)followTarget.position + followOffset;
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            targetPosition.y += bob;
            smoothedPosition = Vector2.Lerp(smoothedPosition, targetPosition, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
            transform.position = smoothedPosition;

            attackPulse = Mathf.MoveTowards(attackPulse, 0f, Time.deltaTime * 7f);
            float pulseScale = 1f + attackPulse * 0.16f;
            transform.localScale = baseScale * pulseScale;
        }

        public void Initialize(Transform target, AnimalData animalData, WeaponData weaponData, SelectedAnimalType selectedAnimal)
        {
            followTarget = target;
            animalType = selectedAnimal;
            gameObject.name = selectedAnimal == SelectedAnimalType.Cat ? "Pet_Cat" : "Pet_Dog";

            GetComponent<WeaponController>().EquipPrimary(weaponData);
            ConfigureVisual(selectedAnimal, animalData);
        }

        private void ConfigureVisual(SelectedAnimalType selectedAnimal, AnimalData animalData)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            string iconPath = selectedAnimal == SelectedAnimalType.Cat
                ? UiIconDrawer.CompanionCat
                : UiIconDrawer.CompanionDog;
            Sprite iconSprite = UiIconDrawer.GetSprite(iconPath);
            spriteRenderer.sprite = iconSprite ?? CreateFallbackSprite();
            spriteRenderer.color = iconSprite != null ? Color.white : selectedAnimal == SelectedAnimalType.Cat ? catColor : dogColor;
            spriteRenderer.sortingOrder = 4;
            baseScale = Vector3.one * GetVisualScale(animalData);
            transform.localScale = baseScale;
        }

        private static float GetVisualScale(AnimalData animalData)
        {
            if (animalData == null)
            {
                return 0.38f;
            }

            return animalData.AttackPattern == AnimalAttackPattern.Ranged ? 0.52f : 0.56f;
        }

        private void PlayAttackFeedback()
        {
            attackPulse = 1f;
        }

        private static Sprite CreateFallbackSprite()
        {
            Texture2D texture = new Texture2D(8, 8);
            Color[] pixels = new Color[8 * 8];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 8f);
        }
    }
}
