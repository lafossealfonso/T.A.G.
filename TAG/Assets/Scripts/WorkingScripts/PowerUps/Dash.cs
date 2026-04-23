using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Dash : Powerup
{
    public float characterSpeed;
    public string playerName;
    private GameObject playerObject;
    public PowerUpStorage powerUpStorage;

    public override void Execute()
    {
        Debug.Log("Executing Started");
        powerUpStorage = FindObjectOfType<PowerUpStorage>();
        playerName = powerUpStorage.collidedPlayerName;
        Debug.Log(powerUpStorage.collidedPlayerName);
        playerObject = GameObject.Find(playerName);
        Debug.Log(playerObject);
        playerObject.GetComponent<CharacterController>().moveSpeed = 1700f;
        StartCoroutine(Coroutine());
    }

    IEnumerator Coroutine()
    {
        yield return new WaitForSeconds(7f);
        playerObject.GetComponent<CharacterController>().moveSpeed = 1000f;
        Debug.Log("Executing Finished");

    }
}
