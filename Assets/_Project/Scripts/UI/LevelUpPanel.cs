using PawVoyage.Combat;
using PawVoyage.Player;
using PawVoyage.Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PawVoyage.UI
{
    public enum LevelUpRewardType
    {
        Damage,
        AttackSpeed,
        MaxHp,
        PickupRadius,
        MoveSpeed,
        ProjectileCount,
        Pierce,
        Range,
        AuraWeapon,
        BoomerangWeapon
    }

    /// <summary>
    /// 레벨업 시 게임을 잠시 멈추고 1차 성장 선택지를 제공합니다.
    /// </summary>
    [RequireComponent(typeof(PlayerExperience))]
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(AutoAttack))]
    [RequireComponent(typeof(SecondaryWeaponController))]
    [RequireComponent(typeof(Health))]
    public class LevelUpPanel : MonoBehaviour
    {
        private const int WeaponRewardWeight = 45;
        private const int PassiveRewardWeight = 35;
        private const int UtilityRewardWeight = 20;

        [SerializeField] private int damageBonus = 2;
        [SerializeField] private float attackRateBonus = 0.15f;
        [SerializeField] private int maxHpBonus = 20;
        [SerializeField] private float pickupRadiusBonus = 0.25f;
        [SerializeField] private float moveSpeedBonus = 0.12f;
        [SerializeField] private int projectileBonus = 1;
        [SerializeField] private int pierceBonus = 1;
        [SerializeField] private float rangeBonus = 0.18f;
        [SerializeField] private WeaponData auraWeapon = null;
        [SerializeField] private WeaponData boomerangWeapon = null;
        [SerializeField, Range(1, 5)] private int visibleRewardCount = 3;

        private PlayerExperience playerExperience;
        private PlayerController playerController;
        private AutoAttack autoAttack;
        private SecondaryWeaponController secondaryWeaponController;
        private Health health;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;
        private int pendingLevelUps;
        private float previousTimeScale = 1f;
        private readonly LevelUpRewardType[] weaponRewardPool =
        {
            LevelUpRewardType.AuraWeapon,
            LevelUpRewardType.BoomerangWeapon
        };

        private readonly LevelUpRewardType[] passiveRewardPool =
        {
            LevelUpRewardType.Damage,
            LevelUpRewardType.AttackSpeed,
            LevelUpRewardType.ProjectileCount,
            LevelUpRewardType.Pierce,
            LevelUpRewardType.Range
        };

        private readonly LevelUpRewardType[] utilityRewardPool =
        {
            LevelUpRewardType.MaxHp,
            LevelUpRewardType.PickupRadius,
            LevelUpRewardType.MoveSpeed
        };

        private readonly LevelUpRewardType[] fallbackRewards =
        {
            LevelUpRewardType.Damage,
            LevelUpRewardType.AttackSpeed,
            LevelUpRewardType.MaxHp,
            LevelUpRewardType.PickupRadius,
            LevelUpRewardType.MoveSpeed
        };

        private readonly LevelUpRewardType[] visibleRewards = new LevelUpRewardType[10];
        private readonly Rect[] rewardButtonRects = new Rect[5];

        private bool IsOpen => pendingLevelUps > 0;

        private void Awake()
        {
            playerExperience = GetComponent<PlayerExperience>();
            playerController = GetComponent<PlayerController>();
            autoAttack = GetComponent<AutoAttack>();
            secondaryWeaponController = GetComponent<SecondaryWeaponController>();
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            playerExperience.LevelGained += OnLevelGained;
        }

        private void OnDisable()
        {
            playerExperience.LevelGained -= OnLevelGained;
            ResumeGameIfNeeded();
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            UpdateButtonRects();
            HandlePointerSelection();
            HandleKeyboardSelection();
        }

        private void OnGUI()
        {
            if (!IsOpen)
            {
                return;
            }

            EnsureStyles();
            Rect panelRect = GetPanelRect();
            UpdateButtonRects();

            GUI.Box(panelRect, GUIContent.none);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 22f, panelRect.width - 48f, 32f), "LEVEL UP", titleStyle);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 58f, panelRect.width - 48f, 26f), $"Choose a reward for LV {playerExperience.CurrentLevel}", bodyStyle);

            int count = GetVisibleRewardCount();
            for (int i = 0; i < count; i++)
            {
                if (GUI.Button(rewardButtonRects[i], GetRewardLabel(visibleRewards[i]), buttonStyle))
                {
                    ApplyReward(visibleRewards[i]);
                }
            }
        }

        private void OnLevelGained(int newLevel)
        {
            pendingLevelUps++;
            GameSfx.PlayLevelUp();
            RollVisibleRewards();

            if (pendingLevelUps == 1)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
        }

        private void ApplyReward(LevelUpRewardType rewardType)
        {
            switch (rewardType)
            {
                case LevelUpRewardType.Damage:
                    ApplyDamageUpgrade();
                    break;
                case LevelUpRewardType.AttackSpeed:
                    ApplyAttackSpeedUpgrade();
                    break;
                case LevelUpRewardType.MaxHp:
                    ApplyHealthUpgrade();
                    break;
                case LevelUpRewardType.PickupRadius:
                    ApplyPickupRadiusUpgrade();
                    break;
                case LevelUpRewardType.MoveSpeed:
                    ApplyMoveSpeedUpgrade();
                    break;
                case LevelUpRewardType.ProjectileCount:
                    ApplyProjectileUpgrade();
                    break;
                case LevelUpRewardType.Pierce:
                    ApplyPierceUpgrade();
                    break;
                case LevelUpRewardType.Range:
                    ApplyRangeUpgrade();
                    break;
                case LevelUpRewardType.AuraWeapon:
                    ApplySecondaryWeapon(auraWeapon);
                    break;
                case LevelUpRewardType.BoomerangWeapon:
                    ApplySecondaryWeapon(boomerangWeapon);
                    break;
            }
        }

        private void ApplyDamageUpgrade()
        {
            if (!IsOpen)
            {
                return;
            }

            autoAttack.AddDamageBonus(damageBonus);
            CloseOneSelection();
        }

        private void ApplyAttackSpeedUpgrade()
        {
            if (!IsOpen)
            {
                return;
            }

            autoAttack.AddAttackRateMultiplier(attackRateBonus);
            CloseOneSelection();
        }

        private void ApplyHealthUpgrade()
        {
            if (!IsOpen)
            {
                return;
            }

            health.AddMaxHpBonus(maxHpBonus, true);
            CloseOneSelection();
        }

        private void ApplyPickupRadiusUpgrade()
        {
            if (!IsOpen)
            {
                return;
            }

            playerExperience.AddPickupRadiusMultiplier(pickupRadiusBonus);
            CloseOneSelection();
        }

        private void ApplyMoveSpeedUpgrade()
        {
            if (!IsOpen)
            {
                return;
            }

            playerController.AddMoveSpeedMultiplier(moveSpeedBonus);
            CloseOneSelection();
        }

        private void ApplyProjectileUpgrade()
        {
            if (!IsOpen)
            {
                return;
            }

            autoAttack.AddProjectileBonus(projectileBonus);
            CloseOneSelection();
        }

        private void ApplyPierceUpgrade()
        {
            if (!IsOpen)
            {
                return;
            }

            autoAttack.AddPierceBonus(pierceBonus);
            CloseOneSelection();
        }

        private void ApplyRangeUpgrade()
        {
            if (!IsOpen)
            {
                return;
            }

            autoAttack.AddRangeMultiplier(rangeBonus);
            CloseOneSelection();
        }

        private void ApplySecondaryWeapon(WeaponData weaponData)
        {
            if (!IsOpen)
            {
                return;
            }

            if (secondaryWeaponController.AcquireOrUpgradeWeapon(weaponData))
            {
                CloseOneSelection();
            }
        }

        private void CloseOneSelection()
        {
            GameSfx.PlayCardSelect();
            pendingLevelUps = Mathf.Max(0, pendingLevelUps - 1);
            ResumeGameIfNeeded();
        }

        private void ResumeGameIfNeeded()
        {
            if (pendingLevelUps > 0)
            {
                return;
            }

            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        }

        private Rect GetPanelRect()
        {
            return new Rect(
                Screen.width * 0.5f - 180f,
                Screen.height * 0.5f - 145f,
                360f,
                290f);
        }

        private void UpdateButtonRects()
        {
            Rect panelRect = GetPanelRect();
            float buttonY = panelRect.y + 100f;
            for (int i = 0; i < rewardButtonRects.Length; i++)
            {
                rewardButtonRects[i] = new Rect(panelRect.x + 28f, buttonY + 58f * i, panelRect.width - 56f, 46f);
            }
        }

        private void HandlePointerSelection()
        {
            if (!TryGetPressedScreenPosition(out Vector2 screenPosition))
            {
                return;
            }

            Vector2 guiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            int count = GetVisibleRewardCount();
            for (int i = 0; i < count; i++)
            {
                if (rewardButtonRects[i].Contains(guiPosition))
                {
                    ApplyReward(visibleRewards[i]);
                    return;
                }
            }
        }

        private static bool TryGetPressedScreenPosition(out Vector2 screenPosition)
        {
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPosition = mouse.position.ReadValue();
                return true;
            }

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            {
                screenPosition = touchscreen.primaryTouch.position.ReadValue();
                return true;
            }

            screenPosition = Vector2.zero;
            return false;
        }

        private void HandleKeyboardSelection()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            {
                ApplyRewardByIndex(0);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            {
                ApplyRewardByIndex(1);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            {
                ApplyRewardByIndex(2);
            }
            else if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame)
            {
                ApplyRewardByIndex(3);
            }
            else if (keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame)
            {
                ApplyRewardByIndex(4);
            }
        }

        private void ApplyRewardByIndex(int index)
        {
            if (index < 0 || index >= GetVisibleRewardCount())
            {
                return;
            }

            ApplyReward(visibleRewards[index]);
        }

        private void RollVisibleRewards()
        {
            int count = GetVisibleRewardCount();
            for (int i = 0; i < visibleRewards.Length; i++)
            {
                visibleRewards[i] = LevelUpRewardType.Damage;
            }

            int filledCount = 0;
            int safety = 0;
            while (filledCount < count && safety < 60)
            {
                safety++;
                LevelUpRewardType rewardType = PickWeightedReward();
                if (ContainsVisibleReward(rewardType, filledCount) || !CanOfferReward(rewardType))
                {
                    continue;
                }

                visibleRewards[filledCount] = rewardType;
                filledCount++;
            }

            for (int i = filledCount; i < count; i++)
            {
                visibleRewards[i] = GetFallbackReward(i);
                for (int j = 0; j < fallbackRewards.Length && ContainsVisibleReward(visibleRewards[i], i); j++)
                {
                    visibleRewards[i] = fallbackRewards[(i + j + 1) % fallbackRewards.Length];
                }
            }
        }

        private int GetVisibleRewardCount()
        {
            return Mathf.Clamp(visibleRewardCount, 1, rewardButtonRects.Length);
        }

        private LevelUpRewardType PickWeightedReward()
        {
            int roll = Random.Range(0, WeaponRewardWeight + PassiveRewardWeight + UtilityRewardWeight);
            if (roll < WeaponRewardWeight)
            {
                return weaponRewardPool[Random.Range(0, weaponRewardPool.Length)];
            }

            if (roll < WeaponRewardWeight + PassiveRewardWeight)
            {
                return passiveRewardPool[Random.Range(0, passiveRewardPool.Length)];
            }

            return utilityRewardPool[Random.Range(0, utilityRewardPool.Length)];
        }

        private bool ContainsVisibleReward(LevelUpRewardType rewardType, int filledCount)
        {
            for (int i = 0; i < filledCount; i++)
            {
                if (visibleRewards[i] == rewardType)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanOfferReward(LevelUpRewardType rewardType)
        {
            return rewardType switch
            {
                LevelUpRewardType.AuraWeapon => auraWeapon != null && (secondaryWeaponController.HasWeapon(auraWeapon) || secondaryWeaponController.EquippedCount < secondaryWeaponController.MaxSecondaryWeapons),
                LevelUpRewardType.BoomerangWeapon => boomerangWeapon != null && (secondaryWeaponController.HasWeapon(boomerangWeapon) || secondaryWeaponController.EquippedCount < secondaryWeaponController.MaxSecondaryWeapons),
                _ => true
            };
        }

        private LevelUpRewardType GetFallbackReward(int index)
        {
            return fallbackRewards[index % fallbackRewards.Length];
        }

        private string GetRewardLabel(LevelUpRewardType rewardType)
        {
            return rewardType switch
            {
                LevelUpRewardType.Damage => $"+{damageBonus} Damage",
                LevelUpRewardType.AttackSpeed => $"+{Mathf.RoundToInt(attackRateBonus * 100f)}% Attack Speed",
                LevelUpRewardType.MaxHp => $"+{maxHpBonus} Max HP",
                LevelUpRewardType.PickupRadius => $"+{Mathf.RoundToInt(pickupRadiusBonus * 100f)}% Pickup Radius",
                LevelUpRewardType.MoveSpeed => $"+{Mathf.RoundToInt(moveSpeedBonus * 100f)}% Move Speed",
                LevelUpRewardType.ProjectileCount => $"+{projectileBonus} Projectile",
                LevelUpRewardType.Pierce => $"+{pierceBonus} Pierce",
                LevelUpRewardType.Range => $"+{Mathf.RoundToInt(rangeBonus * 100f)}% Attack Range",
                LevelUpRewardType.AuraWeapon => GetWeaponRewardLabel(auraWeapon),
                LevelUpRewardType.BoomerangWeapon => GetWeaponRewardLabel(boomerangWeapon),
                _ => "Unknown Reward"
            };
        }

        private string GetWeaponRewardLabel(WeaponData weaponData)
        {
            if (weaponData == null)
            {
                return "Weapon Slot";
            }

            return secondaryWeaponController.HasWeapon(weaponData)
                ? $"{weaponData.DisplayName} Upgrade"
                : $"Get {weaponData.DisplayName}";
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                normal = { textColor = Color.white }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
        }
    }
}
