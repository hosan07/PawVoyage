using PawVoyage.Enemy;
using PawVoyage.Combat;
using UnityEngine;

namespace PawVoyage.Systems
{
    internal enum EnemyVariantType
    {
        Normal,
        Fast,
        Tank
    }

    /// <summary>
    /// 일정 간격으로 플레이어 주변에 적을 생성합니다.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyPrefab = null;
        [SerializeField] private Transform player;
        [SerializeField] private string playerName = "Player";
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float spawnRadius = 7f;
        [SerializeField] private int maxAliveEnemies = 20;
        [SerializeField] private float minimumSpawnInterval = 0.65f;
        [SerializeField] private int finalMaxAliveEnemies = 36;
        [SerializeField] private int enemyBaseMaxHp = 30;
        [SerializeField] private int enemyFinalMaxHp = 70;
        [SerializeField] private int enemyBaseContactDamage = 1;
        [SerializeField] private int enemyFinalContactDamage = 3;
        [SerializeField] private float enemyMoveSpeed = 2f;
        [SerializeField] private float fastEnemyStartTimeSeconds = 8f;
        [SerializeField] private float tankEnemyStartTimeSeconds = 16f;
        [SerializeField] private float eliteSpawnTimeSeconds = 18f;
        [SerializeField] private int eliteMaxHp = 260;
        [SerializeField] private int eliteContactDamage = 5;
        [SerializeField] private float eliteMoveSpeed = 1.35f;
        [SerializeField] private int eliteExperienceAmount = 8;
        [SerializeField] private int eliteCoinAmount = 12;
        [SerializeField] private int eliteHealthPickupHealAmount = 35;
        [SerializeField] private float eliteScale = 1.45f;
        [SerializeField] private Color eliteColor = new Color(0.65f, 0.2f, 1f, 1f);
        [SerializeField] private float eliteWarningDuration = 2.25f;
        [SerializeField] private bool spawnOnStart = true;

        private float nextSpawnTime;
        private RunStats runStats;
        private bool eliteSpawned;
        private bool fastEnemyNoticeShown;
        private bool tankEnemyNoticeShown;
        private float eliteWarningEndTime;
        private float variantNoticeEndTime;
        private string variantNoticeText = string.Empty;
        private GUIStyle eliteWarningStyle;

        private void Start()
        {
            FindPlayerIfNeeded();
            runStats = RunStats.Instance;
            nextSpawnTime = spawnOnStart ? Time.time : Time.time + spawnInterval;
        }

        private void Update()
        {
            FindPlayerIfNeeded();

            if (player == null || Time.time < nextSpawnTime || CountAliveEnemies() >= GetCurrentMaxAliveEnemies())
            {
                TrySpawnElite();
                return;
            }

            TrySpawnElite();
            TryShowVariantNotice();
            SpawnEnemy();
            nextSpawnTime = Time.time + GetCurrentSpawnInterval();
        }

        private void OnGUI()
        {
            EnsureEliteWarningStyle();

            if (Time.unscaledTime < eliteWarningEndTime)
            {
                GUI.Label(new Rect(0f, Screen.height * 0.2f, Screen.width, 42f), "ELITE INCOMING", eliteWarningStyle);
            }

            if (Time.unscaledTime < variantNoticeEndTime)
            {
                GUI.Label(new Rect(0f, Screen.height * 0.26f, Screen.width, 42f), variantNoticeText, eliteWarningStyle);
            }
        }

        private void SpawnEnemy()
        {
            Vector2 spawnPosition = GetSpawnPosition();
            EnemyController enemy = enemyPrefab != null
                ? Instantiate(enemyPrefab, spawnPosition, Quaternion.identity)
                : CreateFallbackEnemy(spawnPosition);

            ConfigureEnemyStats(enemy, ChooseEnemyVariant());
            enemy.Target = player;
        }

        private void TrySpawnElite()
        {
            if (eliteSpawned || player == null || GetElapsedSeconds() < eliteSpawnTimeSeconds)
            {
                return;
            }

            eliteSpawned = true;
            eliteWarningEndTime = Time.unscaledTime + Mathf.Max(0f, eliteWarningDuration);
            Vector2 spawnPosition = GetSpawnPosition();
            EnemyController elite = enemyPrefab != null
                ? Instantiate(enemyPrefab, spawnPosition, Quaternion.identity)
                : CreateFallbackEnemy(spawnPosition);

            ConfigureEliteStats(elite);
            elite.Target = player;
        }

        private void TryShowVariantNotice()
        {
            float elapsedSeconds = GetElapsedSeconds();
            if (!fastEnemyNoticeShown && elapsedSeconds >= fastEnemyStartTimeSeconds)
            {
                fastEnemyNoticeShown = true;
                ShowVariantNotice("FAST ENEMIES JOINED");
            }

            if (!tankEnemyNoticeShown && elapsedSeconds >= tankEnemyStartTimeSeconds)
            {
                tankEnemyNoticeShown = true;
                ShowVariantNotice("TANK ENEMIES JOINED");
            }
        }

