using System.Collections.Generic;
using PawVoyage.Enemy;
using PawVoyage.Systems;
using UnityEngine;

namespace PawVoyage.Combat
{
    /// <summary>
    /// 레벨업 카드로 획득한 보조무기와 강화 레벨을 관리합니다.
    /// </summary>
    public class SecondaryWeaponController : MonoBehaviour
    {
        private const int MaxTargets = 48;

        [SerializeField] private int maxSecondaryWeapons = 3;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private string targetTag = "Enemy";

        private readonly List<SecondaryWeaponState> weapons = new List<SecondaryWeaponState>();
        private readonly Collider2D[] targetBuffer = new Collider2D[MaxTargets];
        private ContactFilter2D targetFilter;

        public int EquippedCount => weapons.Count;
        public int MaxSecondaryWeapons => Mathf.Max(0, maxSecondaryWeapons);

        private void Awake()
        {
            targetFilter = new ContactFilter2D();
            targetFilter.SetLayerMask(targetLayers);
            targetFilter.useTriggers = true;
        }

        private void OnValidate()
        {
            targetFilter.SetLayerMask(targetLayers);
            targetFilter.useTriggers = true;
        }

        private void Update()
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                SecondaryWeaponState state = weapons[i];
                if (Time.time < state.NextFireTime)
                {
                    continue;
                }

