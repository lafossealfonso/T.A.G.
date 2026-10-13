using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkedPlayerData))]
public class OnlineBasic_PlayerScoreCard : NetworkBehaviour
{
    [SerializeField] private float fillSpeed = 3f;
    [SerializeField] private float drainSpeedMultiplier = 0.3f;
    [SerializeField] private float maxScore = 100f;

    public NetworkVariable<float> scoreValue = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private NetworkedPlayerData playerData;
    private bool hasTriggeredWin;

    private void Awake()
    {
        playerData = GetComponent<NetworkedPlayerData>();
    }

    private void FixedUpdate()
    {
        if (!IsServer || hasTriggeredWin) return;

        if (playerData.isIt.Value)
        {
            scoreValue.Value = Mathf.Min(
                maxScore, scoreValue.Value + fillSpeed * Time.fixedDeltaTime);

            if (scoreValue.Value >= maxScore)
            {
                hasTriggeredWin = true;
                if (NetworkedGameManager.Instance != null)
                    NetworkedGameManager.Instance.ServerPlayerReachedMaxScore(OwnerClientId);
            }
        }
        else
        {
            scoreValue.Value = Mathf.Max(
                0f, scoreValue.Value - fillSpeed * drainSpeedMultiplier * Time.fixedDeltaTime);
        }
    }


    public void ServerMultiplyFillSpeed(float factor)
    {
        if (!IsServer) return;
        fillSpeed *= factor;
    }

    public void ServerResetScore()
    {
        if (!IsServer) return;
        scoreValue.Value = 0f;
        hasTriggeredWin = false;
    }

    public void ServerSetFillSpeed(float newFillSpeed)
    {
        if (!IsServer) return;
        fillSpeed = newFillSpeed;
    }
}
