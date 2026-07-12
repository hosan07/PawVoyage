using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// 플레이어가 접촉하면 경험치를 지급하는 필드 보상 구슬입니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ExperienceOrb : MonoBehaviour
    {
        [SerializeField] private int experienceAmount = 1;
        [SerializeField] private float lifetime = 20f;
        [SerializeField] private bool createFallbackVisual = true;

        private float despawnTime;

        private void Awake()
        {
            Collider2D orbCollider = GetComponent<Collider2D>();
            orbCollider.isTrigger = true;
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
            }
        }

        public void Initialize(int amount)
        {
            experienceAmount = Mathf.Max(1, amount);
            despawnTime = Time.time + lifetime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerExperience playerExperience))
            {
                return;
            }

            playerExperience.AddExperience(experienceAmount);
            GameSfx.PlayExperience();
            Destroy(gameObject);
        }

        private void EnsureFallbackVisual()
        {
            if (!createFallbackVisual || GetComponent<SpriteRenderer>() != null)
            {
                return;
            }

            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSquareSprite();
            spriteRenderer.color = Color.green;
            spriteRenderer.sortingOrder = 2;
            transform.localScale = Vector3.one * 0.35f;
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
