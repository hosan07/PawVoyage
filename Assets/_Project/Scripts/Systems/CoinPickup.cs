using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// 플레이어가 접촉하면 현재 런의 코인 획득량을 증가시키는 필드 보상입니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CoinPickup : MonoBehaviour
    {
        [SerializeField] private int coinAmount = 1;
        [SerializeField] private float lifetime = 22f;
        [SerializeField] private float magnetRadius = 1.6f;
        [SerializeField] private float magnetMoveSpeed = 5.4f;
        [SerializeField] private float instantCollectDistance = 0.25f;
        [SerializeField] private bool createFallbackVisual = true;

        private float despawnTime;
        private PlayerExperience targetPlayer;
        private bool isCollected;

        private void Awake()
        {
            Collider2D coinCollider = GetComponent<Collider2D>();
            coinCollider.isTrigger = true;
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
        /// 지급 코인량을 설정하고 유지 시간을 갱신합니다.
        /// </summary>
        public void Initialize(int amount)
        {
            coinAmount = Mathf.Max(1, amount);
            despawnTime = Time.time + lifetime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerExperience playerExperience))
            {
                return;
            }

            Collect(playerExperience);
        }

        private void MoveTowardPlayerIfClose()
        {
            FindTargetIfNeeded();
            if (targetPlayer == null)
            {
                return;
            }

            Vector2 toPlayer = targetPlayer.transform.position - transform.position;
            float effectiveMagnetRadius = magnetRadius * targetPlayer.PickupRadiusMultiplier;
            if (toPlayer.sqrMagnitude > effectiveMagnetRadius * effectiveMagnetRadius)
            {
                return;
            }

            if (toPlayer.sqrMagnitude <= instantCollectDistance * instantCollectDistance)
            {
                Collect(targetPlayer);
                return;
            }

            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPlayer.transform.position,
                magnetMoveSpeed * Time.deltaTime);
        }

        private void Collect(PlayerExperience playerExperience)
        {
            if (isCollected)
            {
                return;
            }

            isCollected = true;
            RunStats.Instance?.AddCoins(coinAmount);
            GameSfx.PlayCoin();
            Destroy(gameObject);
        }

        private void FindTargetIfNeeded()
        {
            if (targetPlayer != null)
            {
                return;
            }

            targetPlayer = FindFirstObjectByType<PlayerExperience>();
        }

        private void EnsureFallbackVisual()
        {
            if (!createFallbackVisual || GetComponent<SpriteRenderer>() != null)
            {
                return;
            }

            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSquareSprite();
            spriteRenderer.color = new Color(1f, 0.85f, 0.1f, 1f);
            spriteRenderer.sortingOrder = 4;
            transform.localScale = Vector3.one * 0.28f;
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
