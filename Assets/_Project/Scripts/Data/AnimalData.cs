using UnityEngine;

namespace PawVoyage.Data
{
    /// <summary>
    /// Static animal master data stored as a Unity asset.
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
        /// Stable unique animal id used by saved player data.
        /// </summary>
        public string Id
        {
            get => id;
            set => id = value;
        }

        /// <summary>
        /// Display name shown in UI.
        /// </summary>
        public string AnimalName
        {
            get => animalName;
            set => animalName = value;
        }

        /// <summary>
        /// Collection rarity tier.
        /// </summary>
        public AnimalTier Tier
        {
            get => tier;
            set => tier = value;
        }

        /// <summary>
        /// Unlock method for this animal.
        /// </summary>
        public AnimalUnlockType UnlockType
        {
            get => unlockType;
            set => unlockType = value;
        }

        /// <summary>
        /// Required currency amount for non-free unlocks.
        /// </summary>
        public int UnlockCost
        {
            get => unlockCost;
            set => unlockCost = value;
        }

        /// <summary>
        /// Base attack power before upgrades.
        /// </summary>
        public int BaseAttack
        {
            get => baseAttack;
            set => baseAttack = value;
        }

        /// <summary>
        /// Base maximum health before upgrades.
        /// </summary>
        public int BaseHp
        {
            get => baseHp;
            set => baseHp = value;
        }

        /// <summary>
        /// Base movement speed before upgrades.
        /// </summary>
        public float BaseSpeed
        {
            get => baseSpeed;
            set => baseSpeed = value;
        }

        /// <summary>
        /// Default attack behavior used by this animal.
        /// </summary>
        public AnimalAttackPattern AttackPattern
        {
            get => attackPattern;
            set => attackPattern = value;
        }
    }

    /// <summary>
    /// Animal rarity tiers.
    /// </summary>
    public enum AnimalTier
    {
        Basic,
        Common,
        Rare,
        Legendary
    }

    /// <summary>
    /// Animal unlock methods.
    /// </summary>
    public enum AnimalUnlockType
    {
        Free,
        Gold,
        Gems,
        VipOnly
    }

    /// <summary>
    /// Animal attack pattern categories.
    /// </summary>
    public enum AnimalAttackPattern
    {
        Melee,
        Ranged,
        Aoe
    }
}
