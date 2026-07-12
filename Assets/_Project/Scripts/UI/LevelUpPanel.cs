using PawVoyage.Combat;
using PawVoyage.Systems;
using UnityEngine;

namespace PawVoyage.UI
{
    /// <summary>
    /// 레벨업 시 게임을 잠시 멈추고 1차 성장 선택지를 제공합니다.
    /// </summary>
    [RequireComponent(typeof(PlayerExperience))]
    [RequireComponent(typeof(AutoAttack))]
    [RequireComponent(typeof(Health))]
    public class LevelUpPanel : MonoBehaviour
    {
        [SerializeField] private int damageBonus = 2;
        [SerializeField] private float attackRateBonus = 0.15f;
        [SerializeField] private int maxHpBonus = 20;

        private PlayerExperience playerExperience;
        private AutoAttack autoAttack;
        private Health health;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;
        private int pendingLevelUps;
        private float previousTimeScale = 1f;

        private bool IsOpen => pendingLevelUps > 0;

        private void Awake()
        {
            playerExperience = GetComponent<PlayerExperience>();
            autoAttack = GetComponent<AutoAttack>();
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

        private void OnGUI()
        {
            if (!IsOpen)
            {
                return;
            }

            EnsureStyles();

            Rect panelRect = new Rect(
                Screen.width * 0.5f - 180f,
                Screen.height * 0.5f - 145f,
                360f,
                290f);

            GUI.Box(panelRect, GUIContent.none);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 22f, panelRect.width - 48f, 32f), "LEVEL UP", titleStyle);
            GUI.Label(new Rect(panelRect.x + 24f, panelRect.y + 58f, panelRect.width - 48f, 26f), $"Choose a reward for LV {playerExperience.CurrentLevel}", bodyStyle);

            float buttonY = panelRect.y + 100f;
            if (GUI.Button(new Rect(panelRect.x + 28f, buttonY, panelRect.width - 56f, 46f), $"+{damageBonus} Damage", buttonStyle))
            {
                ApplyDamageUpgrade();
            }

            if (GUI.Button(new Rect(panelRect.x + 28f, buttonY + 58f, panelRect.width - 56f, 46f), $"+{Mathf.RoundToInt(attackRateBonus * 100f)}% Attack Speed", buttonStyle))
            {
                ApplyAttackSpeedUpgrade();
            }

            if (GUI.Button(new Rect(panelRect.x + 28f, buttonY + 116f, panelRect.width - 56f, 46f), $"+{maxHpBonus} Max HP", buttonStyle))
            {
                ApplyHealthUpgrade();
            }
        }

        private void OnLevelGained(int newLevel)
        {
            pendingLevelUps++;

            if (pendingLevelUps == 1)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
        }

        private void ApplyDamageUpgrade()
        {
            autoAttack.AddDamageBonus(damageBonus);
            CloseOneSelection();
        }

        private void ApplyAttackSpeedUpgrade()
        {
            autoAttack.AddAttackRateMultiplier(attackRateBonus);
            CloseOneSelection();
        }

        private void ApplyHealthUpgrade()
        {
            health.AddMaxHpBonus(maxHpBonus, true);
            CloseOneSelection();
        }

        private void CloseOneSelection()
        {
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
