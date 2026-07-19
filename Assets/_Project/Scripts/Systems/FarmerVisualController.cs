using PawVoyage.Combat;
using PawVoyage.Player;
using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// Farmer 본체의 방향 전환과 공격 순간의 짧은 시각 피드백을 담당합니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(AutoAttack))]
    public class FarmerVisualController : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private PlayerController playerController;
        private AutoAttack autoAttack;
        private Vector3 baseScale = Vector3.one;
        private float attackPulse;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerController = GetComponent<PlayerController>();
            autoAttack = GetComponent<AutoAttack>();
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
            if (playerController == null)
            {
                return;
            }

            if (Mathf.Abs(playerController.LastMoveDirection.x) > 0.01f)
            {
                spriteRenderer.flipX = playerController.LastMoveDirection.x < 0f;
            }

            attackPulse = Mathf.MoveTowards(attackPulse, 0f, Time.deltaTime * 8f);
            transform.localScale = baseScale * (1f + attackPulse * 0.1f);
        }

        /// <summary>
        /// Farmer 아이콘과 전투에서 사용할 기본 크기를 설정합니다.
        /// </summary>
        public void Configure(Sprite farmerSprite, float visualScale)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (farmerSprite != null)
            {
                spriteRenderer.sprite = farmerSprite;
                spriteRenderer.color = Color.white;
            }

            spriteRenderer.sortingOrder = 5;
            baseScale = Vector3.one * Mathf.Max(0.1f, visualScale);
            transform.localScale = baseScale;
        }

        private void PlayAttackFeedback()
        {
            attackPulse = 1f;
        }
    }
}
