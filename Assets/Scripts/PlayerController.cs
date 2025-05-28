using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputActionMap playerActionMap;
    [SerializeField] private float sideSpeed = 5f;
    public Rigidbody rb;

    private Animator animator;
    private Vector2 movementInput;
    private Vector3 currentMovement;
    private bool isMovementPressed;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody bile�eni bulunamad�.");
        }

        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerActionMap = playerInput.actions.FindActionMap("PlayerController");
            if (playerActionMap != null)
            {
                playerActionMap.FindAction("Movement").started += Move;
                playerActionMap.FindAction("Movement").performed += Move;
                playerActionMap.FindAction("Movement").canceled += Move;
                playerActionMap.Enable();
            }
            else
            {
                Debug.LogError("PlayerController action map bulunamad�.");
            }
        }
        else
        {
            Debug.LogError("PlayerInput bile�eni bulunamad�.");
        }
    }

    void Move(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
        currentMovement.x = movementInput.x;
        currentMovement.z = movementInput.y;
        isMovementPressed = movementInput.x != 0 || movementInput.y != 0;
    }

    private void FixedUpdate()
    {
        if (isMovementPressed)
        {
            rb.MovePosition(transform.position + currentMovement * sideSpeed * Time.deltaTime);
        }

        if (rb.position.y < 59f)
        {
            Debug.Log("Player d��erek �ld�.");
            animator.SetTrigger("Fall");
            FindAnyObjectByType<GameManager>().endGame();
        }
    }

    private void OnDisable()
    {
        if (playerActionMap != null)
        {
            playerActionMap.Disable();
        }
    }
}
