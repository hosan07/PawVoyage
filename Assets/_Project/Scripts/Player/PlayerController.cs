using UnityEngine;
using UnityEngine.InputSystem;

namespace PawVoyage.Player
{
    /// <summary>
    /// 키보드 테스트와 추후 모바일 조이스틱 입력을 위한 플레이어 이동을 처리합니다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody2D rb;
        private Vector2 moveInput;
        private Vector2 externalMoveInput;

        /// <summary>
        /// 초당 Unity 단위 기준 현재 이동 속도입니다.
        /// </summary>
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 마지막으로 입력된 0이 아닌 이동 방향입니다. 기본값은 오른쪽입니다.
        /// </summary>
        public Vector2 LastMoveDirection { get; private set; } = Vector2.right;

        /// <summary>
        /// 현재 정규화된 이동 입력입니다.
        /// </summary>
        public Vector2 MoveInput => moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        private void Update()
        {
            Vector2 keyboardInput = ReadKeyboardInput();
            moveInput = keyboardInput.sqrMagnitude > 0f ? keyboardInput : externalMoveInput;
            moveInput = Vector2.ClampMagnitude(moveInput, 1f);

            if (moveInput.sqrMagnitude > 0.001f)
            {
                LastMoveDirection = moveInput.normalized;
            }
        }

        private void FixedUpdate()
        {
            Vector2 nextPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(nextPosition);
        }

        /// <summary>
        /// 가상 조이스틱 또는 다른 모바일 컨트롤에서 받은 이동 입력을 설정합니다.
        /// </summary>
        /// <param name="input">크기가 1 이하로 제한되는 입력 벡터입니다.</param>
        public void SetMoveInput(Vector2 input)
        {
            externalMoveInput = Vector2.ClampMagnitude(input, 1f);
        }

        /// <summary>
        /// 외부 이동 입력을 지웁니다. 보통 가상 조이스틱에서 손을 뗄 때 사용합니다.
        /// </summary>
        public void ClearMoveInput()
        {
            externalMoveInput = Vector2.zero;
        }

        private static Vector2 ReadKeyboardInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            Vector2 input = Vector2.zero;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                input.x -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                input.x += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                input.y -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                input.y += 1f;
            }

            return Vector2.ClampMagnitude(input, 1f);
        }
    }
}
