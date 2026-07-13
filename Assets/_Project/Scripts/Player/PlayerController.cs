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
        [SerializeField] private bool enableTouchDragMove = true;
        [SerializeField] private bool showTouchJoystick = true;
        [SerializeField] private float joystickRadius = 90f;
        [SerializeField] private float touchDeadZone = 12f;

        private Rigidbody2D rb;
        private Vector2 moveInput;
        private Vector2 externalMoveInput;
        private Vector2 touchMoveInput;
        private Vector2 joystickCenter;
        private Vector2 joystickPointer;
        private bool joystickActive;
        private GUIStyle joystickBaseStyle;
        private GUIStyle joystickKnobStyle;

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
            touchMoveInput = ReadTouchDragInput();
            moveInput = keyboardInput.sqrMagnitude > 0f ? keyboardInput : GetFallbackMoveInput();
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

        private void OnGUI()
        {
            if (!showTouchJoystick || !joystickActive)
            {
                return;
            }

            EnsureJoystickStyles();
            float baseSize = joystickRadius * 2f;
            float knobSize = joystickRadius * 0.72f;
            Vector2 guiCenter = new Vector2(joystickCenter.x, Screen.height - joystickCenter.y);
            Vector2 guiPointer = new Vector2(joystickPointer.x, Screen.height - joystickPointer.y);

            GUI.Box(new Rect(guiCenter.x - joystickRadius, guiCenter.y - joystickRadius, baseSize, baseSize), GUIContent.none, joystickBaseStyle);
            GUI.Box(new Rect(guiPointer.x - knobSize * 0.5f, guiPointer.y - knobSize * 0.5f, knobSize, knobSize), GUIContent.none, joystickKnobStyle);
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

        private Vector2 GetFallbackMoveInput()
        {
            if (touchMoveInput.sqrMagnitude > 0f)
            {
                return touchMoveInput;
            }

            return externalMoveInput;
        }

        private Vector2 ReadTouchDragInput()
        {
            if (!enableTouchDragMove)
            {
                joystickActive = false;
                return Vector2.zero;
            }

            if (TryReadPrimaryPointer(out Vector2 position, out bool isPressed, out bool wasPressedThisFrame))
            {
                if (wasPressedThisFrame && IsJoystickStartArea(position))
                {
                    joystickActive = true;
                    joystickCenter = position;
                    joystickPointer = position;
                }

                if (isPressed && joystickActive)
                {
                    Vector2 offset = Vector2.ClampMagnitude(position - joystickCenter, joystickRadius);
                    joystickPointer = joystickCenter + offset;
                    return offset.magnitude < touchDeadZone ? Vector2.zero : offset / joystickRadius;
                }
            }

            joystickActive = false;
            joystickPointer = joystickCenter;
            return Vector2.zero;
        }

        private static bool TryReadPrimaryPointer(out Vector2 position, out bool isPressed, out bool wasPressedThisFrame)
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                position = touchscreen.primaryTouch.position.ReadValue();
                isPressed = touchscreen.primaryTouch.press.isPressed;
                wasPressedThisFrame = touchscreen.primaryTouch.press.wasPressedThisFrame;
                return isPressed || wasPressedThisFrame || touchscreen.primaryTouch.press.wasReleasedThisFrame;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                position = mouse.position.ReadValue();
                isPressed = mouse.leftButton.isPressed;
                wasPressedThisFrame = mouse.leftButton.wasPressedThisFrame;
                return isPressed || wasPressedThisFrame || mouse.leftButton.wasReleasedThisFrame;
            }

            position = Vector2.zero;
            isPressed = false;
            wasPressedThisFrame = false;
            return false;
        }

        private static bool IsJoystickStartArea(Vector2 screenPosition)
        {
            return screenPosition.x <= Screen.width * 0.55f && screenPosition.y <= Screen.height * 0.55f;
        }

        private void EnsureJoystickStyles()
        {
            joystickBaseStyle ??= CreateJoystickStyle(new Color(1f, 1f, 1f, 0.18f));
            joystickKnobStyle ??= CreateJoystickStyle(new Color(1f, 1f, 1f, 0.34f));
        }

        private static GUIStyle CreateJoystickStyle(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();

            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = texture;
            return style;
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
