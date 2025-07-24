using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerSpawner : NetworkBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private NetworkPrefabRef sessionInfoNetwork;
    [SerializeField] private Transform spwanLocation;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // Only spawn the local player on their own machine
        if (runner.LocalPlayer == player)
        {
            float spawnRadius = 3f;

            // Random position inside a circle on the XZ plane
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = spwanLocation.position + new Vector3(randomOffset.x, 0f, randomOffset.y);

            NetworkObject playerObj = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
            runner.SetPlayerObject(player, playerObj);
            playerObj.GetComponent<NetworkThirdPersonLoader>().PlayerName = "Player " + player.PlayerId;

            Debug.Log($"Spawned local player: {player} at {spawnPos}");
        }

        // Spawn SessionInfoNetwork once by the host only
        if (runner.IsSharedModeMasterClient && runner.LocalPlayer == player)
        {
            runner.Spawn(sessionInfoNetwork, Vector3.zero, Quaternion.identity);
            Debug.Log("SessionInfoNetwork spawned by master client");
        }
    }
    // Empty INetworkRunnerCallbacks implementations (required for interface) ---

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}