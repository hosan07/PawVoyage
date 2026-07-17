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

        private Rigidbody2D rb;
        private Health health;
        private float nextChargeTime;
        private float chargeStateEndTime;
        private Vector2 chargeDirection = Vector2.right;
        private ChargeState chargeState;

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

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
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

        private bool IsEliteEnraged()
        {
            return health != null && health.CurrentHp <= Mathf.CeilToInt(health.MaxHp * eliteEnrageHpRatio);
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
