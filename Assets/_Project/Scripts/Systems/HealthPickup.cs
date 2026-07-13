using PawVoyage.Combat;
using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// 플레이어가 접촉하면 체력을 회복하는 필드 보상입니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HealthPickup : MonoBehaviour
    {
        [SerializeField] private int healAmount = 15;
        [SerializeField] private float lifetime = 18f;
        [SerializeField] private float magnetRadius = 1.4f;
        [SerializeField] private float magnetMoveSpeed = 4.4f;
        [SerializeField] private float instantCollectDistance = 0.25f;
        [SerializeField] private bool createFallbackVisual = true;

        private float despawnTime;
        private Health targetHealth;
        private bool isCollected;

        private void Awake()
        {
            Collider2D pickupCollider = GetComponent<Collider2D>();
            pickupCollider.isTrigger = true;
            EnsureFallbackVisual();
        }

        private void OnEnable()
        {
            despawnTime = Time.time + lifetime;
        }

        private void Update()
        {
            if (Time.time >= despawnTime)
            {
                Destroy(gameObject);
                return;
            }

            MoveTowardPlayerIfClose();
        }

        /// <summary>
        /// 회복량을 설정하고 유지 시간을 갱신합니다.
        /// </summary>
        public void Initialize(int amount)
        {
            healAmount = Mathf.Max(1, amount);
            despawnTime = Time.time + lifetime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<PlayerExperience>(out _) || !other.TryGetComponent(out Health health))
            {
                return;
            }

            Collect(health);
        }

        private void MoveTowardPlayerIfClose()
        {
            FindTargetIfNeeded();
            if (targetHealth == null)
            {
                return;
            }

            Vector2 toPlayer = targetHealth.transform.position - transform.position;
            if (toPlayer.sqrMagnitude > magnetRadius * magnetRadius)
            {
                return;
            }

            if (toPlayer.sqrMagnitude <= instantCollectDistance * instantCollectDistance)
            {
                Collect(targetHealth);
                return;
            }

            transform.position = Vector2.MoveTowards(
                transform.position,
                targetHealth.transform.position,
                magnetMoveSpeed * Time.deltaTime);
        }

        private void Collect(Health health)
        {
            if (isCollected)
            {
                return;
            }

            isCollected = true;
            health.Heal(healAmount);
            Destroy(gameObject);
        }

        private void FindTargetIfNeeded()
        {
            if (targetHealth != null)
            {
                return;
            }

            GameObject playerObject = GameObject.Find("Player");
            if (playerObject != null)
            {
                targetHealth = playerObject.GetComponent<Health>();
            }
        }

        private void EnsureFallbackVisual()
        {
            if (!createFallbackVisual || GetComponent<SpriteRenderer>() != null)
            {
                return;
            }

            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSquareSprite();
            spriteRenderer.color = new Color(1f, 0.18f, 0.28f, 1f);
            spriteRenderer.sortingOrder = 3;
            transform.localScale = Vector3.one * 0.38f;
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
