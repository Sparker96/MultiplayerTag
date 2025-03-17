using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class ScoreManager : NetworkBehaviour
{
    // Server-side dictionary: playerID -> times they've been "It"
    private Dictionary<ulong, int> timesPlayerWasIt = new Dictionary<ulong, int>();

    // Client-side copy for display. Only valid on *this* client.
    private Dictionary<ulong, int> localScores = new Dictionary<ulong, int>();

    public static ScoreManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Initialize dictionary for all connected clients
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                timesPlayerWasIt[clientId] = 0;
            }
        }
    }

    public void IncrementItCount(ulong clientId)
    {
        if (!IsServer) return; // server authority

        if (timesPlayerWasIt.ContainsKey(clientId))
        {
            timesPlayerWasIt[clientId]++;
        }
        else
        {
            timesPlayerWasIt[clientId] = 1;
        }
        // Whenever we update scores, push them out to all clients
        PushScoresToAllClients();
    }

    private void PushScoresToAllClients()
    {
        List<ulong> clientIds = new List<ulong>(timesPlayerWasIt.Keys);
        List<int> scores = new List<int>();

        foreach (var cid in clientIds)
        {
            scores.Add(timesPlayerWasIt[cid]);
        }

        // Convert to arrays for RPC
        UpdateScoresClientRpc(clientIds.ToArray(), scores.ToArray());
    }

    [ClientRpc]
    private void UpdateScoresClientRpc(ulong[] clientIds, int[] scores)
    {
        // Each client updates its localScores dictionary
        localScores.Clear();

        for (int i = 0; i < clientIds.Length; i++)
        {
            localScores[clientIds[i]] = scores[i];
        }
    }


    // Optionally a method to get the score
    public int GetLocalScore(ulong clientId)
    {
        if (localScores.ContainsKey(clientId))
            return localScores[clientId];
        return 0;
    }
}
