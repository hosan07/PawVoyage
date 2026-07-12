using UnityEngine;
using UnityEngine.InputSystem;

namespace PawVoyage.Player
{
    /// <summary>
    /// Handles player movement for keyboard testing and future mobile joystick input.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody2D rb;
        private Vector2 moveInput;
        private Vector2 externalMoveInput;

        /// <summary>
        /// Current movement speed in Unity units per second.
        /// </summary>
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        /// <summary>
        /// Last non-zero movement direction. Defaults to right.
        /// </summary>
        public Vector2 LastMoveDirection { get; private set; } = Vector2.right;

        /// <summary>
        /// Current normalized movement input.
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
        /// Sets movement input from a virtual joystick or other mobile control.
        /// </summary>
        /// <param name="input">Input vector where magnitude is clamped to 1.</param>
        public void SetMoveInput(Vector2 input)
        {
            externalMoveInput = Vector2.ClampMagnitude(input, 1f);
        }

        /// <summary>
        /// Clears external movement input, usually when a virtual joystick is released.
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
