using UnityEngine;

namespace PawVoyage.Combat
{
    /// <summary>
    /// 월드 공간에서 위로 떠오르며 사라지는 피해 숫자입니다.
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.55f;
        [SerializeField] private float floatSpeed = 1.4f;
        [SerializeField] private float fontSize = 0.28f;

        private TextMesh textMesh;
        private Color startColor;
        private float elapsed;

        public static void Spawn(Vector3 worldPosition, int amount, bool isCritical)
        {
            GameObject popupObject = new GameObject("DamagePopup");
            popupObject.transform.position = worldPosition;

            DamagePopup popup = popupObject.AddComponent<DamagePopup>();
            popup.Initialize(amount.ToString(), isCritical ? new Color(1f, 0.9f, 0.2f, 1f) : Color.white);
        }

        /// <summary>
        /// 회복량을 초록색 팝업으로 표시합니다.
        /// </summary>
        public static void SpawnHealing(Vector3 worldPosition, int amount)
        {
            GameObject popupObject = new GameObject("HealingPopup");
            popupObject.transform.position = worldPosition;

            DamagePopup popup = popupObject.AddComponent<DamagePopup>();
            popup.Initialize($"+{Mathf.Max(0, amount)}", new Color(0.25f, 1f, 0.45f, 1f));
        }

        private void Initialize(string text, Color color)
        {
            textMesh = gameObject.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = Mathf.RoundToInt(fontSize * 100f);
            textMesh.characterSize = 0.1f;
            textMesh.color = color;

            MeshRenderer meshRenderer = textMesh.GetComponent<MeshRenderer>();
            meshRenderer.sortingOrder = 20;

            startColor = textMesh.color;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, lifetime));
            Color color = startColor;
            color.a = 1f - progress;
            textMesh.color = color;

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
