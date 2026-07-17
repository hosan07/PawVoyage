using PawVoyage.Combat;
using PawVoyage.Data;
using UnityEngine;

namespace PawVoyage.Enemy
{
    /// <summary>
    /// 플레이어를 추적하고, 피해를 받으며, 체력이 0이 되면 사망하는 기본 적 동작입니다.
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
        [SerializeField] private MonsterBehaviorType behaviorType = MonsterBehaviorType.Chase;
        [SerializeField] private float zigzagAmplitude = 0.7f;
        [SerializeField] private float zigzagFrequency = 4.5f;
        [SerializeField] private float chargeWindupSeconds = 0.42f;
        [SerializeField] private float chargeSeconds = 0.48f;
        [SerializeField] private float chargeCooldownSeconds = 2.1f;
        [SerializeField] private float chargeSpeedMultiplier = 3.2f;
        [SerializeField] private float eliteEnrageHpRatio = 0.5f;
        [SerializeField] private float eliteEnrageSpeedMultiplier = 1.55f;
        [SerializeField] private float eliteEnrageDamageMultiplier = 1.35f;
        [SerializeField] private float shooterPreferredDistance = 4.2f;
        [SerializeField] private float shooterRetreatDistance = 2.5f;
        [SerializeField] private float shooterFireInterval = 2.15f;
        [SerializeField] private float shooterProjectileSpeed = 3.4f;
        [SerializeField] private int shooterProjectileDamage = 3;

        private Rigidbody2D rb;
        private Health health;
        private ContactDamage contactDamage;
        private SpriteRenderer spriteRenderer;
        private Color baseColor = Color.white;
        private float nextChargeTime;
        private float nextShootTime;
        private float chargeStateEndTime;
        private Vector2 chargeDirection = Vector2.right;
        private ChargeState chargeState;
        private bool eliteEnrageApplied;

        private enum ChargeState
        {
            Ready,
            Windup,
            Charging
        }

        /// <summary>
        /// 이동 대상입니다. 보통 플레이어를 가리킵니다.
        /// </summary>
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        /// <summary>
        /// 런타임 난이도나 적 타입에 맞춰 이동 속도를 설정합니다.
        /// </summary>
        public void SetMoveSpeed(float value)
        {
            moveSpeed = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 몬스터 데이터에서 지정한 이동/공격 행동 타입을 적용합니다.
        /// </summary>
        public void SetBehavior(MonsterBehaviorType value)
        {
            behaviorType = value;
        }

        /// <summary>
        /// 패턴 예고 표시 후 돌아갈 기본 표시 색상을 설정합니다.
        /// </summary>
        public void SetVisualBaseColor(Color color)
        {
            baseColor = color;
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = baseColor;
            }
        }

