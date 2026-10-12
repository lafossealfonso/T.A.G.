using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkedGameManager : NetworkBehaviour
{
    public static NetworkedGameManager Instance { get; private set; }

    // Online equivalent of GameManager.GameStarted. Networked obstacles
    // will subscribe to this later, same OnEnable/OnDisable pattern.
    public static event Action<bool> OnlineGameStarted;

    public NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        gameStarted.OnValueChanged += HandleGameStartedChanged;

        // If we joined mid-game, fire the event for the current value too.
        if (gameStarted.Value) HandleGameStartedChanged(false, true);
    }

    public override void OnNetworkDespawn()
    {
        gameStarted.OnValueChanged -= HandleGameStartedChanged;
    }

    private void HandleGameStartedChanged(bool previous, bool current)
    {
        OnlineGameStarted?.Invoke(current);
    }

    public void ServerStartGame()
    {
        if (!IsServer) return;
        gameStarted.Value = true;
    }
}