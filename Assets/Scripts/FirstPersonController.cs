using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    private bool IsSprinting => Input.GetKey(KeyCode.LeftShift);
    private bool ShouldJump => Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded;
    private bool ShouldCrouch => Input.GetKeyDown(KeyCode.LeftControl) && !isDuringCrouchAnimation && characterController.isGrounded;

    [Header("Move Parameters")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 6f;
    private Vector3 moveDirection;
    private float inputX, inputY;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80f;
    [SerializeField, Range(1, 10)] private float mouseSensitivityX = 3f;
    [SerializeField, Range(1, 10)] private float mouseSensitivityY = 3f;
    [SerializeField] private bool invertAxisY;
    private float rotationX;

    [Header("Jump Parameters")]
    [SerializeField] private float gravity = 30f;
    [SerializeField] private float jumpForce = 10f;
    private float movementSpeed;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchedHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private Vector3 crouchedCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    [SerializeField] private float timeToCrouch = 0.25f;
    private bool isCrouching;
    private bool isDuringCrouchAnimation;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.11f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float bobSpeed;
    private float bobAmount;
    private float defaultYPos = 0;
    private float bobTimer;

    // Sliding parameters
    private Vector3 hitPointNormal;
    private bool IsSliding
    {
        get
        {
            if (characterController.isGrounded && Physics.Raycast(playerT.position, Vector3.down, out RaycastHit hit, 2f))
            {
                hitPointNormal = hit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) >= characterController.slopeLimit;
            }

            return false;
        }
    }

    private Camera playerCamera;
    private CharacterController characterController;
    private Transform playerCameraT;
    private Transform playerT;

    private void Awake()
    {
        playerCamera = gameObject.GetComponentInChildren<Camera>();
        characterController = gameObject.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        playerCameraT = playerCamera.gameObject.transform;
        defaultYPos = playerCameraT.localPosition.y;
        playerT = transform;
    }

    void Update()
    {
        HandleMovementInput();
        HandleMouseLook();
        HandleJump();
        HandleCrouch();
        HandleHeadBob();

        ApplyMovements();
    }

    private void HandleMovementInput()
    {
        movementSpeed = isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
        inputX = movementSpeed * Input.GetAxis("Horizontal");
        inputY = movementSpeed * Input.GetAxis("Vertical");

        moveDirection = playerT.TransformDirection(inputX, moveDirection.y, inputY);
    }

    private void HandleMouseLook()
    {
        if (invertAxisY)
            rotationX += Input.GetAxis("Mouse Y") * mouseSensitivityY;
        else
            rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivityY;
        
        rotationX = Mathf.Clamp(rotationX, -lowerLookLimit, upperLookLimit);

        playerCameraT.localRotation = Quaternion.Euler(rotationX, 0, 0);
        playerT.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * mouseSensitivityX, 0);
    }

    private void HandleJump()
    {
        if (ShouldJump)
        {
            moveDirection.y = jumpForce;
        }
    }

    private void HandleCrouch()
    {
        if (ShouldCrouch)
            StartCoroutine(CrouchCoroutine());
    }

    private IEnumerator CrouchCoroutine()
    {
        if (isCrouching && Physics.Raycast(playerCameraT.position, Vector3.up, 1.0f))
            yield break;

        isDuringCrouchAnimation = true;

        float timeElapsed = 0;
        float currentHeight = characterController.height;
        float targetHeight = isCrouching ? standingHeight : crouchedHeight;
        Vector3 currentCenter = characterController.center;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchedCenter;

        while (timeElapsed < timeToCrouch)
        {
            float t = (timeElapsed / timeToCrouch);
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, t);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        isDuringCrouchAnimation = false;
    }

    private void HandleHeadBob()
    {
        if (!characterController.isGrounded)
            return;

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            bobSpeed = (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            bobAmount = (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount);
            bobTimer += Time.deltaTime * bobSpeed;

            playerCameraT.localPosition = new Vector3(
                playerCameraT.localPosition.x,
                defaultYPos + (Mathf.Sin(bobTimer) * bobAmount),
                playerCameraT.localPosition.z);
        }
    }

    private void ApplyMovements()
    {
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        if (IsSliding)
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }
}
