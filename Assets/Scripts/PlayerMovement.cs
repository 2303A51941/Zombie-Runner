
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Running")]
    public float forwardSpeed = 8f;

    [Header("Lane Movement")]
    public int totalLanes = 3;
    public float laneDistance = 3f;
    public float laneChangeSpeed = 12f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public float gravity = -20f;

    [Header("Slide")]
    public float slideDuration = 0.8f;
    public float slideHeight = 1f;

    [Header("Animation")]
    public Animator animator;

    private CharacterController controller;
    private int currentLane = 1;
    private float verticalVelocity;
    private float normalSpeed;

    private bool isSliding;
    private bool movementStopped;
    private bool gameStarted = false;

    private float originalHeight;
    private Vector3 originalCenter;

    private Coroutine slideRoutine;
    private Coroutine boostRoutine;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        originalHeight = controller.height;
        originalCenter = controller.center;

        normalSpeed = forwardSpeed;

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetBool("isJumping", false);
            animator.SetBool("IsSliding", false);
        }
    }

    void Update()
    {
        if (!gameStarted || movementStopped)
            return;

        HandleInput();
        MovePlayer();

        if (animator != null && controller.isGrounded && !isSliding)
        {
            animator.SetBool("isJumping", false);
        }
    }

    public void StartRunning()
    {
        controller.enabled = true;

        movementStopped = false;
        gameStarted = true;

        forwardSpeed = normalSpeed;
        verticalVelocity = 0f;

        controller.height = originalHeight;
        controller.center = originalCenter;

        isSliding = false;

        if (animator != null)
        {
            animator.SetBool("isRunning", true);
            animator.SetBool("isJumping", false);
            animator.SetBool("IsSliding", false);
        }

        Debug.Log("PLAYER STARTED! Speed = " + forwardSpeed);
    }

    void HandleInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            currentLane--;
        }

        if (Keyboard.current.dKey.wasPressedThisFrame ||
            Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            currentLane++;
        }

        currentLane = Mathf.Clamp(currentLane, 0, totalLanes - 1);

        if (Keyboard.current.spaceKey.wasPressedThisFrame &&
            controller.isGrounded && !isSliding)
        {
            verticalVelocity = jumpForce;

            if (animator != null)
                animator.SetBool("isJumping", true);
        }

        if ((Keyboard.current.sKey.wasPressedThisFrame ||
             Keyboard.current.downArrowKey.wasPressedThisFrame) &&
            controller.isGrounded && !isSliding)
        {
            slideRoutine = StartCoroutine(Slide());
        }
    }

    void MovePlayer()
    {
        float targetX = (currentLane - 1) * laneDistance;

        float newX = Mathf.MoveTowards(
            transform.position.x,
            targetX,
            laneChangeSpeed * Time.deltaTime
        );

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = new Vector3(
            newX - transform.position.x,
            verticalVelocity * Time.deltaTime,
            forwardSpeed * Time.deltaTime
        );

        Vector3 oldPosition = transform.position;

        controller.Move(movement);

        // Diagnostic: check actual forward movement.
        Debug.Log(
            "Before Z: " + oldPosition.z.ToString("F2") +
            " | After Z: " + transform.position.z.ToString("F2") +
            " | Speed: " + forwardSpeed.ToString("F2") +
            " | Collision: " + controller.collisionFlags
        );
    }

    IEnumerator Slide()
    {
        isSliding = true;

        if (animator != null)
            animator.SetBool("IsSliding", true);

        controller.height = slideHeight;

        controller.center = new Vector3(
            originalCenter.x,
            slideHeight / 2f,
            originalCenter.z
        );

        yield return new WaitForSeconds(slideDuration);

        controller.height = originalHeight;
        controller.center = originalCenter;

        isSliding = false;

        if (animator != null)
            animator.SetBool("IsSliding", false);

        slideRoutine = null;
    }

    public void ActivateUVBoost(float multiplier, float duration)
    {
        if (boostRoutine != null)
            StopCoroutine(boostRoutine);

        boostRoutine = StartCoroutine(UVBoost(multiplier, duration));
    }

    IEnumerator UVBoost(float multiplier, float duration)
    {
        forwardSpeed = normalSpeed * multiplier;

        yield return new WaitForSeconds(duration);

        forwardSpeed = normalSpeed;
        boostRoutine = null;
    }

    public void StopMovement()
    {
        movementStopped = true;
        gameStarted = false;

        forwardSpeed = 0f;
        verticalVelocity = 0f;

        if (slideRoutine != null)
        {
            StopCoroutine(slideRoutine);
            slideRoutine = null;
        }

        if (boostRoutine != null)
        {
            StopCoroutine(boostRoutine);
            boostRoutine = null;
        }

        controller.height = originalHeight;
        controller.center = originalCenter;

        isSliding = false;

        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("IsSliding", false);
            animator.SetBool("isJumping", false);
        }
    }
}
