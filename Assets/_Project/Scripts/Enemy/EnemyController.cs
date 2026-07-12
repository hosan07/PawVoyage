using PawVoyage.Combat;
using UnityEngine;

namespace PawVoyage.Enemy
{
    /// <summary>
    /// Basic enemy behavior: follow the player, receive damage, and die at zero HP.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public class EnemyController : MonoBehaviour
    {
        private static Sprite fallbackSprite;

        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private Transform target;
        [SerializeField] private string targetName = "Player";
        [SerializeField] private bool createFallbackVisual = true;

        private Rigidbody2D rb;

        /// <summary>
        /// Movement target, usually the player.
        /// </summary>
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            EnsureCollider();
            EnsureFallbackVisual();
        }

        private void Update()
        {
            if (target == null)
            {
                FindTarget();
            }
        }

        private void FixedUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector2 direction = ((Vector2)target.position - rb.position).normalized;
            Vector2 nextPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(nextPosition);
        }

        private void FindTarget()
        {
            GameObject targetObject = GameObject.Find(targetName);
            if (targetObject != null)
            {
                target = targetObject.transform;
            }
        }

        private void EnsureCollider()
        {
            if (GetComponent<Collider2D>() != null)
            {
                return;
            }

            CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.radius = 0.35f;
        }

        private void EnsureFallbackVisual()
        {
            if (!createFallbackVisual || GetComponent<SpriteRenderer>() != null)
            {
                return;
            }

            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetFallbackSprite();
            spriteRenderer.color = Color.red;
            spriteRenderer.sortingOrder = 1;
        }

        private static Sprite GetFallbackSprite()
        {
            if (fallbackSprite != null)
            {
                return fallbackSprite;
            }

            Texture2D texture = new Texture2D(16, 16);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);

            return fallbackSprite;
        }
    }
}