                FireSecondaryWeapon(state);
                state.NextFireTime = Time.time + GetCooldown(state);
                weapons[i] = state;
            }
        }

        /// <summary>
        /// 보조무기를 새로 획득하거나 이미 보유 중이면 강화합니다.
        /// </summary>
        public bool AcquireOrUpgradeWeapon(WeaponData weaponData)
        {
            if (weaponData == null)
            {
                return false;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                SecondaryWeaponState state = weapons[i];
                if (state.WeaponData != weaponData)
                {
                    continue;
                }

                state.Level++;
                weapons[i] = state;
                return true;
            }

            if (weapons.Count >= MaxSecondaryWeapons)
            {
                return false;
            }

            weapons.Add(new SecondaryWeaponState(weaponData));
            return true;
        }

        /// <summary>
        /// 해당 보조무기를 이미 보유 중인지 확인합니다.
        /// </summary>
        public bool HasWeapon(WeaponData weaponData)
        {
            if (weaponData == null)
            {
                return false;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i].WeaponData == weaponData)
                {
                    return true;
                }
            }

            return false;
        }

        private void FireSecondaryWeapon(SecondaryWeaponState state)
        {
            switch (state.WeaponData.AttackType)
            {
                case WeaponAttackType.Aura:
                    FireAura(state);
                    break;
                case WeaponAttackType.Boomerang:
                    FireBoomerang(state);
                    break;
            }
        }

        private void FireAura(SecondaryWeaponState state)
        {
            float range = state.WeaponData.BaseRange + state.Level * 0.2f;
            int hitCount = Physics2D.OverlapCircle(transform.position, range, targetFilter, targetBuffer);
            int damage = GetDamage(state);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D candidate = targetBuffer[i];
                if (candidate == null || !MatchesTarget(candidate))
                {
                    continue;
                }

                if (candidate.TryGetComponent(out IDamageable damageable))
                {
                    damageable.ApplyDamage(new DamageRequest(damage, gameObject));
                    GameSfx.PlayEnemyHit();
                }
            }

            AuraPulseVisual.Spawn(transform.position, range);
        }

        private void FireBoomerang(SecondaryWeaponState state)
        {
            Transform target = FindNearestTarget(state.WeaponData.BaseRange);
            Vector2 direction = target != null
                ? ((Vector2)target.position - (Vector2)transform.position).normalized
                : Vector2.right;

            GameObject boomerangObject = new GameObject("BoomerangProjectile");
            boomerangObject.transform.position = transform.position;

            Rigidbody2D rb = boomerangObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            CircleCollider2D collider = boomerangObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.18f;

            SpriteRenderer spriteRenderer = boomerangObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = RuntimeSquareSprite.Get();
            spriteRenderer.color = new Color(0.35f, 1f, 0.75f, 1f);
            spriteRenderer.sortingOrder = 5;
            boomerangObject.transform.localScale = new Vector3(0.45f, 0.18f, 1f);

            BoomerangProjectile projectile = boomerangObject.AddComponent<BoomerangProjectile>();
            projectile.Initialize(transform, direction, state.WeaponData.ProjectileSpeed, state.WeaponData.BaseRange, GetDamage(state), targetLayers, targetTag);
        }

        private Transform FindNearestTarget(float range)
        {
            int hitCount = Physics2D.OverlapCircle(transform.position, range, targetFilter, targetBuffer);
            Transform nearestTarget = null;
            float nearestDistanceSqr = float.PositiveInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D candidate = targetBuffer[i];
                if (candidate == null || !MatchesTarget(candidate))
                {
                    continue;
                }

                float distanceSqr = ((Vector2)candidate.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestTarget = candidate.transform;
                }
            }

            return nearestTarget;
        }

        private bool MatchesTarget(Collider2D candidate)
        {
            bool isTargetLayer = (targetLayers.value & (1 << candidate.gameObject.layer)) != 0;
            bool isTargetTag = candidate.GetComponent<EnemyController>() != null
                || string.IsNullOrWhiteSpace(targetTag)
                || candidate.gameObject.tag == targetTag;

            return isTargetLayer && isTargetTag;
        }

        private static int GetDamage(SecondaryWeaponState state)
        {
            return state.WeaponData.BaseDamage + Mathf.Max(0, state.Level - 1) * 2;
        }

        private static float GetCooldown(SecondaryWeaponState state)
        {
            float levelReduction = Mathf.Max(0, state.Level - 1) * 0.05f;
            return state.WeaponData.BaseCooldown * Mathf.Max(0.55f, 1f - levelReduction);
        }

        private struct SecondaryWeaponState
        {
            public SecondaryWeaponState(WeaponData weaponData)
            {
                WeaponData = weaponData;
                Level = 1;
                NextFireTime = 0f;
            }

            public WeaponData WeaponData { get; }
            public int Level { get; set; }
            public float NextFireTime { get; set; }
        }

        private class BoomerangProjectile : MonoBehaviour
        {
            private readonly HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();
            private Transform owner;
            private Vector2 direction;
            private LayerMask targetLayers;
            private string targetTag;
            private float speed;
            private float maxDistance;
            private int damage;
            private Vector2 startPosition;
            private bool returning;

            public void Initialize(Transform ownerTransform, Vector2 fireDirection, float projectileSpeed, float travelDistance, int attackDamage, LayerMask layers, string requiredTag)
            {
                owner = ownerTransform;
                direction = fireDirection.sqrMagnitude > 0.001f ? fireDirection.normalized : Vector2.right;
                speed = Mathf.Max(0.1f, projectileSpeed);
                maxDistance = Mathf.Max(0.5f, travelDistance);
                damage = Mathf.Max(0, attackDamage);
                targetLayers = layers;
                targetTag = requiredTag;
                startPosition = transform.position;
            }

            private void Update()
            {
                Vector2 targetDirection = returning && owner != null
                    ? ((Vector2)owner.position - (Vector2)transform.position).normalized
                    : direction;

                transform.Translate(targetDirection * speed * Time.deltaTime, Space.World);
                transform.Rotate(0f, 0f, 720f * Time.deltaTime);

                if (!returning && Vector2.Distance(startPosition, transform.position) >= maxDistance)
                {
                    returning = true;
                }

                if (returning && (owner == null || Vector2.Distance(transform.position, owner.position) <= 0.3f))
                {
                    Destroy(gameObject);
                }
            }

            private void OnTriggerEnter2D(Collider2D other)
            {
                if (hitTargets.Contains(other) || !IsValidTarget(other))
                {
                    return;
                }

                hitTargets.Add(other);
                if (other.TryGetComponent(out IDamageable damageable))
                {
                    damageable.ApplyDamage(new DamageRequest(damage, gameObject));
                    GameSfx.PlayEnemyHit();
                }
            }

            private bool IsValidTarget(Collider2D other)
            {
                bool isTargetLayer = (targetLayers.value & (1 << other.gameObject.layer)) != 0;
                bool isTargetTag = other.GetComponent<EnemyController>() != null
                    || string.IsNullOrWhiteSpace(targetTag)
                    || other.gameObject.tag == targetTag;

                return isTargetLayer && isTargetTag;
            }
        }

        private class AuraPulseVisual : MonoBehaviour
        {
            private SpriteRenderer spriteRenderer;
            private Color startColor;
            private float elapsed;

            public static void Spawn(Vector3 position, float range)
            {
                GameObject visualObject = new GameObject("AuraPulse");
                visualObject.transform.position = position;
                visualObject.transform.localScale = Vector3.one * range * 2f;

                SpriteRenderer renderer = visualObject.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeCircleSprite.Get();
                renderer.color = new Color(0.1f, 0.85f, 1f, 0.24f);
                renderer.sortingOrder = 4;

                visualObject.AddComponent<AuraPulseVisual>().Initialize(renderer.color);
            }

            private void Initialize(Color color)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                startColor = color;
            }

            private void Update()
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / 0.22f);
                transform.localScale *= 1f + Time.deltaTime * 1.6f;

                if (spriteRenderer != null)
                {
                    Color color = startColor;
                    color.a *= 1f - progress;
                    spriteRenderer.color = color;
                }

                if (progress >= 1f)
                {
                    Destroy(gameObject);
                }
            }
        }

        private static class RuntimeSquareSprite
        {
            private static Sprite sprite;

            public static Sprite Get()
            {
                if (sprite != null)
                {
                    return sprite;
                }

                Texture2D texture = new Texture2D(8, 8);
                Color[] pixels = new Color[64];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = Color.white;
                }

                texture.SetPixels(pixels);
                texture.Apply();
                sprite = Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 8f);
                return sprite;
            }
        }

        private static class RuntimeCircleSprite
        {
            private static Sprite sprite;

            public static Sprite Get()
            {
                if (sprite != null)
                {
                    return sprite;
                }

                Texture2D texture = new Texture2D(32, 32);
                Color[] pixels = new Color[32 * 32];
                Vector2 center = new Vector2(15.5f, 15.5f);
                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        float distance = Vector2.Distance(new Vector2(x, y), center);
                        pixels[y * 32 + x] = distance <= 15.5f ? Color.white : Color.clear;
                    }
                }

                texture.SetPixels(pixels);
                texture.Apply();
                sprite = Sprite.Create(texture, new Rect(0f, 0f, 32f, 32f), new Vector2(0.5f, 0.5f), 16f);
                return sprite;
            }
        }
    }
}
