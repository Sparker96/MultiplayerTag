using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCameraController : NetworkBehaviour
{
    public Transform playerTarget;       // The root transform of the player
    public float distance = 4.0f;        // How far behind the player
    public float height = 2.0f;          // How high above the player
    public float rotationSpeed = 2.0f;   // Mouse sensitivity
    public float minY = -20f;            // Clamp camera pitch
    public float maxY = 80f;

    private float currentX = 0f; // Horizontal angle (yaw)
    private float currentY = 0f; // Vertical angle (pitch)

    private void Start()
    {
        // Only enable this script if this is our local player
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (!IsOwner || playerTarget == null) return;

        // Get mouse movement
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Accumulate rotation values
        currentX += mouseX * rotationSpeed;   // Yaw (left/right)
        currentY -= mouseY * rotationSpeed;   // Pitch (up/down)

        // Clamp vertical so we can't look too far up/down
        currentY = Mathf.Clamp(currentY, minY, maxY);
    }

    private void LateUpdate()
    {
        if (!IsOwner || playerTarget == null) return;

        // -----------------------------
        // 1) Rotate the Player horizontally
        // -----------------------------
        // We'll only apply yaw (currentX) to the player's transform.
        // This makes the player turn left/right along with the camera.
        playerTarget.rotation = Quaternion.Euler(0, currentX, 0);

        // -----------------------------
        // 2) Position/Rotate the Camera
        // -----------------------------
        // We'll use both yaw (currentX) and pitch (currentY) for the camera.
        Quaternion cameraRotation = Quaternion.Euler(currentY, currentX, 0);

        // Place the camera behind the player by 'distance' along -Z of our rotation
        Vector3 offset = new Vector3(0, 0, -distance);
        Vector3 desiredPosition = cameraRotation * offset + playerTarget.position + Vector3.up * height;

        transform.position = desiredPosition;

        // Make the camera look at the player's upper body or just the root
        transform.LookAt(playerTarget.position + Vector3.up * (height * 0.75f));
    }
}
