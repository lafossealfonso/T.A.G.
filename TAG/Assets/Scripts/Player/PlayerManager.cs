using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections.Generic;
using TMPro;


public class PlayerManager : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<PlayerMenuCard> playerMenuItems;
    [SerializeField] private GameObject menuParent;
    [SerializeField] private List<PlayerInput> players;
    [SerializeField] public List<Basic_PlayerScoreCard> scoreCards;
    [SerializeField] List<GameObject> joinedPlayers = new List<GameObject>();
    [SerializeField] List<Color> playerColors;

    private void OnPlayerJoined(PlayerInput player)
    {
        targetGroup.AddMember(player.gameObject.transform, 0.5f, 1);
        players.Add(player);
        Color playerColor = playerColors[players.IndexOf(player)];
        RegisterPlayer(player.gameObject, playerColor);
        player.gameObject.GetComponent<PlayerMovement>().SetPlayerVisualColour(playerColor);
        player.transform.position = spawnPoints[players.IndexOf(player)].position;
        playerMenuItems[players.IndexOf(player)].menuText.text = "Ready";
        playerMenuItems[players.IndexOf(player)].menuText.fontStyle = (FontStyles)FontStyle.Bold;
        playerMenuItems[players.IndexOf(player)].menuText.color = playerColor;

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

    public void RegisterPlayer(GameObject player, Color playerColor)
    {
        joinedPlayers.Add(player);

        int index = joinedPlayers.Count - 1;

        if(index < scoreCards.Count)
        {
            scoreCards[index].AssignPlayer(player, playerColor);
        }
    }
}
