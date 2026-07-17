using PawVoyage.Enemy;
using PawVoyage.Combat;
using PawVoyage.Data;
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
        private const float HpScaleStart = 0.75f;
        private const float HpScaleMid = 1.55f;
        private const float HpScaleEnd = 2.45f;
        private const float DamageScaleStart = 1f;
        private const float DamageScaleMid = 1.65f;
        private const float DamageScaleEnd = 2.4f;
        private const float SpeedScaleStart = 0.92f;
        private const float SpeedScaleMid = 1.18f;
        private const float SpeedScaleEnd = 1.38f;

        [SerializeField] private EnemyController enemyPrefab = null;
        [SerializeField] private Transform player;
        [SerializeField] private string playerName = "Player";
        [SerializeField] private StageData stageData = null;
        [SerializeField] private StageRuntimeMode stageMode = StageRuntimeMode.Mvp;
        [SerializeField] private MonsterData normalMonsterData = null;
        [SerializeField] private MonsterData fastMonsterData = null;
        [SerializeField] private MonsterData tankMonsterData = null;
        [SerializeField] private MonsterData eliteMonsterData = null;
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
        [SerializeField] private int eliteDefeatBonusCoins = 15;
        [SerializeField] private int eliteHealthPickupHealAmount = 35;
        [SerializeField] private float eliteScale = 1.45f;
        [SerializeField] private Color eliteColor = new Color(0.65f, 0.2f, 1f, 1f);
        [SerializeField] private float eliteWarningDuration = 2.25f;
        [SerializeField] private string eliteDisplayName = "MINI BOSS";
        [SerializeField] private bool spawnOnStart = true;

        private float nextSpawnTime;
        private RunStats runStats;
        private Health activeEliteHealth;
        private bool eliteSpawned;
        private bool fastEnemyNoticeShown;
        private bool tankEnemyNoticeShown;
        private float eliteWarningEndTime;
        private float variantNoticeEndTime;
        private string variantNoticeText = string.Empty;
        private GUIStyle eliteWarningStyle;
        private GUIStyle eliteHealthLabelStyle;
        private Texture2D whiteTexture;

        public static WaveSpawner Instance { get; private set; }

        public string CurrentPhaseName
        {
            get
            {
                float elapsedSeconds = GetElapsedSeconds();
                if (elapsedSeconds >= GetEliteSpawnTime())
                {
                    return "ELITE";
                }

                if (elapsedSeconds >= GetTankEnemyStartTime())
                {
                    return "TANK";
                }

                if (elapsedSeconds >= GetFastEnemyStartTime())
                {
                    return "FAST";
                }

                return "NORMAL";
            }
        }

        public int CurrentAliveEnemies => CountAliveEnemies();
        public int CurrentMaxAliveEnemies => GetCurrentMaxAliveEnemies();
        public float CurrentSpawnInterval => GetCurrentSpawnInterval();
        public bool IsEliteActive => activeEliteHealth != null && activeEliteHealth.CurrentHp > 0;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (activeEliteHealth != null)
            {
                activeEliteHealth.Died -= OnEliteDied;
            }
        }

        private void Start()
        {
            FindPlayerIfNeeded();
            runStats = RunStats.Instance;
            ConfigureRunStats();
            nextSpawnTime = spawnOnStart ? Time.time : Time.time + GetCurrentSpawnInterval();
        }

        private void Update()
        {
            FindPlayerIfNeeded();

            if (player == null || Time.time < nextSpawnTime || CountAliveEnemies() >= GetCurrentMaxAliveEnemies())
            {
                TrySpawnElite();
                TryShowVariantNotice();
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

            DrawEliteHealthBar();
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
            if (eliteSpawned || player == null || GetElapsedSeconds() < GetEliteSpawnTime())
            {
                return;
            }

            eliteSpawned = true;
            runStats ??= RunStats.Instance;
            runStats?.RegisterMiniBossSeen();
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
            if (!fastEnemyNoticeShown && elapsedSeconds >= GetFastEnemyStartTime())
            {
                fastEnemyNoticeShown = true;
                ShowVariantNotice("FAST ENEMIES JOINED");
            }

            if (!tankEnemyNoticeShown && elapsedSeconds >= GetTankEnemyStartTime())
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
            MonsterData monsterData = GetMonsterData(variantType);
            if (monsterData != null)
            {
                ConfigureEnemyStats(enemy, monsterData);
                return;
            }

            EnemyVariantTuning tuning = GetEnemyVariantTuning(variantType);

            if (enemy.TryGetComponent(out Health health))
            {
                int maxHp = Mathf.RoundToInt(GetCurrentEnemyMaxHp() * tuning.MaxHpMultiplier);
                health.SetBaseMaxHp(maxHp, true);
            }

            if (enemy.TryGetComponent(out ContactDamage contactDamage))
            {
                contactDamage.SetDamage(GetCurrentEnemyContactDamage() + tuning.ContactDamageBonus);
                contactDamage.SetHitCooldown(GetCurrentContactHitCooldown());
            }

            enemy.gameObject.name = tuning.Name;
            enemy.transform.localScale = Vector3.one * tuning.Scale;
            enemy.SetMoveSpeed(enemyMoveSpeed * tuning.MoveSpeedMultiplier);
            enemy.SetBehavior(tuning.BehaviorType);

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
            MonsterData monsterData = eliteMonsterData;
            if (monsterData != null)
            {
                ConfigureEnemyStats(enemy, monsterData);
            }
            else
            {
                ConfigureEnemyStats(enemy);
            }

            enemy.gameObject.name = "EliteEnemy";
            enemy.transform.localScale = Vector3.one * Mathf.Max(0.1f, monsterData != null ? monsterData.SizeScale : eliteScale);
            enemy.SetMoveSpeed(GetScaledMoveSpeed(monsterData != null ? monsterData.MoveSpeed : eliteMoveSpeed) * 1.08f);
            enemy.SetBehavior(MonsterBehaviorType.EliteBrute);

            if (enemy.TryGetComponent(out Health health))
            {
                int baseMaxHp = monsterData != null ? monsterData.MaxHp : eliteMaxHp;
                health.SetBaseMaxHp(Mathf.RoundToInt(baseMaxHp * 1.35f), true);
                TrackEliteHealth(health);
            }

            if (enemy.TryGetComponent(out ContactDamage contactDamage))
            {
                int baseDamage = monsterData != null ? monsterData.ContactDamage : eliteContactDamage;
                contactDamage.SetDamage(Mathf.Max(baseDamage + 2, Mathf.RoundToInt(baseDamage * 1.4f)));
                contactDamage.SetHitCooldown(0.28f);
            }

            if (enemy.TryGetComponent(out EnemyReward enemyReward))
            {
                enemyReward.SetExperienceAmount(monsterData != null ? monsterData.ExpReward : eliteExperienceAmount);
                enemyReward.SetCoinAmount(monsterData != null ? monsterData.CoinReward : eliteCoinAmount);
                enemyReward.SetHealthPickupDropChance(monsterData != null ? monsterData.HealthPickupDropChance : 1f);
                enemyReward.SetHealthPickupHealAmount(monsterData != null ? monsterData.HealthPickupHealAmount : eliteHealthPickupHealAmount);
            }

            if (enemy.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = monsterData != null ? monsterData.VisualHint : eliteColor;
            }
        }

        private void ConfigureEnemyStats(EnemyController enemy, MonsterData monsterData)
        {
            if (enemy.TryGetComponent(out Health health))
            {
                health.SetBaseMaxHp(GetScaledMaxHp(monsterData.MaxHp), true);
            }

            if (enemy.TryGetComponent(out ContactDamage contactDamage))
            {
                contactDamage.SetDamage(GetScaledContactDamage(monsterData.ContactDamage));
                contactDamage.SetHitCooldown(GetCurrentContactHitCooldown());
            }

            enemy.gameObject.name = monsterData.DisplayName;
            enemy.transform.localScale = Vector3.one * monsterData.SizeScale;
            enemy.SetMoveSpeed(GetScaledMoveSpeed(monsterData.MoveSpeed));
            enemy.SetBehavior(monsterData.BehaviorType);

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
                enemyReward.SetExperienceAmount(monsterData.ExpReward);
                enemyReward.SetCoinAmount(monsterData.CoinReward);
                enemyReward.SetHealthPickupDropChance(monsterData.HealthPickupDropChance);
                enemyReward.SetHealthPickupHealAmount(monsterData.HealthPickupHealAmount);
            }

            if (enemy.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = monsterData.VisualHint;
            }
        }

        private void TrackEliteHealth(Health health)
        {
            if (activeEliteHealth != null)
            {
                activeEliteHealth.Died -= OnEliteDied;
            }

            activeEliteHealth = health;
            activeEliteHealth.Died += OnEliteDied;
        }

        private void OnEliteDied(Health deadHealth)
        {
            if (deadHealth != activeEliteHealth)
            {
                return;
            }

            activeEliteHealth.Died -= OnEliteDied;
            activeEliteHealth = null;

            int bonusCoins = Mathf.Max(0, eliteDefeatBonusCoins);
            if (bonusCoins > 0)
            {
                runStats ??= RunStats.Instance;
                runStats?.AddBonusCoins(bonusCoins);
                GameSfx.PlayCoin();
                ShowVariantNotice($"ELITE DEFEATED  +{bonusCoins} COINS");
            }
            else
            {
                ShowVariantNotice("ELITE DEFEATED");
            }

            if (GetClearCondition() == StageClearCondition.MiniBossDefeat)
            {
                runStats ??= RunStats.Instance;
                runStats?.CompleteRun();
            }
        }

        private EnemyVariantType ChooseEnemyVariant()
        {
            float elapsedSeconds = GetElapsedSeconds();
            bool canSpawnTank = elapsedSeconds >= GetTankEnemyStartTime();
            bool canSpawnFast = elapsedSeconds >= GetFastEnemyStartTime();

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
                    color: new Color(1f, 0.58f, 0.1f, 1f),
                    behaviorType: MonsterBehaviorType.Zigzag),
                EnemyVariantType.Tank => new EnemyVariantTuning(
                    "TankEnemy",
                    maxHpMultiplier: 1.85f,
                    moveSpeedMultiplier: 0.72f,
                    contactDamageBonus: 1,
                    experienceAmount: 2,
                    coinAmount: 3,
                    healthPickupDropChance: 0.12f,
                    scale: 1.05f,
                    color: new Color(0.95f, 0.12f, 0.35f, 1f),
                    behaviorType: MonsterBehaviorType.Charger),
                _ => new EnemyVariantTuning(
                    "Enemy",
                    maxHpMultiplier: 1f,
                    moveSpeedMultiplier: 1f,
                    contactDamageBonus: 0,
                    experienceAmount: 1,
                    coinAmount: 1,
                    healthPickupDropChance: 0.08f,
                    scale: 0.75f,
                    color: Color.red,
                    behaviorType: MonsterBehaviorType.Chase)
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

            return runStats == null ? 0f : Mathf.Clamp01(runStats.ElapsedSeconds / Mathf.Max(1f, GetClearTimeSeconds()));
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
            StageModeConfig config = GetStageConfig();
            return Mathf.Lerp(config.spawnIntervalStart, config.spawnIntervalEnd, GetRunProgress());
        }

        private int GetCurrentMaxAliveEnemies()
        {
            StageModeConfig config = GetStageConfig();
            int scaledMax = Mathf.RoundToInt(Mathf.Lerp(config.enemyCapStart, config.enemyCapEnd, GetRunProgress()));
            return Mathf.Max(1, scaledMax);
        }

        private void ConfigureRunStats()
        {
            if (runStats == null)
            {
                return;
            }

            string currentStageId = stageData == null ? string.Empty : stageData.StageId;
            runStats.ConfigureStage(currentStageId, stageMode, GetClearTimeSeconds(), GetClearCondition());
        }

        private StageModeConfig GetStageConfig()
        {
            if (stageData != null)
            {
                return stageData.GetConfig(stageMode);
            }

            return new StageModeConfig
            {
                clearTimeSeconds = 30f,
                fastEnemyStartTime = fastEnemyStartTimeSeconds,
                tankEnemyStartTime = tankEnemyStartTimeSeconds,
                eliteSpawnTime = eliteSpawnTimeSeconds,
                spawnIntervalStart = spawnInterval,
                spawnIntervalEnd = minimumSpawnInterval,
                enemyCapStart = maxAliveEnemies,
                enemyCapEnd = finalMaxAliveEnemies,
                clearCondition = StageClearCondition.SurviveTime
            };
        }

        private float GetClearTimeSeconds()
        {
            return Mathf.Max(1f, GetStageConfig().clearTimeSeconds);
        }

        private StageClearCondition GetClearCondition()
        {
            return GetStageConfig().clearCondition;
        }

        private float GetFastEnemyStartTime()
        {
            return Mathf.Max(0f, GetStageConfig().fastEnemyStartTime);
        }

        private float GetTankEnemyStartTime()
        {
            return Mathf.Max(0f, GetStageConfig().tankEnemyStartTime);
        }

        private float GetEliteSpawnTime()
        {
            return Mathf.Max(0f, GetStageConfig().eliteSpawnTime);
        }

        private MonsterData GetMonsterData(EnemyVariantType variantType)
        {
            return variantType switch
            {
                EnemyVariantType.Fast => fastMonsterData,
                EnemyVariantType.Tank => tankMonsterData,
                _ => normalMonsterData
            };
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

        private int GetScaledMaxHp(int baseMaxHp)
        {
            float progress = GetRunProgress();
            float scale = progress < 0.5f
                ? Mathf.Lerp(HpScaleStart, HpScaleMid, progress / 0.5f)
                : Mathf.Lerp(HpScaleMid, HpScaleEnd, (progress - 0.5f) / 0.5f);
            return Mathf.Max(1, Mathf.RoundToInt(baseMaxHp * scale));
        }

        private int GetScaledContactDamage(int baseContactDamage)
        {
            float progress = GetRunProgress();
            float scale = progress < 0.5f
                ? Mathf.Lerp(DamageScaleStart, DamageScaleMid, progress / 0.5f)
                : Mathf.Lerp(DamageScaleMid, DamageScaleEnd, (progress - 0.5f) / 0.5f);
            return Mathf.Max(1, Mathf.RoundToInt(baseContactDamage * scale));
        }

        private float GetScaledMoveSpeed(float baseMoveSpeed)
        {
            float progress = GetRunProgress();
            float scale = progress < 0.5f
                ? Mathf.Lerp(SpeedScaleStart, SpeedScaleMid, progress / 0.5f)
                : Mathf.Lerp(SpeedScaleMid, SpeedScaleEnd, (progress - 0.5f) / 0.5f);
            return Mathf.Max(0.1f, baseMoveSpeed * scale);
        }

        private float GetCurrentContactHitCooldown()
        {
            return Mathf.Lerp(0.46f, 0.28f, GetRunProgress());
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

            eliteHealthLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }

        private void DrawEliteHealthBar()
        {
            if (!IsEliteActive || whiteTexture == null)
            {
                return;
            }

            float width = Mathf.Min(520f, Screen.width - 56f);
            Rect frameRect = new Rect(Screen.width * 0.5f - width * 0.5f, 62f, width, 26f);
            Rect fillRect = new Rect(frameRect.x + 3f, frameRect.y + 3f, (frameRect.width - 6f) * GetEliteHealthRatio(), frameRect.height - 6f);

            DrawGuiRect(new Rect(frameRect.x - 2f, frameRect.y - 2f, frameRect.width + 4f, frameRect.height + 4f), Color.black);
            DrawGuiRect(frameRect, new Color(0.13f, 0.05f, 0.18f, 0.92f));
            DrawGuiRect(fillRect, new Color(0.77f, 0.16f, 1f, 0.95f));

            GUI.Label(
                frameRect,
                $"{eliteDisplayName}  {activeEliteHealth.CurrentHp}/{activeEliteHealth.MaxHp}",
                eliteHealthLabelStyle);
        }

        private float GetEliteHealthRatio()
        {
            if (activeEliteHealth == null || activeEliteHealth.MaxHp <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp01((float)activeEliteHealth.CurrentHp / activeEliteHealth.MaxHp);
        }

        private void DrawGuiRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previousColor;
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
                Color color,
                MonsterBehaviorType behaviorType)
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
                BehaviorType = behaviorType;
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
            public MonsterBehaviorType BehaviorType { get; }
        }
    }
}
