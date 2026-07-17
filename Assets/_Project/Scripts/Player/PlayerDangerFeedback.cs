using PawVoyage.Combat;
using UnityEngine;

namespace PawVoyage.Player
{
    /// <summary>
    /// 플레이어 체력이 낮을 때 화면 경고와 회복 피드백을 표시합니다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerDangerFeedback : MonoBehaviour
    {
        [SerializeField, Range(0.05f, 0.9f)] private float dangerHpRatio = 0.3f;
        [SerializeField] private float edgeThickness = 18f;
        [SerializeField] private float pulseSpeed = 5.5f;
        [SerializeField] private Vector3 healPopupOffset = new Vector3(0f, 0.78f, 0f);

        private static Texture2D whiteTexture;

        private Health health;
        private GUIStyle warningStyle;

        private void Awake()
        {
            health = GetComponent<Health>();
            EnsureTexture();
        }

        private void OnEnable()
        {
            health.Healed += OnHealed;
        }

        private void OnDisable()
        {
            health.Healed -= OnHealed;
        }

        private void OnGUI()
        {
            if (!IsDangerState())
            {
                return;
            }

            EnsureStyle();
            float pulse = 0.45f + Mathf.PingPong(Time.unscaledTime * pulseSpeed, 0.35f);
            Color warningColor = new Color(1f, 0.05f, 0.05f, pulse);

            DrawRect(new Rect(0f, 0f, Screen.width, edgeThickness), warningColor);
            DrawRect(new Rect(0f, Screen.height - edgeThickness, Screen.width, edgeThickness), warningColor);
            DrawRect(new Rect(0f, 0f, edgeThickness, Screen.height), warningColor);
            DrawRect(new Rect(Screen.width - edgeThickness, 0f, edgeThickness, Screen.height), warningColor);

            GUI.Label(
                new Rect(0f, Screen.height * 0.72f, Screen.width, 34f),
                "LOW HP",
                warningStyle);
        }

        private void OnHealed(Health healedHealth, int amount)
        {
            DamagePopup.SpawnHealing(transform.position + healPopupOffset, amount);
        }

        private bool IsDangerState()
        {
            return health != null
                && health.CurrentHp > 0
                && health.MaxHp > 0
                && (float)health.CurrentHp / health.MaxHp <= dangerHpRatio;
        }

        private void EnsureStyle()
        {
            if (warningStyle != null)
            {
                return;
            }

            warningStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.24f, 0.18f, 1f) }
            };
        }

        private static void EnsureTexture()
        {
            if (whiteTexture != null)
            {
                return;
            }

            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previousColor;
        }
    }
}
