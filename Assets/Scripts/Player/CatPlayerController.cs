using UnityEngine;
using UnityEngine.InputSystem;

public class CatPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float moveAcceleration = 12f;
    [SerializeField] private float moveDeceleration = 16f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Animation")]
    [SerializeField] private float walkThreshold = 0.1f;

    [Header("References")]
    [SerializeField] private Transform visualModel;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTransform;

    [Header("Model Facing")]
    [SerializeField] private float visualYawOffset = 0f;

    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");

    private Vector3 currentVelocity;

    private void Awake()
    {
        if (visualModel == null)
        {
            visualModel = transform.childCount > 0 ? transform.GetChild(0) : transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        Vector2 input = ReadMoveInput();
        bool hasMoveInput = input.sqrMagnitude > 0.0001f;
        bool runHeld = ReadRunInput();
        Vector3 moveDirection = GetMoveDirection(input);
        float targetSpeed = runHeld ? runSpeed : moveSpeed;
        Vector3 targetVelocity = moveDirection * targetSpeed;

        float accel = targetVelocity.sqrMagnitude > 0.001f ? moveAcceleration : moveDeceleration;
        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, accel * Time.deltaTime);

        transform.position += currentVelocity * Time.deltaTime;

        bool isMoving = currentVelocity.sqrMagnitude > walkThreshold * walkThreshold;
        bool isRunning = hasMoveInput && runHeld;
        if (animator != null)
        {
            animator.SetBool(IsWalking, isMoving);
            animator.SetBool(IsRunning, isRunning);
        }

        if (!isMoving || visualModel == null)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(currentVelocity.normalized, Vector3.up) * Quaternion.Euler(0f, visualYawOffset, 0f);
        visualModel.rotation = Quaternion.Slerp(
            visualModel.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private Vector3 GetMoveDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.0001f)
        {
            return Vector3.zero;
        }

        if (cameraTransform == null)
        {
            return new Vector3(input.x, 0f, input.y).normalized;
        }

        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 move = cameraForward * input.y + cameraRight * input.x;
        return move.normalized;
    }

    private Vector2 ReadMoveInput()
    {
        if (Keyboard.current == null)
        {
            return Vector2.zero;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            horizontal -= 1f;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            horizontal += 1f;
        }

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            vertical -= 1f;
        }

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            vertical += 1f;
        }

        return new Vector2(horizontal, vertical).normalized;
    }

    private bool ReadRunInput()
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
    }
}