        /// <summary>
        /// 원거리 적의 탄 피해와 탄속을 런타임 난이도에 맞춰 설정합니다.
        /// </summary>
        public void SetShooterStats(int projectileDamage, float projectileSpeed)
        {
            shooterProjectileDamage = Mathf.Max(1, projectileDamage);
            shooterProjectileSpeed = Mathf.Max(0.5f, projectileSpeed);
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            contactDamage = GetComponent<ContactDamage>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            EnsureCollider();
            EnsureFallbackVisual();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                baseColor = spriteRenderer.color;
            }
        }

        private void Update()
        {
            if (target == null)
            {
                FindTarget();
            }

            UpdateVisualState();
            ApplyEliteEnrageIfNeeded();
            TryShoot();
        }

        private void FixedUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector2 direction = GetMoveDirection();
            float currentMoveSpeed = GetCurrentMoveSpeed();
            Vector2 nextPosition = rb.position + direction * currentMoveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(nextPosition);
        }

        private Vector2 GetMoveDirection()
        {
            Vector2 toTarget = ((Vector2)target.position - rb.position).normalized;
            return behaviorType switch
            {
                MonsterBehaviorType.Zigzag => GetZigzagDirection(toTarget),
                MonsterBehaviorType.Charger => GetChargeDirection(toTarget),
                MonsterBehaviorType.Shooter => GetShooterDirection(toTarget),
                MonsterBehaviorType.EliteBrute => GetEliteDirection(toTarget),
                _ => toTarget
            };
        }

        private Vector2 GetZigzagDirection(Vector2 toTarget)
        {
            Vector2 side = new Vector2(-toTarget.y, toTarget.x);
            float wave = Mathf.Sin(Time.time * zigzagFrequency + GetInstanceID() * 0.17f) * zigzagAmplitude;
            return (toTarget + side * wave).normalized;
        }

        private Vector2 GetShooterDirection(Vector2 toTarget)
        {
            float distance = Vector2.Distance(rb.position, target.position);
            if (distance < shooterRetreatDistance)
            {
                return -toTarget;
            }

            if (distance > shooterPreferredDistance)
            {
                return toTarget;
            }

            Vector2 side = new Vector2(-toTarget.y, toTarget.x);
            float wave = Mathf.Sin(Time.time * 2.6f + GetInstanceID() * 0.11f);
            return side * Mathf.Sign(wave == 0f ? 1f : wave);
        }

        private Vector2 GetChargeDirection(Vector2 toTarget)
        {
            UpdateChargeState(toTarget);
            return chargeState == ChargeState.Charging ? chargeDirection : toTarget;
        }

        private Vector2 GetEliteDirection(Vector2 toTarget)
        {
            if (IsEliteEnraged())
            {
                return GetZigzagDirection(toTarget);
            }

            return GetChargeDirection(toTarget);
        }

        private void UpdateChargeState(Vector2 toTarget)
        {
            if (chargeState == ChargeState.Windup && Time.time >= chargeStateEndTime)
            {
                chargeState = ChargeState.Charging;
                chargeStateEndTime = Time.time + chargeSeconds;
                chargeDirection = toTarget.sqrMagnitude > 0.001f ? toTarget : chargeDirection;
                return;
            }

            if (chargeState == ChargeState.Charging && Time.time >= chargeStateEndTime)
            {
                chargeState = ChargeState.Ready;
                nextChargeTime = Time.time + chargeCooldownSeconds;
                return;
            }

            if (chargeState == ChargeState.Ready && Time.time >= nextChargeTime)
            {
                chargeState = ChargeState.Windup;
                chargeStateEndTime = Time.time + chargeWindupSeconds;
                chargeDirection = toTarget.sqrMagnitude > 0.001f ? toTarget : chargeDirection;
            }
        }

        private float GetCurrentMoveSpeed()
        {
            float speed = moveSpeed;
            if (chargeState == ChargeState.Windup)
            {
                speed = 0f;
            }
            else if (chargeState == ChargeState.Charging)
            {
                speed *= chargeSpeedMultiplier;
            }

            if (behaviorType == MonsterBehaviorType.EliteBrute && IsEliteEnraged())
            {
                speed *= eliteEnrageSpeedMultiplier;
            }

            return speed;
        }

        private void TryShoot()
        {
            if (behaviorType != MonsterBehaviorType.Shooter || target == null || Time.time < nextShootTime)
            {
                return;
            }

            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            nextShootTime = Time.time + shooterFireInterval;
            EnemyProjectile.Spawn(transform.position, direction, shooterProjectileSpeed, shooterProjectileDamage);
        }

        private bool IsEliteEnraged()
        {
            return health != null && health.CurrentHp <= Mathf.CeilToInt(health.MaxHp * eliteEnrageHpRatio);
        }

        private void ApplyEliteEnrageIfNeeded()
        {
            if (eliteEnrageApplied || behaviorType != MonsterBehaviorType.EliteBrute || !IsEliteEnraged())
            {
                return;
            }

            eliteEnrageApplied = true;
            if (contactDamage != null)
            {
                contactDamage.SetDamage(Mathf.Max(contactDamage.Damage + 2, Mathf.RoundToInt(contactDamage.Damage * eliteEnrageDamageMultiplier)));
                contactDamage.SetHitCooldown(0.22f);
            }
        }

        private void UpdateVisualState()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (chargeState == ChargeState.Windup)
            {
                float pulse = Mathf.PingPong(Time.time * 8f, 1f);
                spriteRenderer.color = Color.Lerp(baseColor, Color.white, 0.35f + pulse * 0.45f);
                return;
            }

            if (behaviorType == MonsterBehaviorType.EliteBrute && IsEliteEnraged())
            {
                float pulse = Mathf.PingPong(Time.time * 4f, 1f);
                spriteRenderer.color = Color.Lerp(baseColor, new Color(1f, 0.08f, 0.08f, 1f), 0.35f + pulse * 0.25f);
                return;
            }

            spriteRenderer.color = baseColor;
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

    /// <summary>
    /// 원거리 적이 발사하는 느린 단일 투사체입니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    internal class EnemyProjectile : MonoBehaviour
    {
        private static Sprite projectileSprite;

        [SerializeField] private float lifetimeSeconds = 4.5f;
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private string targetName = "Player";

        private int damage = 1;
        private Rigidbody2D rb;

        public static EnemyProjectile Spawn(Vector2 position, Vector2 direction, float speed, int damage)
        {
            GameObject projectileObject = new GameObject("EnemyProjectile");
            projectileObject.transform.position = position;

            CircleCollider2D collider = projectileObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.16f;

            SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetProjectileSprite();
            renderer.color = new Color(0.65f, 0.95f, 1f, 1f);
            projectileObject.transform.localScale = Vector3.one * 0.36f;

            EnemyProjectile projectile = projectileObject.AddComponent<EnemyProjectile>();
            projectile.Initialize(direction, speed, damage);
            return projectile;
        }

        /// <summary>
        /// 생성 직후 진행 방향, 속도, 피해량을 설정합니다.
        /// </summary>
        public void Initialize(Vector2 direction, float speed, int attackDamage)
        {
            damage = Mathf.Max(1, attackDamage);
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.linearVelocity = direction.normalized * Mathf.Max(0.5f, speed);
            Destroy(gameObject, Mathf.Max(0.1f, lifetimeSeconds));
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsValidTarget(other) || !other.TryGetComponent(out IDamageable damageable) || !damageable.CanReceiveDamage)
            {
                return;
            }

            damageable.ApplyDamage(new DamageRequest(damage, gameObject));
            PawVoyage.Systems.GameSfx.PlayDamage();
            Destroy(gameObject);
        }

        private bool IsValidTarget(Collider2D other)
        {
            bool isTargetTag = string.IsNullOrWhiteSpace(targetTag) || other.gameObject.tag == targetTag;
            bool isTargetName = string.IsNullOrWhiteSpace(targetName) || other.gameObject.name == targetName;
            return isTargetTag || isTargetName;
        }

        private static Sprite GetProjectileSprite()
        {
            if (projectileSprite != null)
            {
                return projectileSprite;
            }

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            projectileSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return projectileSprite;
        }
    }
}
