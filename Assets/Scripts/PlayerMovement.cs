using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private float verticalVelocity; // for handling gravity
    private Animator animator;

    private void Start()
    {
        // Disable this script if this isn't our local player
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        // Get or add the CharacterController
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        if (!controller)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // 1) Horizontal input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        // 2) Determine speed
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // 3) Move direction
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // 4) If the controller is grounded, reset vertical velocity slightly downward
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            // A small negative helps keep the controller snapped to the ground
            verticalVelocity = -2f;
        }

        // 5) Jump
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity = jumpForce;
            animator.SetBool("isJumping", true);
        }

        // 6) Apply gravity each frame
        verticalVelocity += gravity * Time.deltaTime;

        // 7) Final velocity vector
        Vector3 velocity = move * currentSpeed;
        velocity.y = verticalVelocity; // Apply vertical movement

        // 8) Move the controller
        controller.Move(velocity * Time.deltaTime);
        float horizontalSpeed = new Vector2(moveX, moveZ).magnitude * currentSpeed;
        animator.SetFloat("speed", horizontalSpeed);
        // If we want to detect landing for jump
        if (controller.isGrounded)
        {
            animator.SetBool("isGrounded", true);
        }
        else
        {
            animator.SetBool("isGrounded", false);
        }
        animator.SetBool("isJumping", false);
    }
}
