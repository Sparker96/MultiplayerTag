using Unity.Netcode;
using UnityEngine;

public class PlayerCameraEnabler : NetworkBehaviour
{
    private Camera playerCamera;
    private AudioListener audioListener; // if needed

    private void Awake()
    {
        // Assume you named the camera "ThirdPersonCamera"
        playerCamera = GetComponentInChildren<Camera>();
        audioListener = GetComponentInChildren<AudioListener>(); // If you have an AudioListener
    }

    public override void OnNetworkSpawn()
    {
        // Only enable this camera & audio if this is MY player
        if (IsOwner)
        {
            if (playerCamera != null)
            {
                playerCamera.enabled = true;
            }
            if (audioListener != null)
            {
                audioListener.enabled = true;
            }
        }
        else
        {
            // Make sure others' cameras and listeners are OFF
            if (playerCamera != null)
            {
                playerCamera.enabled = false;
            }
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }
        }
    }
}
