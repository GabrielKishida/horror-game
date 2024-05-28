using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInputActions inputActions;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera playerCamera;

    [SerializeField] private float walkSpeed = 6.0f;

    [SerializeField] private float lookSensitivity = 0.5f;
    [SerializeField] private float lookXLimit = 60.0f;

    [SerializeField] private float rotationX = 0.0f;

    private InputAction move;
    private InputAction look;

    private void RotateCamera()
    {
        Vector2 lookDelta = look.ReadValue<Vector2>();
        if (lookDelta != Vector2.zero)
        {
            lookDelta *= lookSensitivity;
            rotationX = Mathf.Clamp(rotationX - lookDelta.y, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, lookDelta.x, 0);
        }
    }

    private void MovePlayer()
    {
        Vector2 movement = move.ReadValue<Vector2>();
        if (movement != Vector2.zero)
        {
            Vector3 movement3D = new Vector3(movement.x, 0, movement.y) * walkSpeed;
            movement3D = transform.TransformDirection(movement3D);
            characterController.Move(movement3D * Time.deltaTime);
        }
    }

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void Start()
    {
        move = inputActions.Player.Move;
        look = inputActions.Player.Look;

        move.Enable();
        look.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        RotateCamera();
        MovePlayer();
    }
}
