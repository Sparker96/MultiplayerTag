using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class TagManager : NetworkBehaviour
{
    // We'll store the ClientID of who is currently "It"
    public NetworkVariable<ulong> currentItPlayerId = new NetworkVariable<ulong>(0);
    [Header("Tag SFX")]
    public AudioClip tagSoundClip;

    public override void OnNetworkSpawn()
    {
        // Only the server sets up the initial "It" upon spawn
        if (IsServer)
        {
            // For simplicity, pick a random connected client as "It"
            SetRandomPlayerIt();
        }
    }

    void SetRandomPlayerIt()
    {
        var allClients = NetworkManager.Singleton.ConnectedClientsIds;
        if (allClients.Count > 0)
        {
            ulong[] clientArray = allClients.ToArray();
            ulong randomId = clientArray[Random.Range(0, clientArray.Length)];

            currentItPlayerId.Value = randomId;
        }
    }

    // Called by a player script (e.g., TagPlayer) when they tag someone
    [Rpc(SendTo.Server)]
    public void TransferItServerRpc(ulong newItClientId)
    {
        // Only the server can change "It"
        if (!IsServer) return;
        currentItPlayerId.Value = newItClientId;

        // Increase that player's "became It" count
        ScoreManager scoreMgr = ScoreManager.Instance;
        if (scoreMgr != null)
        {
            scoreMgr.IncrementItCount(newItClientId);
        }

        // Call a client RPC so that everyone hears or sees a tag event
        PlayTagSoundClientRpc();
    }

    [ClientRpc]
    private void PlayTagSoundClientRpc()
    {
        // On each client, we can have a simple script or an AudioSource on the TagManager
        // that plays a "tag" audio clip
        var audioSource = GetComponent<AudioSource>();
        if (audioSource != null && tagSoundClip != null)
        {
            audioSource.PlayOneShot(tagSoundClip);
        }
    }
}
