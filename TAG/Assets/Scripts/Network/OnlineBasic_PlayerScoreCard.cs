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
        if (!IsServer) return;
        if (hasTriggeredWin) return;

        if (playerData.isIt.Value)
        {
            scoreValue.Value = Mathf.Min(
                maxScore, scoreValue.Value + fillSpeed * Time.fixedDeltaTime);

            if (scoreValue.Value >= maxScore)
            {
                hasTriggeredWin = true;
                // TODO: once BasicGameMode/GameManager is converted, trigger
                // the real networked win sequence here instead of just logging.
                Debug.Log($"[Score] Client {OwnerClientId} reached max score.");
            }
            else
            {
                scoreValue.Value = Mathf.Max(
                    0f, scoreValue.Value - fillSpeed * drainSpeedMultiplier * Time.fixedDeltaTime);
            }
        }
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
