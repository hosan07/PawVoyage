using UnityEngine;

namespace PawVoyage.Data
{
    /// <summary>
    /// Unity 에셋으로 저장되는 동물 고정 마스터 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "AnimalData", menuName = "Paw Voyage/Animal Data")]
    public class AnimalData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string animalName = string.Empty;
        [SerializeField] private AnimalTier tier = AnimalTier.Basic;
        [SerializeField] private AnimalUnlockType unlockType = AnimalUnlockType.Free;
        [SerializeField] private int unlockCost;
        [SerializeField] private int baseAttack;
        [SerializeField] private int baseHp;
        [SerializeField] private float baseSpeed;
        [SerializeField] private AnimalAttackPattern attackPattern = AnimalAttackPattern.Melee;

        /// <summary>
        /// 저장된 플레이어 데이터에서 사용하는 고정 동물 고유 ID입니다.
        /// </summary>
        public string Id
        {
            get => id;
            set => id = value;
        }

        /// <summary>
        /// 화면에 표시되는 이름입니다.
        /// </summary>
        public string AnimalName
        {
            get => animalName;
            set => animalName = value;
        }

        /// <summary>
        /// 수집 희귀도 등급입니다.
        /// </summary>
        public AnimalTier Tier
        {
            get => tier;
            set => tier = value;
        }

        /// <summary>
        /// 이 동물의 해금 방식입니다.
        /// </summary>
        public AnimalUnlockType UnlockType
        {
            get => unlockType;
            set => unlockType = value;
        }

        /// <summary>
        /// 무료가 아닌 해금에 필요한 재화 수량입니다.
        /// </summary>
        public int UnlockCost
        {
            get => unlockCost;
            set => unlockCost = value;
        }

        /// <summary>
        /// 업그레이드 적용 전 기본 공격력입니다.
        /// </summary>
        public int BaseAttack
        {
            get => baseAttack;
            set => baseAttack = value;
        }

        /// <summary>
        /// 업그레이드 적용 전 기본 최대 체력입니다.
        /// </summary>
        public int BaseHp
        {
            get => baseHp;
            set => baseHp = value;
        }

        /// <summary>
        /// 업그레이드 적용 전 기본 이동 속도입니다.
        /// </summary>
        public float BaseSpeed
        {
            get => baseSpeed;
            set => baseSpeed = value;
        }

        /// <summary>
        /// 이 동물이 기본으로 사용하는 공격 방식입니다.
        /// </summary>
        public AnimalAttackPattern AttackPattern
        {
            get => attackPattern;
            set => attackPattern = value;
        }
    }

    /// <summary>
    /// 동물 희귀도 등급입니다.
    /// </summary>
    public enum AnimalTier
    {
        Basic,
        Common,
        Rare,
        Legendary
    }

    /// <summary>
    /// 동물 해금 방식입니다.
    /// </summary>
    public enum AnimalUnlockType
    {
        Free,
        Gold,
        Gems,
        VipOnly
    }

    /// <summary>
    /// 동물 공격 패턴 분류입니다.
    /// </summary>
    public enum AnimalAttackPattern
    {
        Melee,
        Ranged,
        Aoe
    }
}
