using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TagPlayer : NetworkBehaviour
{
    // Reference to the TagManager
    private TagManager tagManager;
    private Renderer[] renderers;

    [Header("Tag Cooldown")]
    public float tagCooldown = 1f;    // 1 second by default
    private float nextTagAllowedTime = 0f;

    // Used to detect the moment we become "It"
    private bool wasItLastFrame = false;

    private void Start()
    {
        // We do want color changes on *all* clients, 
        // so do NOT return if !IsOwner here. 
        // But for collisions, we’ll still check !IsOwner in OnTriggerEnter.

        tagManager = FindFirstObjectByType<TagManager>(); 
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void Update()
    {
        // 1) Determine if THIS instance of the player is "It"
        bool iAmIt = IsCurrentPlayerIt();

        // 2) Everyone updates color locally so all players see who’s red/green
        SetColor(iAmIt ? Color.red : Color.green);

        // 3) If we just became "It" this frame, start a cooldown so we can’t tag back immediately
        if (!wasItLastFrame && iAmIt)
        {
            nextTagAllowedTime = Time.time + tagCooldown;
        }

        wasItLastFrame = iAmIt;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only the local owner can *initiate* a tag
        if (!IsOwner) return;

        // Are we "It" right now?
        if (IsCurrentPlayerIt())
        {
            // Is our cooldown over?
            if (Time.time >= nextTagAllowedTime)
            {
                // Check if we collided with another player
                TagPlayer otherPlayer = other.GetComponent<TagPlayer>();
                if (otherPlayer != null && otherPlayer != this && tagManager != null)
                {
                    // Tag them (RPC call to the server)
                    tagManager.TransferItServerRpc(otherPlayer.OwnerClientId);

                    // Also reset our cooldown so we can’t chain multiple tags instantly
                    nextTagAllowedTime = Time.time + tagCooldown;
                }
            }
        }
    }

    bool IsCurrentPlayerIt()
    {
        // If TagManager not found, can’t be it
        if (tagManager == null) return false;

        // Compare my OwnerClientId to the TagManager’s “It” ID
        return (OwnerClientId == tagManager.currentItPlayerId.Value);
    }

    void SetColor(Color c)
    {
        if (renderers == null) return;
        foreach (var r in renderers)
        {
            r.material.color = c;
        }
    }
}
