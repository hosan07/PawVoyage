using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// 플레이어를 부드럽게 따라가는 2D 카메라 추적 컴포넌트입니다.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private string targetName = "Player";
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float smoothTime = 0.16f;
        [SerializeField] private float snapDistance = 20f;

        private Vector3 velocity;

        private void LateUpdate()
        {
            FindTargetIfNeeded();

            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            if (Vector2.Distance(transform.position, desiredPosition) > snapDistance)
            {
                transform.position = desiredPosition;
                velocity = Vector3.zero;
                return;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, Mathf.Max(0.01f, smoothTime));
        }

        private void FindTargetIfNeeded()
        {
            if (target != null)
            {
                return;
            }

            GameObject targetObject = GameObject.Find(targetName);
            if (targetObject != null)
            {
                target = targetObject.transform;
            }
        }
    }
}
