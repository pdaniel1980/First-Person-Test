using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Move Parameters")]
    [SerializeField, Range(1, 5)] private float walkSpeed = 2.0f;
    [SerializeField] private float gravity = 9.8f;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80f;
    [SerializeField, Range(1, 10)] private float mouseSensitivityX = 3.0f;
    [SerializeField, Range(1, 10)] private float mouseSensitivityY = 3.0f;
    [SerializeField] private bool invertAxisY;

    private float rotationX;

    private Vector3 moveDirection;
    private float inputX, inputY;

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
        playerT = transform;
    }

    void Update()
    {
        HandleMovementInput();
        HandleMouseLook();

        ApplyMovements();
    }

    private void HandleMovementInput()
    {
        inputX = walkSpeed * Input.GetAxis("Horizontal");
        inputY = walkSpeed * Input.GetAxis("Vertical");

        moveDirection = playerT.TransformDirection(inputX, 0, inputY);
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

    private void ApplyMovements()
    {
        if (!characterController.isGrounded)
            moveDirection.y -= gravity;

        characterController.Move(moveDirection * Time.deltaTime);
    }
}
