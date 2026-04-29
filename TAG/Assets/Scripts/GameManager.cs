using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    public static event Action<GameObject, GameObject> OnPlayerTagged;

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
}