        private void ShowVariantNotice(string message)
        {
            variantNoticeText = message;
            variantNoticeEndTime = Time.unscaledTime + Mathf.Max(0f, eliteWarningDuration);
        }

        private Vector2 GetSpawnPosition()
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector2.right;
            }

            return (Vector2)player.position + direction * spawnRadius;
        }

        private EnemyController CreateFallbackEnemy(Vector2 spawnPosition)
        {
            GameObject enemyObject = new GameObject("Enemy");
            enemyObject.transform.position = spawnPosition;
            enemyObject.transform.localScale = Vector3.one * 0.75f;
            enemyObject.AddComponent<Health>();
            enemyObject.AddComponent<ContactDamage>();
            enemyObject.AddComponent<EnemyReward>();
            enemyObject.AddComponent<EnemyHitFeedback>();
            enemyObject.AddComponent<EnemyDeathFeedback>();
            return enemyObject.AddComponent<EnemyController>();
        }

        private void ConfigureEnemyStats(EnemyController enemy)
        {
            ConfigureEnemyStats(enemy, EnemyVariantType.Normal);
        }

        private void ConfigureEnemyStats(EnemyController enemy, EnemyVariantType variantType)
        {
            EnemyVariantTuning tuning = GetEnemyVariantTuning(variantType);

            if (enemy.TryGetComponent(out Health health))
            {
                int maxHp = Mathf.RoundToInt(GetCurrentEnemyMaxHp() * tuning.MaxHpMultiplier);
                health.SetBaseMaxHp(maxHp, true);
            }

            if (enemy.TryGetComponent(out ContactDamage contactDamage))
            {
                contactDamage.SetDamage(GetCurrentEnemyContactDamage() + tuning.ContactDamageBonus);
            }

            enemy.gameObject.name = tuning.Name;
            enemy.transform.localScale = Vector3.one * tuning.Scale;
            enemy.SetMoveSpeed(enemyMoveSpeed * tuning.MoveSpeedMultiplier);

            if (!enemy.TryGetComponent<EnemyHitFeedback>(out _))
            {
                enemy.gameObject.AddComponent<EnemyHitFeedback>();
            }

            if (!enemy.TryGetComponent<EnemyDeathFeedback>(out _))
            {
                enemy.gameObject.AddComponent<EnemyDeathFeedback>();
            }

            if (enemy.TryGetComponent(out EnemyReward enemyReward))
            {
                enemyReward.SetExperienceAmount(tuning.ExperienceAmount);
                enemyReward.SetCoinAmount(tuning.CoinAmount);
                enemyReward.SetHealthPickupDropChance(tuning.HealthPickupDropChance);
            }

            if (enemy.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = tuning.Color;
            }
        }

        private void ConfigureEliteStats(EnemyController enemy)
        {
            ConfigureEnemyStats(enemy);

            enemy.gameObject.name = "EliteEnemy";
            enemy.transform.localScale = Vector3.one * Mathf.Max(0.1f, eliteScale);
            enemy.SetMoveSpeed(eliteMoveSpeed);

            if (enemy.TryGetComponent(out Health health))
            {
                health.SetBaseMaxHp(eliteMaxHp, true);
            }

            if (enemy.TryGetComponent(out ContactDamage contactDamage))
            {
                contactDamage.SetDamage(eliteContactDamage);
            }

            if (enemy.TryGetComponent(out EnemyReward enemyReward))
            {
                enemyReward.SetExperienceAmount(eliteExperienceAmount);
                enemyReward.SetCoinAmount(eliteCoinAmount);
                enemyReward.SetHealthPickupDropChance(1f);
                enemyReward.SetHealthPickupHealAmount(eliteHealthPickupHealAmount);
            }

            if (enemy.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = eliteColor;
            }
        }

        private EnemyVariantType ChooseEnemyVariant()
        {
            float elapsedSeconds = GetElapsedSeconds();
            bool canSpawnTank = elapsedSeconds >= tankEnemyStartTimeSeconds;
            bool canSpawnFast = elapsedSeconds >= fastEnemyStartTimeSeconds;

            if (!canSpawnFast)
            {
                return EnemyVariantType.Normal;
            }

            int normalWeight = 100;
            int fastWeight = canSpawnFast ? Mathf.RoundToInt(Mathf.Lerp(20f, 42f, GetRunProgress())) : 0;
            int tankWeight = canSpawnTank ? Mathf.RoundToInt(Mathf.Lerp(12f, 32f, GetRunProgress())) : 0;
            int totalWeight = normalWeight + fastWeight + tankWeight;
            int roll = Random.Range(0, totalWeight);

            if (roll < tankWeight)
            {
                return EnemyVariantType.Tank;
            }

            if (roll < tankWeight + fastWeight)
            {
                return EnemyVariantType.Fast;
            }

            return EnemyVariantType.Normal;
        }

        private static EnemyVariantTuning GetEnemyVariantTuning(EnemyVariantType variantType)
        {
            return variantType switch
            {
                EnemyVariantType.Fast => new EnemyVariantTuning(
                    "FastEnemy",
                    maxHpMultiplier: 0.7f,
                    moveSpeedMultiplier: 1.65f,
                    contactDamageBonus: 0,
                    experienceAmount: 1,
                    coinAmount: 1,
                    healthPickupDropChance: 0.04f,
                    scale: 0.58f,
                    color: new Color(1f, 0.58f, 0.1f, 1f)),
                EnemyVariantType.Tank => new EnemyVariantTuning(
                    "TankEnemy",
                    maxHpMultiplier: 1.85f,
                    moveSpeedMultiplier: 0.72f,
                    contactDamageBonus: 1,
                    experienceAmount: 2,
                    coinAmount: 3,
                    healthPickupDropChance: 0.12f,
                    scale: 1.05f,
                    color: new Color(0.95f, 0.12f, 0.35f, 1f)),
                _ => new EnemyVariantTuning(
                    "Enemy",
                    maxHpMultiplier: 1f,
                    moveSpeedMultiplier: 1f,
                    contactDamageBonus: 0,
                    experienceAmount: 1,
                    coinAmount: 1,
                    healthPickupDropChance: 0.08f,
                    scale: 0.75f,
                    color: Color.red)
            };
        }

        private void FindPlayerIfNeeded()
        {
            if (player != null)
            {
                return;
            }

            GameObject playerObject = GameObject.Find(playerName);
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        private static int CountAliveEnemies()
        {
            return Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length;
        }

        private float GetRunProgress()
        {
            if (runStats == null)
            {
                runStats = RunStats.Instance;
            }

            return runStats == null ? 0f : Mathf.Clamp01(runStats.ElapsedSeconds / runStats.ClearTimeSeconds);
        }

        private float GetElapsedSeconds()
        {
            if (runStats == null)
            {
                runStats = RunStats.Instance;
            }

            return runStats == null ? 0f : runStats.ElapsedSeconds;
        }

        private float GetCurrentSpawnInterval()
        {
            return Mathf.Lerp(spawnInterval, minimumSpawnInterval, GetRunProgress());
        }

        private int GetCurrentMaxAliveEnemies()
        {
            int scaledMax = Mathf.RoundToInt(Mathf.Lerp(maxAliveEnemies, finalMaxAliveEnemies, GetRunProgress()));
            return Mathf.Max(1, scaledMax);
        }

        private int GetCurrentEnemyMaxHp()
        {
            int scaledMaxHp = Mathf.RoundToInt(Mathf.Lerp(enemyBaseMaxHp, enemyFinalMaxHp, GetRunProgress()));
            return Mathf.Max(1, scaledMaxHp);
        }

        private int GetCurrentEnemyContactDamage()
        {
            int scaledDamage = Mathf.RoundToInt(Mathf.Lerp(enemyBaseContactDamage, enemyFinalContactDamage, GetRunProgress()));
            return Mathf.Max(0, scaledDamage);
        }

        private void OnDrawGizmosSelected()
        {
            Transform center = player != null ? player : transform;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center.position, spawnRadius);
        }

        private void EnsureEliteWarningStyle()
        {
            if (eliteWarningStyle != null)
            {
                return;
            }

            eliteWarningStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.35f, 0.2f, 1f) }
            };
        }

        private readonly struct EnemyVariantTuning
        {
            public EnemyVariantTuning(
                string name,
                float maxHpMultiplier,
                float moveSpeedMultiplier,
                int contactDamageBonus,
                int experienceAmount,
                int coinAmount,
                float healthPickupDropChance,
                float scale,
                Color color)
            {
                Name = name;
                MaxHpMultiplier = maxHpMultiplier;
                MoveSpeedMultiplier = moveSpeedMultiplier;
                ContactDamageBonus = contactDamageBonus;
                ExperienceAmount = experienceAmount;
                CoinAmount = coinAmount;
                HealthPickupDropChance = healthPickupDropChance;
                Scale = Mathf.Max(0.1f, scale);
                Color = color;
            }

            public string Name { get; }
            public float MaxHpMultiplier { get; }
            public float MoveSpeedMultiplier { get; }
            public int ContactDamageBonus { get; }
            public int ExperienceAmount { get; }
            public int CoinAmount { get; }
            public float HealthPickupDropChance { get; }
            public float Scale { get; }
            public Color Color { get; }
        }
    }
}
