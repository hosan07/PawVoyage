using UnityEngine;

namespace PawVoyage.Systems
{
    public enum MetaUpgradeType
    {
        Damage,
        MaxHp,
        AttackSpeed,
        MoveSpeed,
        PickupRadius
    }

    /// <summary>
    /// 런 사이에 유지되는 영구 성장 레벨과 구매 비용을 관리합니다.
    /// </summary>
    public static class MetaProgressionData
    {
        private const int MaxUpgradeLevel = 10;
        private const string DamageLevelKey = "MetaUpgrade.DamageLevel";
        private const string MaxHpLevelKey = "MetaUpgrade.MaxHpLevel";
        private const string AttackSpeedLevelKey = "MetaUpgrade.AttackSpeedLevel";
        private const string MoveSpeedLevelKey = "MetaUpgrade.MoveSpeedLevel";
        private const string PickupRadiusLevelKey = "MetaUpgrade.PickupRadiusLevel";

        public static int MaxLevel => MaxUpgradeLevel;

        public static int GetLevel(MetaUpgradeType upgradeType)
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(GetKey(upgradeType), 0), 0, MaxUpgradeLevel);
        }

        public static bool IsMaxLevel(MetaUpgradeType upgradeType)
        {
            return GetLevel(upgradeType) >= MaxUpgradeLevel;
        }

        public static bool IsAdvancedLevel(MetaUpgradeType upgradeType)
        {
            return GetLevel(upgradeType) >= 5;
        }

        public static int GetCost(MetaUpgradeType upgradeType)
        {
            int level = GetLevel(upgradeType);
            if (level >= MaxUpgradeLevel)
            {
                return 0;
            }

            int baseCost = upgradeType switch
            {
                MetaUpgradeType.Damage => 25,
                MetaUpgradeType.MaxHp => 20,
                MetaUpgradeType.AttackSpeed => 30,
                MetaUpgradeType.MoveSpeed => 24,
                MetaUpgradeType.PickupRadius => 22,
                _ => 25
            };

            if (level < 5)
            {
                return baseCost + level * 15;
            }

            int earlyCost = baseCost + 4 * 15;
            int lateLevel = level - 4;
            return earlyCost + lateLevel * lateLevel * 70 + lateLevel * 35;
        }

        public static string GetDisplayName(MetaUpgradeType upgradeType)
        {
            return upgradeType switch
            {
                MetaUpgradeType.Damage => "Training Bite",
                MetaUpgradeType.MaxHp => "Sturdy Paws",
                MetaUpgradeType.AttackSpeed => "Quick Reflex",
                MetaUpgradeType.MoveSpeed => "Trail Runner",
                MetaUpgradeType.PickupRadius => "Keen Nose",
                _ => "Unknown"
            };
        }

        public static string GetEffectText(MetaUpgradeType upgradeType)
        {
            return upgradeType switch
            {
                MetaUpgradeType.Damage => "+1 Damage / Lv",
                MetaUpgradeType.MaxHp => "+10 Max HP / Lv",
                MetaUpgradeType.AttackSpeed => "+5% Attack Speed / Lv",
                MetaUpgradeType.MoveSpeed => "+5% Move Speed / Lv",
                MetaUpgradeType.PickupRadius => "+6% Pickup Radius / Lv",
                _ => string.Empty
            };
        }

        public static bool TryPurchase(MetaUpgradeType upgradeType)
        {
            int level = GetLevel(upgradeType);
            if (level >= MaxUpgradeLevel)
            {
                return false;
            }

            int cost = GetCost(upgradeType);
            if (!RunResultData.TrySpendCoins(cost))
            {
                return false;
            }

            PlayerPrefs.SetInt(GetKey(upgradeType), level + 1);
            PlayerPrefs.Save();
            return true;
        }

        public static void ResetUpgrades()
        {
            PlayerPrefs.DeleteKey(DamageLevelKey);
            PlayerPrefs.DeleteKey(MaxHpLevelKey);
            PlayerPrefs.DeleteKey(AttackSpeedLevelKey);
            PlayerPrefs.DeleteKey(MoveSpeedLevelKey);
            PlayerPrefs.DeleteKey(PickupRadiusLevelKey);
            PlayerPrefs.Save();
        }

        private static string GetKey(MetaUpgradeType upgradeType)
        {
            return upgradeType switch
            {
                MetaUpgradeType.Damage => DamageLevelKey,
                MetaUpgradeType.MaxHp => MaxHpLevelKey,
                MetaUpgradeType.AttackSpeed => AttackSpeedLevelKey,
                MetaUpgradeType.MoveSpeed => MoveSpeedLevelKey,
                MetaUpgradeType.PickupRadius => PickupRadiusLevelKey,
                _ => DamageLevelKey
            };
        }
    }
}
