using Assets.Scripts;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInputActions inputActions;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Light flashlightObject;
    [SerializeField] private HUD hud;
    [SerializeField] private MonsterController monster;

    [SerializeField] private float walkSpeed = 6.0f;

    [SerializeField] private float lookSensitivity = 0.5f;
    [SerializeField] private float lookXLimit = 60.0f;

    [SerializeField] private float rotationX = 0.0f;

    [SerializeField] private float deathDuration = 5.0f;

    private Vector3 startPosition;

    public bool isFlashlightOn = true;

    private InputAction move;
    private InputAction look;
    private InputAction flashlight;

    private bool isDying = false;

    private IEnumerator DeathCoroutine ()
    {
        isDying = true;
        monster.ChangeState(MonsterState.Killing);
        float lerpSpeed = 2.0f;
        float passedTime = 0f;
        Vector3 originalPosition = playerCamera.transform.position;
        float originalFov = playerCamera.fieldOfView;
        while (passedTime < deathDuration)
        {
            Vector3 targetDirection =  monster.transform.position + new Vector3(0f,0.35f) - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            playerCamera.transform.rotation = Quaternion.Slerp(playerCamera.transform.rotation, targetRotation, lerpSpeed * Time.deltaTime);

            float xOffset = Random.Range(-1f, 1f) * passedTime / 50;
            float yOffset = Random.Range(-1f, 1f) * passedTime / 50;
            bool toggleFlashlight = Random.Range(0f, 1.0f) > 0.99f;
            if (toggleFlashlight)
            {
                isFlashlightOn = !isFlashlightOn;
                flashlightObject.enabled = isFlashlightOn;
            }
            playerCamera.transform.position = originalPosition + new Vector3(xOffset, yOffset, 0f);

            playerCamera.fieldOfView = Mathf.Lerp(15, originalFov, 1/passedTime);

            passedTime += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.position = originalPosition;
        playerCamera.fieldOfView = originalFov;
        StartCoroutine(RestartCoroutine());
    }

    private IEnumerator RestartCoroutine()
    {
        isDying = false;
        transform.position = startPosition;
        monster.ResetPosition();
        monster.ChangeState(MonsterState.Patrolling);
        yield return null;
    }

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

    private void FlashlightControl()
    {
        if (flashlight.WasPressedThisFrame())
        {
            isFlashlightOn = !isFlashlightOn;
            flashlightObject.enabled = isFlashlightOn;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Key"))
        {
            hud.AddKey();
            Destroy(other.gameObject);
        } else if (other.CompareTag("Note"))
        {
            hud.AddNote();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Door"))
        {
            if (hud.keyCounter >= hud.keyMax)
            {
                hud.OpenDoor();
                Destroy(other.gameObject);
            } else
            {
                hud.LockedDoor();
            }
        } else if (other.CompareTag("Monster"))
        {
            StartCoroutine(DeathCoroutine());
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
        flashlight = inputActions.Player.Light;

        startPosition = transform.position;

        move.Enable();
        look.Enable();
        flashlight.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!isDying)
        {
            RotateCamera();
            MovePlayer();
            FlashlightControl();
        }

    }
}
