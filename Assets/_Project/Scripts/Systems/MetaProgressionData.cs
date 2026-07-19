using UnityEngine;

namespace PawVoyage.Systems
{
    public enum MetaUpgradeType
    {
        Damage,
        MaxHp,
        AttackSpeed,
        MoveSpeed,
        PickupRadius,
        BarnMaxHp,
        BarnDefense
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
        private const string BarnMaxHpLevelKey = "MetaUpgrade.BarnMaxHpLevel";
        private const string BarnDefenseLevelKey = "MetaUpgrade.BarnDefenseLevel";

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
                MetaUpgradeType.BarnMaxHp => 32,
                MetaUpgradeType.BarnDefense => 40,
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
                MetaUpgradeType.Damage => "농기구 숙련",
                MetaUpgradeType.MaxHp => "튼튼한 장화",
                MetaUpgradeType.AttackSpeed => "빠른 손놀림",
                MetaUpgradeType.MoveSpeed => "들길 질주",
                MetaUpgradeType.PickupRadius => "예리한 감각",
                MetaUpgradeType.BarnMaxHp => "헛간 보강",
                MetaUpgradeType.BarnDefense => "튼튼한 울타리",
                _ => "알 수 없음"
            };
        }

        public static string GetEffectText(MetaUpgradeType upgradeType)
        {
            return upgradeType switch
            {
                MetaUpgradeType.Damage => "레벨당 피해 +1",
                MetaUpgradeType.MaxHp => "레벨당 최대 체력 +10",
                MetaUpgradeType.AttackSpeed => "레벨당 공격 속도 +5%",
                MetaUpgradeType.MoveSpeed => "레벨당 이동 속도 +5%",
                MetaUpgradeType.PickupRadius => "레벨당 획득 범위 +6%",
                MetaUpgradeType.BarnMaxHp => "레벨당 헛간 최대 체력 +25",
                MetaUpgradeType.BarnDefense => "레벨당 헛간 피해 감소 +2%",
                _ => string.Empty
            };
        }

        /// <summary>
        /// 영구 강화로 증가하는 헛간 최대 체력 보너스입니다.
        /// </summary>
        public static int GetBarnMaxHpBonus()
        {
            return GetLevel(MetaUpgradeType.BarnMaxHp) * 25;
        }

        /// <summary>
        /// 영구 강화로 증가하는 헛간 피해 감소율입니다.
        /// </summary>
        public static float GetBarnDefenseBonus()
        {
            return GetLevel(MetaUpgradeType.BarnDefense) * 0.02f;
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
            PlayerPrefs.DeleteKey(BarnMaxHpLevelKey);
            PlayerPrefs.DeleteKey(BarnDefenseLevelKey);
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
                MetaUpgradeType.BarnMaxHp => BarnMaxHpLevelKey,
                MetaUpgradeType.BarnDefense => BarnDefenseLevelKey,
                _ => DamageLevelKey
            };
        }
    }
}
