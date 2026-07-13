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
            popup.Initialize(amount, isCritical);
        }

        private void Initialize(int amount, bool isCritical)
        {
            textMesh = gameObject.AddComponent<TextMesh>();
            textMesh.text = amount.ToString();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = Mathf.RoundToInt(fontSize * 100f);
            textMesh.characterSize = 0.1f;
            textMesh.color = isCritical ? new Color(1f, 0.9f, 0.2f, 1f) : Color.white;

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
