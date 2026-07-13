using System.Collections;
using PawVoyage.Combat;
using UnityEngine;

namespace PawVoyage.Enemy
{
    /// <summary>
    /// 적이 피해를 받을 때 색상 깜빡임과 피해 숫자를 표시합니다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class EnemyHitFeedback : MonoBehaviour
    {
        [SerializeField] private Color hitColor = Color.white;
        [SerializeField] private float flashDuration = 0.08f;
        [SerializeField] private Vector3 popupOffset = new Vector3(0f, 0.55f, 0f);

        private Health health;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Coroutine flashRoutine;

        private void Awake()
        {
            health = GetComponent<Health>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        private void OnEnable()
        {
            health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            health.Damaged -= OnDamaged;
        }

        private void OnDamaged(Health damagedHealth, int amount, bool isCritical)
        {
            if (amount <= 0)
            {
                return;
            }

            DamagePopup.Spawn(transform.position + popupOffset, amount, isCritical);
            Flash();
        }

        private void Flash()
        {
            FindSpriteRendererIfNeeded();

            if (spriteRenderer == null)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private void FindSpriteRendererIfNeeded()
        {
            if (spriteRenderer != null)
            {
                return;
            }

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        private IEnumerator FlashRoutine()
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            flashRoutine = null;
        }
    }
}
