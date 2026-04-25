using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections.Generic;


public class PlayerManager : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<PlayerMenuCard> playerMenuItems;
    [SerializeField] private GameObject menuParent;
    [SerializeField] private List<PlayerInput> players;

    private void OnPlayerJoined(PlayerInput player)
    {
        targetGroup.AddMember(player.gameObject.transform, 0.5f, 1);
        players.Add(player);
        player.transform.position = spawnPoints[players.IndexOf(player)].position;
        playerMenuItems[players.IndexOf(player)].menuText.text = "Ready";
    }
    private void OnPlayerLeft(PlayerInput player)
    {
        targetGroup.RemoveMember(player.gameObject.transform);
        players.Remove(player);
        playerMenuItems[players.IndexOf(player)].menuText.text = "Press Start / Enter\r\nto Join";
    }
    public void StartGame()
    {
        foreach (PlayerInput player in players) 
        {
            player.GetComponent<PlayerMovement>().canMove = true;
        }
        menuParent.SetActive(false);
        playerInputManager.DisableJoining();
    }
}
