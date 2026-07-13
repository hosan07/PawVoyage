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
        [SerializeField] private bool spawnOnStart = true;

        private float nextSpawnTime;
        private RunStats runStats;

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
                return;
            }

            SpawnEnemy();
            nextSpawnTime = Time.time + GetCurrentSpawnInterval();
        }

        private void SpawnEnemy()
        {
            Vector2 spawnPosition = GetSpawnPosition();
            EnemyController enemy = enemyPrefab != null
                ? Instantiate(enemyPrefab, spawnPosition, Quaternion.identity)
                : CreateFallbackEnemy(spawnPosition);

            enemy.Target = player;
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
            enemyObject.AddComponent<ContactDamage>();
            enemyObject.AddComponent<EnemyReward>();
            return enemyObject.AddComponent<EnemyController>();
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

        private float GetCurrentSpawnInterval()
        {
            return Mathf.Lerp(spawnInterval, minimumSpawnInterval, GetRunProgress());
        }

        private int GetCurrentMaxAliveEnemies()
        {
            int scaledMax = Mathf.RoundToInt(Mathf.Lerp(maxAliveEnemies, finalMaxAliveEnemies, GetRunProgress()));
            return Mathf.Max(1, scaledMax);
        }

        private void OnDrawGizmosSelected()
        {
            Transform center = player != null ? player : transform;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center.position, spawnRadius);
        }
    }
}
