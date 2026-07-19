using System.Collections;
using PawVoyage.Combat;
using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// First Night Raid에서 지켜야 하는 중앙 Barn 목표물입니다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Collider2D))]
    public class BarnObjective : MonoBehaviour
    {
        private static Sprite fallbackSprite;

        [SerializeField] private int baseMaxHp = 520;
        [SerializeField, Range(0f, 0.75f)] private float defense = 0.08f;
        [SerializeField] private Vector3 damagePopupOffset = new Vector3(0f, 0.9f, 0f);
        [SerializeField] private float feedbackSeconds = 0.16f;

        private Health health;
        private SpriteRenderer spriteRenderer;
        private Color baseColor;
        private Vector3 baseScale;
        private Vector3 basePosition;
        private Coroutine feedbackRoutine;

        public static BarnObjective Instance { get; private set; }

        public Health Health => health;
        public int CurrentHp => health != null ? health.CurrentHp : 0;
        public int MaxHp => health != null ? health.MaxHp : Mathf.Max(1, baseMaxHp);
        public bool IsAlive => health != null && health.CurrentHp > 0;
        public float HpRatio => MaxHp > 0 ? Mathf.Clamp01((float)CurrentHp / MaxHp) : 0f;
        public int DamageTaken { get; private set; }

        public static BarnObjective CreateDefaultIfMissing(int maxHp, float defense)
        {
            BarnObjective existing = Instance;
            if (existing == null)
            {
                existing = Object.FindFirstObjectByType<BarnObjective>();
            }

            if (existing != null)
            {
                existing.Configure(maxHp, defense);
                return existing;
            }

            GameObject barnObject = new GameObject("Barn");
            barnObject.transform.position = Vector3.zero;
            barnObject.transform.localScale = new Vector3(1.35f, 1.05f, 1f);

            BoxCollider2D collider = barnObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1.4f, 1.1f);

            SpriteRenderer renderer = barnObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetFallbackSprite();
            renderer.color = new Color(0.62f, 0.28f, 0.18f, 1f);
            renderer.sortingOrder = 0;

            Health health = barnObject.AddComponent<Health>();
            BarnObjective barn = barnObject.AddComponent<BarnObjective>();
            barn.Configure(maxHp, defense);
            health.SetBaseMaxHp(maxHp, true);
            return barn;
        }

        /// <summary>
        /// StageData에서 넘어온 Barn 기본 체력과 방어율을 적용합니다.
        /// </summary>
        public void Configure(int maxHp, float incomingDefense)
        {
            baseMaxHp = Mathf.Max(1, maxHp);
            defense = Mathf.Clamp(incomingDefense, 0f, 0.75f);

            if (health == null)
            {
                health = GetComponent<Health>();
            }

            health.SetBaseMaxHp(baseMaxHp, true);
            health.SetDamageReduction(defense);
        }

        private void Awake()
        {
            Instance = this;
            health = GetComponent<Health>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            baseScale = transform.localScale;
            basePosition = transform.position;
        }

        private void OnEnable()
        {
            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }

        private void OnDisable()
        {
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnDamaged(Health damagedHealth, int amount, bool isCritical)
        {
            int displayedAmount = Mathf.Max(0, amount);
            DamageTaken += Mathf.Max(0, displayedAmount);
            RunStats.Instance?.AddBarnDamageTaken(displayedAmount, CurrentHp, MaxHp);
            DamagePopup.Spawn(transform.position + damagePopupOffset, displayedAmount, false);
            GameSfx.PlayDamage();
            PlayFeedback();
        }

        private void OnDied(Health deadHealth)
        {
            RunStats.Instance?.FailRun(RunFailureReason.BarnDestroyed);
        }

        private void PlayFeedback()
        {
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(FeedbackRoutine());
        }

        private IEnumerator FeedbackRoutine()
        {
            float elapsed = 0f;
            while (elapsed < feedbackSeconds)
            {
                elapsed += Time.deltaTime;
                float pulse = Mathf.PingPong(elapsed * 18f, 1f);
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(baseColor, Color.red, 0.45f + pulse * 0.35f);
                }

                transform.localScale = baseScale * (1f + pulse * 0.08f);
                transform.position = basePosition + new Vector3(Mathf.Sin(elapsed * 70f) * 0.035f, 0f, 0f);
                yield return null;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = baseColor;
            }

            transform.localScale = baseScale;
            transform.position = basePosition;
            feedbackRoutine = null;
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
