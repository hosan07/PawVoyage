using UnityEngine;
using PawVoyage.Enemy;
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
        [SerializeField] private bool createFallbackVisual = true;
        [SerializeField] private Color fallbackColor = new Color(1f, 0.92f, 0.1f, 1f);

        private Vector2 direction = Vector2.right;
        private float speed = 10f;
        private int damage = 1;
        private int pierceRemaining;
        private LayerMask targetLayers = ~0;
        private string targetTag = "Enemy";
        private float despawnTime;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            Collider2D projectileCollider = GetComponent<Collider2D>();
            projectileCollider.isTrigger = true;
            EnsureFallbackVisual();
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

            if (pierceRemaining <= 0)
            {
                Destroy(gameObject);
                return;
            }

            pierceRemaining--;
        }

        /// <summary>
        /// 투사체 이동과 피해 데이터를 초기화합니다.
        /// </summary>
        public void Initialize(Vector2 fireDirection, float fireSpeed, int attackDamage, LayerMask layers, string requiredTag, int pierceCount = 0)
        {
            direction = fireDirection.sqrMagnitude > 0.001f ? fireDirection.normalized : Vector2.right;
            speed = Mathf.Max(0f, fireSpeed);
            damage = Mathf.Max(0, attackDamage);
            pierceRemaining = Mathf.Max(0, pierceCount);
            targetLayers = layers;
            targetTag = requiredTag;
            despawnTime = Time.time + lifetime;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        private bool IsValidTarget(Collider2D other)
        {
            bool isTargetLayer = (targetLayers.value & (1 << other.gameObject.layer)) != 0;
            bool isTargetTag = other.GetComponent<EnemyController>() != null
                || string.IsNullOrWhiteSpace(targetTag)
                || other.gameObject.tag == targetTag;

            return isTargetLayer && isTargetTag;
        }

        private void EnsureFallbackVisual()
        {
            if (!createFallbackVisual || GetComponent<SpriteRenderer>() != null)
            {
                return;
            }

            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSquareSprite();
            spriteRenderer.color = fallbackColor;
            spriteRenderer.sortingOrder = 5;
            transform.localScale = new Vector3(0.7f, 0.22f, 1f);
        }

        private static Sprite CreateSquareSprite()
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
