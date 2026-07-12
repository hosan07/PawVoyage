using UnityEngine;
using PawVoyage.Systems;

namespace PawVoyage.Combat
{
    /// <summary>
    /// 한 방향으로 이동하며 조건에 맞는 대상에게 피해를 주는 단순 2D 투사체입니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;

        private Vector2 direction = Vector2.right;
        private float speed = 10f;
        private int damage = 1;
        private LayerMask targetLayers = ~0;
        private string targetTag = "Enemy";
        private float despawnTime;

        private void Awake()
        {
            Collider2D projectileCollider = GetComponent<Collider2D>();
            projectileCollider.isTrigger = true;
        }

        private void OnEnable()
        {
            despawnTime = Time.time + lifetime;
        }

        private void Update()
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            if (Time.time >= despawnTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsValidTarget(other))
            {
                return;
            }

            DamageRequest request = new DamageRequest(damage, gameObject);
            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.ApplyDamage(request);
                GameSfx.PlayEnemyHit();
            }
            else
            {
                other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                GameSfx.PlayEnemyHit();
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// 투사체 이동과 피해 데이터를 초기화합니다.
        /// </summary>
        public void Initialize(Vector2 fireDirection, float fireSpeed, int attackDamage, LayerMask layers, string requiredTag)
        {
            direction = fireDirection.sqrMagnitude > 0.001f ? fireDirection.normalized : Vector2.right;
            speed = Mathf.Max(0f, fireSpeed);
            damage = Mathf.Max(0, attackDamage);
            targetLayers = layers;
            targetTag = requiredTag;
            despawnTime = Time.time + lifetime;
        }

        private bool IsValidTarget(Collider2D other)
        {
            bool isTargetLayer = (targetLayers.value & (1 << other.gameObject.layer)) != 0;
            bool isTargetTag = string.IsNullOrWhiteSpace(targetTag) || other.gameObject.tag == targetTag;

            return isTargetLayer && isTargetTag;
        }
    }
}
