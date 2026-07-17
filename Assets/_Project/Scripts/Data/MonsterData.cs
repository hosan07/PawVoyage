using UnityEngine;

namespace PawVoyage.Data
{
    public enum MonsterRole
    {
        Normal,
        Fast,
        Tank,
        Elite
    }

    public enum MonsterBehaviorType
    {
        Chase,
        Zigzag,
        Charger,
        Shooter,
        EliteBrute
    }

    /// <summary>
    /// 몬스터 종류별 전투 수치와 보상을 담는 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterData", menuName = "Paw Voyage/Data/Monster Data")]
    public class MonsterData : ScriptableObject
    {
        [SerializeField] private string monsterId = "monster_id";
        [SerializeField] private string displayName = "Monster";
        [SerializeField] private MonsterRole role = MonsterRole.Normal;
        [SerializeField] private int maxHp = 30;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private MonsterBehaviorType behaviorType = MonsterBehaviorType.Chase;
        [SerializeField] private float sizeScale = 0.75f;
        [SerializeField] private int expReward = 1;
        [SerializeField] private int coinReward = 1;
        [SerializeField] private float healthPickupDropChance = 0.08f;
        [SerializeField] private int healthPickupHealAmount = 15;
        [SerializeField] private Color visualHint = Color.red;

        public string MonsterId => monsterId;
        public string DisplayName => displayName;
        public MonsterRole Role => role;
        public int MaxHp => Mathf.Max(1, maxHp);
        public int ContactDamage => Mathf.Max(0, contactDamage);
        public float MoveSpeed => Mathf.Max(0f, moveSpeed);
        public MonsterBehaviorType BehaviorType => behaviorType;
        public float SizeScale => Mathf.Max(0.1f, sizeScale);
        public int ExpReward => Mathf.Max(0, expReward);
        public int CoinReward => Mathf.Max(0, coinReward);
        public float HealthPickupDropChance => Mathf.Clamp01(healthPickupDropChance);
        public int HealthPickupHealAmount => Mathf.Max(1, healthPickupHealAmount);
        public Color VisualHint => visualHint;
    }
}
