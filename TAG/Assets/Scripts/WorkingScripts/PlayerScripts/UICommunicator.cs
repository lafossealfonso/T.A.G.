using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class UICommunicator : MonoBehaviour
{
    public int playerInt;
    private PlayerandSoawnManager playerandSoawnManager;
    private CharacterController characterController;
    private PlayerUIScript playerUIScript;
    private PlayerInput playerInput;
    private PlayerIndicator playerIndicator;    
    private int lastTeamJoined;
    // Start is called before the first frame update
    void Awake()
    {
        playerandSoawnManager = FindObjectOfType<PlayerandSoawnManager>();
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        playerIndicator = GetComponentInChildren<PlayerIndicator>();
        playerandSoawnManager.UICommunicators.Add(this);
        playerInput.actions["Swap"].Enable();
        playerInput.actions["Start"].Enable();
        playerInt = playerandSoawnManager._PlayerObject.Count;
        playerandSoawnManager.UIElements[playerInt].SetActive(true);
        playerandSoawnManager.UIElements2[playerInt].SetActive(false);
        playerUIScript = playerandSoawnManager.UIElements[playerInt].GetComponent<PlayerUIScript>();
        playerUIScript.playerText.text = "Player " + (playerInt + 1).ToString();
        lastTeamJoined = 1;
        playerandSoawnManager.unassigned.Add(this.gameObject);
        playerUIScript.teamText.text = "Unassigned";
        characterController.playerSprite.sprite = playerUIScript.unassignedSprite;
        playerUIScript.playerImage.sprite = characterController.playerSprite.sprite;
    }

    // Update is called once per frame
    void Update()
    {
            playerInput.actions["Swap"].performed += SwapTeam;
            playerInput.actions["Start"].performed += StartGame;
    }
    private void SwapTeam(InputAction.CallbackContext context)
    {
        Debug.Log("TeamSwapped" + this.gameObject);
        
            if (playerandSoawnManager.gameStarted == false)
            {
                if (playerandSoawnManager.unassigned.Contains(this.gameObject))
                {
                    if (playerandSoawnManager.team2.Count < 2 && lastTeamJoined == 1)
                    {
                        playerandSoawnManager.unassigned.Remove(this.gameObject);
                        playerandSoawnManager.team2.Add(this.gameObject);
                        playerUIScript.teamText.text = "Team 2";
                    playerUIScript.teamText.color = playerUIScript.teamColors[1];
                    characterController.playerSprite.sprite = playerUIScript.team2Sprites[playerandSoawnManager.team2.IndexOf(this.gameObject)];
                    playerUIScript.playerText.color = playerUIScript.player2Colors[playerandSoawnManager.team2.IndexOf(this.gameObject)];
                    playerIndicator.textMeshProUGUI.color = playerUIScript.player2Colors[playerandSoawnManager.team2.IndexOf(this.gameObject)];
                    playerUIScript.playerImage.sprite = characterController.playerSprite.sprite;
                        if (playerandSoawnManager.team2[0] != this.gameObject && playerandSoawnManager.team2[0].GetComponent<UICommunicator>().playerUIScript.playerImage.sprite == playerandSoawnManager.team2[0].GetComponent<UICommunicator>().playerUIScript.team2Sprites[playerandSoawnManager.team2.IndexOf(this.gameObject)])
                        {
                            characterController.playerSprite.sprite = playerUIScript.team2Sprites[playerandSoawnManager.team2.IndexOf(this.gameObject) - 1];
                        playerUIScript.playerText.color = playerUIScript.player2Colors[playerandSoawnManager.team2.IndexOf(this.gameObject) - 1];
                        playerIndicator.textMeshProUGUI.color = playerUIScript.player2Colors[playerandSoawnManager.team2.IndexOf(this.gameObject) - 1];
                        playerUIScript.playerImage.sprite = characterController.playerSprite.sprite;
                        }
                        lastTeamJoined = 2;
                        return;
                    }
                    else if (playerandSoawnManager.team1.Count < 2)
                    {
                        playerandSoawnManager.unassigned.Remove(this.gameObject);
                        playerandSoawnManager.team1.Add(this.gameObject);
                        playerUIScript.teamText.text = "Team 1";
                    playerUIScript.teamText.color = playerUIScript.teamColors[0];
                    characterController.playerSprite.sprite = playerUIScript.team1Sprites[playerandSoawnManager.team1.IndexOf(this.gameObject)];
                    playerUIScript.playerText.color = playerUIScript.player1Colors[playerandSoawnManager.team1.IndexOf(this.gameObject)];
                    playerIndicator.textMeshProUGUI.color = playerUIScript.player1Colors[playerandSoawnManager.team1.IndexOf(this.gameObject)];
                    playerUIScript.playerImage.sprite = characterController.playerSprite.sprite;
                        if (playerandSoawnManager.team1[0] != this.gameObject && playerandSoawnManager.team1[0].GetComponent<UICommunicator>().playerUIScript.playerImage.sprite == playerandSoawnManager.team1[0].GetComponent<UICommunicator>().playerUIScript.team1Sprites[playerandSoawnManager.team1.IndexOf(this.gameObject)])
                        {
                            characterController.playerSprite.sprite = playerUIScript.team1Sprites[playerandSoawnManager.team1.IndexOf(this.gameObject) - 1];
                        playerUIScript.playerText.color = playerUIScript.player1Colors[playerandSoawnManager.team1.IndexOf(this.gameObject) - 1];
                        playerIndicator.textMeshProUGUI.color = playerUIScript.player1Colors[playerandSoawnManager.team1.IndexOf(this.gameObject) - 1];
                        playerUIScript.playerImage.sprite = characterController.playerSprite.sprite;
                        }
                        lastTeamJoined = 1;
                        return;
                    }
                else
                {
                    {
                        playerandSoawnManager.unassigned.Remove(this.gameObject);
                        playerandSoawnManager.team2.Add(this.gameObject);
                        playerUIScript.teamText.text = "Team 2";
                        playerUIScript.teamText.color = playerUIScript.teamColors[1];
                        characterController.playerSprite.sprite = playerUIScript.team2Sprites[playerandSoawnManager.team2.IndexOf(this.gameObject)];
                        playerUIScript.playerText.color = playerUIScript.player2Colors[playerandSoawnManager.team2.IndexOf(this.gameObject)];
                        playerUIScript.playerImage.sprite = characterController.playerSprite.sprite;
                        if (playerandSoawnManager.team2[0] != this.gameObject && playerandSoawnManager.team2[0].GetComponent<UICommunicator>().playerUIScript.playerImage.sprite == playerandSoawnManager.team2[0].GetComponent<UICommunicator>().playerUIScript.team2Sprites[playerandSoawnManager.team2.IndexOf(this.gameObject)])
                        {
                            characterController.playerSprite.sprite = playerUIScript.team2Sprites[playerandSoawnManager.team2.IndexOf(this.gameObject) - 1];
                            playerUIScript.playerText.color = playerUIScript.player2Colors[playerandSoawnManager.team2.IndexOf(this.gameObject) - 1];
                            playerUIScript.playerImage.sprite = characterController.playerSprite.sprite;
                        }
                        lastTeamJoined = 2;
                        return;
                    }
                }
                }
                if (playerandSoawnManager.team2.Contains(this.gameObject))
                {
                        playerandSoawnManager.team2.Remove(this.gameObject);
                        playerandSoawnManager.unassigned.Add(this.gameObject);
                        playerUIScript.teamText.text = "Unassigned";
                playerUIScript.teamText.color = playerUIScript.teamColors[2];
                playerUIScript.playerText.color = Color.white;
                characterController.playerSprite.sprite = playerUIScript.unassignedSprite;
                        playerUIScript.playerImage.sprite = characterController.playerSprite.sprite;
                    return;
                }
                if (playerandSoawnManager.team1.Contains(this.gameObject))
                {
                    playerandSoawnManager.team1.Remove(this.gameObject);
                    playerandSoawnManager.unassigned.Add(this.gameObject);
                    playerUIScript.teamText.text = "Unassigned";
                playerUIScript.teamText.color = playerUIScript.teamColors[2];
                playerUIScript.playerText.color = Color.white;
                characterController.playerSprite.sprite = playerUIScript.unassignedSprite;
                    playerUIScript.playerImage.sprite = characterController.playerSprite.sprite;
                    return;
                }
        }
    }
    private void StartGame(InputAction.CallbackContext context)
    {
        playerandSoawnManager.StartGame();
    }

    }
