using PawVoyage.Enemy;
using PawVoyage.Combat;
using UnityEngine;

namespace PawVoyage.Systems
{
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
        [SerializeField] private float eliteSpawnTimeSeconds = 18f;
        [SerializeField] private int eliteMaxHp = 260;
        [SerializeField] private int eliteContactDamage = 5;
        [SerializeField] private float eliteMoveSpeed = 1.35f;
        [SerializeField] private int eliteExperienceAmount = 8;
        [SerializeField] private int eliteHealthPickupHealAmount = 35;
        [SerializeField] private float eliteScale = 1.45f;
        [SerializeField] private Color eliteColor = new Color(0.65f, 0.2f, 1f, 1f);
        [SerializeField] private float eliteWarningDuration = 2.25f;
        [SerializeField] private bool spawnOnStart = true;

        private float nextSpawnTime;
        private RunStats runStats;
        private bool eliteSpawned;
        private float eliteWarningEndTime;
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
            SpawnEnemy();
            nextSpawnTime = Time.time + GetCurrentSpawnInterval();
        }

        private void OnGUI()
        {
            if (Time.unscaledTime >= eliteWarningEndTime)
            {
                return;
            }

            EnsureEliteWarningStyle();
            GUI.Label(new Rect(0f, Screen.height * 0.2f, Screen.width, 42f), "ELITE INCOMING", eliteWarningStyle);
        }

        private void SpawnEnemy()
        {
            Vector2 spawnPosition = GetSpawnPosition();
            EnemyController enemy = enemyPrefab != null
                ? Instantiate(enemyPrefab, spawnPosition, Quaternion.identity)
                : CreateFallbackEnemy(spawnPosition);

            ConfigureEnemyStats(enemy);
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
            if (enemy.TryGetComponent(out Health health))
            {
                health.SetBaseMaxHp(GetCurrentEnemyMaxHp(), true);
            }

            if (enemy.TryGetComponent(out ContactDamage contactDamage))
            {
                contactDamage.SetDamage(GetCurrentEnemyContactDamage());
            }

            enemy.SetMoveSpeed(enemyMoveSpeed);

            if (!enemy.TryGetComponent<EnemyHitFeedback>(out _))
            {
                enemy.gameObject.AddComponent<EnemyHitFeedback>();
            }

            if (!enemy.TryGetComponent<EnemyDeathFeedback>(out _))
            {
                enemy.gameObject.AddComponent<EnemyDeathFeedback>();
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
                enemyReward.SetHealthPickupDropChance(1f);
                enemyReward.SetHealthPickupHealAmount(eliteHealthPickupHealAmount);
            }

            if (enemy.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = eliteColor;
            }
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
    }
}
