using PawVoyage.Combat;
using PawVoyage.Player;
using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// 저장된 영구 업그레이드를 런 시작 시 플레이어 능력치에 반영합니다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(AutoAttack))]
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PlayerExperience))]
    public class PlayerMetaUpgradeApplier : MonoBehaviour
    {
        private void Awake()
        {
            ApplyUpgrades();
        }

        private void ApplyUpgrades()
        {
            GetComponent<AutoAttack>().AddDamageBonus(MetaProgressionData.GetLevel(MetaUpgradeType.Damage));
            GetComponent<Health>().AddMaxHpBonus(MetaProgressionData.GetLevel(MetaUpgradeType.MaxHp) * 10, true);
            GetComponent<AutoAttack>().AddAttackRateMultiplier(MetaProgressionData.GetLevel(MetaUpgradeType.AttackSpeed) * 0.05f);
            GetComponent<PlayerController>().AddMoveSpeedMultiplier(MetaProgressionData.GetLevel(MetaUpgradeType.MoveSpeed) * 0.05f);
            GetComponent<PlayerExperience>().AddPickupRadiusMultiplier(MetaProgressionData.GetLevel(MetaUpgradeType.PickupRadius) * 0.06f);
        }
    }
}
