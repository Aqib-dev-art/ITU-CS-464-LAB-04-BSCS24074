using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Third-person player controller for the Level05 lab.
// Controls: WASD or arrow keys to move, Space to jump, Left Shift to sprint.
// Movement is relative to the camera, so "forward" is always away from the camera.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.5f;
    public float turnSpeed = 12f;

    [Header("Jump")]
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Respawn")]
    public float fallLimitY = -15f;   // below this height the player returns to the start

    [Header("Camera (optional, uses Main Camera if empty)")]
    public Transform cameraTransform;

    CharacterController controller;
    float verticalVelocity;
    Vector3 startPosition;
    Quaternion startRotation;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // 1. Read input
        Vector2 input = ReadMoveInput();
        bool jumpPressed = ReadJumpPressed();
        bool sprinting = ReadSprintHeld();

        // 2. Work out the move direction relative to the camera
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;
        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
        }
        Vector3 moveDir = forward * input.y + right * input.x;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // 3. Gravity and jumping
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;   // small push keeps the player stuck to the ground

        if (controller.isGrounded && jumpPressed)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;

        // 4. Move
        float speed = moveSpeed * (sprinting ? sprintMultiplier : 1f);
        Vector3 velocity = moveDir * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        // 5. Turn to face the direction of travel
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // 6. Fell off the level: go back to the start
        if (transform.position.y < fallLimitY)
            Respawn();
    }

    void Respawn()
    {
        controller.enabled = false;   // the controller must be off to teleport it
        transform.position = startPosition;
        transform.rotation = startRotation;
        controller.enabled = true;
        verticalVelocity = 0f;
    }

    // ---------- Input (works with the new Input System and the old Input Manager) ----------

    Vector2 ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k == null) return Vector2.zero;
        float x = 0f, y = 0f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) x += 1f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed) x -= 1f;
        if (k.wKey.isPressed || k.upArrowKey.isPressed) y += 1f;
        if (k.sKey.isPressed || k.downArrowKey.isPressed) y -= 1f;
        return new Vector2(x, y);
#else
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }

    bool ReadJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Space);
#endif
    }

    bool ReadSprintHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
#else
        return Input.GetKey(KeyCode.LeftShift);
#endif
    }
}
