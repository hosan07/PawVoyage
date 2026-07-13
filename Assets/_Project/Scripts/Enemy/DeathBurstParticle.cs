using UnityEngine;

namespace PawVoyage.Enemy
{
    /// <summary>
    /// 사망 버스트에서 쓰는 단일 파편입니다.
    /// </summary>
    public class DeathBurstParticle : MonoBehaviour
    {
        private static Sprite fallbackSprite;

        private SpriteRenderer spriteRenderer;
        private Vector3 velocity;
        private float lifetime;
        private float elapsed;
        private Color startColor;

        public static void Spawn(Vector3 position, Vector2 velocity, float lifetime, float scale, Color color)
        {
            GameObject particleObject = new GameObject("DeathBurstParticle");
            particleObject.transform.position = position;
            particleObject.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);

            DeathBurstParticle particle = particleObject.AddComponent<DeathBurstParticle>();
            particle.Initialize(velocity, lifetime, color);
        }

        private void Initialize(Vector2 initialVelocity, float duration, Color color)
        {
            velocity = initialVelocity;
            lifetime = Mathf.Max(0.05f, duration);
            startColor = color;

            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetFallbackSprite();
            spriteRenderer.color = startColor;
            spriteRenderer.sortingOrder = 12;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 4f);

            float progress = Mathf.Clamp01(elapsed / lifetime);
            Color color = startColor;
            color.a = 1f - progress;
            spriteRenderer.color = color;

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private static Sprite GetFallbackSprite()
        {
            if (fallbackSprite != null)
            {
                return fallbackSprite;
            }

            Texture2D texture = new Texture2D(8, 8);
            Color[] pixels = new Color[8 * 8];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 8f);

            return fallbackSprite;
        }
    }
}
