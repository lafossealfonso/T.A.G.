using MoreMountains.Feedbacks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup targetGroup;
    public static GameManager Instance;
    public static event Action<GameObject, GameObject> OnPlayerTagged;
    public static event Action<GameObject> OnWinnerChosen;

    [Header("Feedback Setups")]
    public MMF_Player playerTaggedEffectPlayer;
    [SerializeField] MMF_Player cameraTeleportEffectPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayerTagged(GameObject taggingPlayer, GameObject taggedPlayer)
    {
        Debug.Log(taggedPlayer.name + "was Tagged");

        OnPlayerTagged?.Invoke(taggingPlayer,taggedPlayer);
        playerTaggedEffectPlayer.PlayFeedbacks();
    }

    public void RemoveFromCinemachineTargetGroup(Transform transform)
    {
        targetGroup.RemoveMember(transform);
    }

    public void WinnerEvent(GameObject winningPlayer)
    {
        OnWinnerChosen?.Invoke(winningPlayer);
    }

    public void ManagerSuicide()
    {
        Destroy(this);
    }

    public void SendToScene()
    {

    }
}
