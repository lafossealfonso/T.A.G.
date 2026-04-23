using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PowerUpStorage : MonoBehaviour
{
    public List<Powerup> equippedPowerUp;
    public List<GameObject> listOfPowerUp;
    public bool PowerUpEquipped;
    public GameObject playerObject;
    public string collidedPlayerName;
    public CharacterController player1Controller;


    private void Update()
    {
        if(equippedPowerUp.Count <= 0)
        {
            PowerUpEquipped = false;
        }

        else if(equippedPowerUp.Count == 1)
        {
            PowerUpEquipped=true;
            Debug.Log("Item Equipped" + equippedPowerUp[0].name);
        }

        else if(equippedPowerUp.Count > 1)
        {
            
            Debug.LogError("More than one power up equipped");
        }
    }

    public void ExecuteCurrentPowerUp()
    {
        collidedPlayerName = player1Controller.thisPlayerName;
        equippedPowerUp[0].GetComponent<Powerup>().Execute();
        
    }

    public void GetPowerUp()
    {
        int i = Random.Range(0, listOfPowerUp.Count);
        Powerup randomPowerUp = listOfPowerUp[i].GetComponent<Powerup>();
        Instantiate(randomPowerUp);
        equippedPowerUp.Add(randomPowerUp);
        
    }
}
