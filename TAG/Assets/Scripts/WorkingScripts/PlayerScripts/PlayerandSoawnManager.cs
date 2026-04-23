using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerandSoawnManager : MonoBehaviour
{
    [SerializeField] public Transform[] playerSpawnPositions;
    public List<GameObject> team1;
    public List<GameObject> team2;
    public List<GameObject> unassigned;
    public PlayerCamera playerCamera;
    public GameObject crownObject;
    public bool teamCheck;
    private OddBallScoring oddBallScoring;
    public float crownCollectDelay = 1.0f; 
    public bool canCollectCrown = true;
    private int playerSizeOffset = 65;
    public GameObject[] UIElements;
    public GameObject[] UIElements2;
    public GameObject mainMenu;
    public List<UICommunicator> UICommunicators;
    public bool gameStarted;
    public GameObject gameMenu;
    public GameObject team1Win;
    public GameObject team2Win;
    public GameObject sliderObject;
    public DestructibleObjectManager destructibleObjectManager;

    public List<GameObject> _PlayerObject = new List<GameObject>();
    public GameObject _CrownObject = null;
    [SerializeField] private PlayerInputManager playerInputManager;
    
    
    // Start is called before the first frame update
    private void Awake()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
        oddBallScoring = GetComponent<OddBallScoring>();
        destructibleObjectManager = GetComponent<DestructibleObjectManager>();
        gameStarted = false;
        Time.timeScale = 0;
    }

    // Update is called once per frame

    public void PlayersCollided(GameObject player, CharacterController playerController, GameObject otherPlayer, CharacterController otherPlayerController)
    {
        if (playerController.moveDirection == -otherPlayerController.moveDirection)
        {
            StartCoroutine(playerController.ClashDelay());
            StartCoroutine(otherPlayerController.ClashDelay());
            if (playerController.hasCrown && canCollectCrown == true)
            {
                playerController.hasCrown = false;
                Instantiate(crownObject, player.transform.position, Quaternion.identity);
                oddBallScoring.playerWithCrown = null;
                StartCoroutine(StartCrownCollectDelay());
            }

            if (otherPlayerController.hasCrown && canCollectCrown == true)
            {
                otherPlayerController.hasCrown = false;
                Instantiate(crownObject, otherPlayer.transform.position, Quaternion.identity);
                oddBallScoring.playerWithCrown = null;
                StartCoroutine(StartCrownCollectDelay());
            }
        }
        else
        {
            if (playerController.moveDirection == new Vector2(0,1) && player.transform.position.x > otherPlayerController.transform.position.x - playerSizeOffset && player.transform.position.x < otherPlayerController.transform.position.x + playerSizeOffset && player.transform.position.y < otherPlayer.transform.position.y)
            {
                DestroyPlayer(otherPlayer, player);
            }
            if (playerController.moveDirection == new Vector2(0, -1) && player.transform.position.x > otherPlayerController.transform.position.x - playerSizeOffset && player.transform.position.x < otherPlayerController.transform.position.x + playerSizeOffset && player.transform.position.y > otherPlayer.transform.position.y)
            {
                DestroyPlayer(otherPlayer, player);
            }
            if (playerController.moveDirection == new Vector2(1, 0) && player.transform.position.y > otherPlayerController.transform.position.y - playerSizeOffset && player.transform.position.y < otherPlayerController.transform.position.y + playerSizeOffset && player.transform.position.x < otherPlayer.transform.position.x)
            {
                DestroyPlayer(otherPlayer, player);
            }
            if (playerController.moveDirection == new Vector2(-1, 0) && player.transform.position.y > otherPlayerController.transform.position.y - playerSizeOffset && player.transform.position.y < otherPlayerController.transform.position.y + playerSizeOffset && player.transform.position.x > otherPlayer.transform.position.x)
            {
                DestroyPlayer(otherPlayer, player);
            }

            if (otherPlayerController.moveDirection == new Vector2(0, 1) && otherPlayer.transform.position.x > playerController.transform.position.x - playerSizeOffset && otherPlayer.transform.position.x < playerController.transform.position.x + playerSizeOffset && otherPlayer.transform.position.y < player.transform.position.y)
            {
                DestroyPlayer(player, otherPlayer);
            }
            if (otherPlayerController.moveDirection == new Vector2(0, -1) && otherPlayer.transform.position.x > playerController.transform.position.x - playerSizeOffset && otherPlayer.transform.position.x < playerController.transform.position.x + playerSizeOffset && otherPlayer.transform.position.y > player.transform.position.y)
            {
                DestroyPlayer(player, otherPlayer);
            }
            if (otherPlayerController.moveDirection == new Vector2(1, 0) && otherPlayer.transform.position.y > playerController.transform.position.y - playerSizeOffset && otherPlayer.transform.position.y < playerController.transform.position.y + playerSizeOffset && otherPlayer.transform.position.x < player.transform.position.x)
            {
                DestroyPlayer(player, otherPlayer);
            }
            if (otherPlayerController.moveDirection == new Vector2(-1, 0) && otherPlayer.transform.position.y > playerController.transform.position.y - playerSizeOffset && otherPlayer.transform.position.y < playerController.transform.position.y + playerSizeOffset && otherPlayer.transform.position.x > player.transform.position.x)
            {
                DestroyPlayer(player, otherPlayer);
            }
        }
    }
    private IEnumerator StartCrownCollectDelay()
    {
        canCollectCrown = false;
        yield return new WaitForSeconds(crownCollectDelay);
        canCollectCrown = true;
    }
    private void DestroyPlayer(GameObject player, GameObject otherPlayer)
    {
        if (player.GetComponent<CharacterController>().hasCrown == true)
        {
            player.GetComponent<CharacterController>().hasCrown = false;
            otherPlayer.GetComponent<CharacterController>().hasCrown = true;
        }
        player.transform.position = playerSpawnPositions[_PlayerObject.IndexOf(player.gameObject)].position;
        StartCoroutine(player.GetComponent<CharacterController>().SpawnI());
    }
    public void StartGame()
    {
        if (team1.Count > 0 && team2.Count > 0 && unassigned.Count <= 0)
        {
            mainMenu.SetActive(false);
            Time.timeScale = 1;
            gameStarted = true;
            gameMenu.SetActive(true);
            foreach (UICommunicator ui in UICommunicators)
            {
                ui.enabled = false;
                ui.gameObject.GetComponentInChildren<PlayerIndicator>().DisableText();
                StartCoroutine(ui.gameObject.GetComponentInChildren<PlayerIndicator>().EnableText());
            }
        }
    }
    public void ResetScene()
    {
        foreach (GameObject player in _PlayerObject)
        {
            player.GetComponent<CharacterController>().hasCrown = false;
            player.transform.position = playerSpawnPositions[_PlayerObject.IndexOf(player.gameObject)].position;
        }
        team1Win.SetActive(false);
        team2Win.SetActive(false);
        sliderObject.SetActive(true);
        oddBallScoring.score = 100;
        Time.timeScale = 1;
        destructibleObjectManager.RoundStart();
        Instantiate(crownObject);
    }
}
