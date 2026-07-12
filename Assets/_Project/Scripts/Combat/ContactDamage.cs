using System.Collections.Generic;
using PawVoyage.Systems;
using UnityEngine;

namespace PawVoyage.Combat
{
    /// <summary>
    /// 프레임 단위 중첩 피해를 막기 위해 대상별 쿨다운을 두고 접촉 피해를 적용합니다.
    /// </summary>
    public class ContactDamage : MonoBehaviour
    {
        [SerializeField] private int damage = 1;
        [SerializeField] private float hitCooldown = 0.5f;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private string targetName = "Player";

        private readonly Dictionary<IDamageable, float> nextHitTimes = new Dictionary<IDamageable, float>();

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryDamage(collision.collider);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDamage(other);
        }

        private void TryDamage(Collider2D other)
        {
            if (!IsValidTarget(other) || !other.TryGetComponent(out IDamageable damageable) || !damageable.CanReceiveDamage)
            {
                return;
            }

            if (nextHitTimes.TryGetValue(damageable, out float nextHitTime) && Time.time < nextHitTime)
            {
                return;
            }

            damageable.ApplyDamage(new DamageRequest(damage, gameObject));
            GameSfx.PlayDamage();
            nextHitTimes[damageable] = Time.time + Mathf.Max(0f, hitCooldown);
        }

        private bool IsValidTarget(Collider2D other)
        {
            bool isTargetLayer = (targetLayers.value & (1 << other.gameObject.layer)) != 0;
            bool isTargetTag = string.IsNullOrWhiteSpace(targetTag) || other.gameObject.tag == targetTag;
            bool isTargetName = string.IsNullOrWhiteSpace(targetName) || other.gameObject.name == targetName;

            return isTargetLayer && (isTargetTag || isTargetName);
        }
    }
}
