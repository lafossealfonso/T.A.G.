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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else { Destroy(gameObject); }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void PlayerTagged(GameObject taggingPlayer, GameObject taggedPlayer)
    {
        Debug.Log(taggedPlayer.name + "was Tagged");

        OnPlayerTagged?.Invoke(taggingPlayer,taggedPlayer);
    }

    public void RemoveFromCinemachineTargetGroup(Transform transform)
    {
        targetGroup.RemoveMember(transform);
    }

    public void WinnerEvent(GameObject winningPlayer)
    {
        OnWinnerChosen?.Invoke(winningPlayer);
    }
}
