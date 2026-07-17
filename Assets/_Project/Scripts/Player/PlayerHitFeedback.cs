using System.Collections;
using PawVoyage.Combat;
using PawVoyage.Systems;
using UnityEngine;

namespace PawVoyage.Player
{
    /// <summary>
    /// 플레이어가 피해를 받을 때 색상 깜빡임과 피해 숫자를 표시합니다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerHitFeedback : MonoBehaviour
    {
        [SerializeField] private Color hitColor = new Color(1f, 0.25f, 0.25f, 1f);
        [SerializeField] private float flashDuration = 0.12f;
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

            DamagePopup.Spawn(transform.position + popupOffset, amount, false);
            RunStats.Instance?.AddDamageTaken(amount);
            Flash();
        }

        private void Flash()
        {
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

        private IEnumerator FlashRoutine()
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            flashRoutine = null;
        }
    }
}
