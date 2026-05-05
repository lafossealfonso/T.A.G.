using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using MoreMountains.Feedbacks;


public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<PlayerMenuCard> playerMenuItems;
    [SerializeField] private GameObject menuParent;
    [SerializeField] private List<PlayerInput> players;
    [SerializeField] public List<Basic_PlayerScoreCard> scoreCards;
    [SerializeField] List<GameObject> joinedPlayers = new List<GameObject>();
    [SerializeField] List<Color> playerColors;
    [SerializeField] List<string> playerNames;
    public InputAction holdButton;
    bool hasGameStarted = false;
    [SerializeField] Slider startSlider;
    [SerializeField] Image startFillImage;
    private PlayerInput lastPlayerToHold;
    public MMF_Player startSliderFeedback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else { Destroy(gameObject); }
    }

    private void Start()
    {
        startSlider.value = 0f;
    }

    private void OnEnable()
    {
        holdButton.Enable();

        
    }

    private void OnDisable()
    {
        holdButton.Disable();

        
    }
    private void OnPlayerJoined(PlayerInput player)
    {
        targetGroup.AddMember(player.gameObject.transform, 0.5f, 1);
        players.Add(player);
        Color playerColor = playerColors[players.IndexOf(player)];
        string playerName = playerNames[players.IndexOf(player)];
        RegisterPlayer(player.gameObject, playerColor, playerName);
        player.gameObject.GetComponent<PlayerMovement>().SetPlayerVisualColour(playerColor);
        player.gameObject.GetComponent<PlayerMovement>().SetPlayerMenuCard(playerMenuItems[players.IndexOf(player)]);
        player.gameObject.GetComponent<PlayerMovement>().playerIndex = players.IndexOf(player);
        player.transform.position = spawnPoints[players.IndexOf(player)].position;
        playerMenuItems[players.IndexOf(player)].menuText.text = "READY";
        playerMenuItems[players.IndexOf(player)].menuText.fontStyle = (FontStyles)FontStyle.Bold;
        playerMenuItems[players.IndexOf(player)].menuText.color = playerColor;
        ShakeUI();

    }
    private void OnPlayerLeft(PlayerInput player)
    {
        targetGroup.RemoveMember(player.gameObject.transform);
        players.Remove(player);
        playerMenuItems[players.IndexOf(player)].menuText.text = "Press X / Enter\r\nto Join";
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

    public void RegisterPlayer(GameObject player, Color playerColor, string playerName)
    {
        joinedPlayers.Add(player);

        int index = joinedPlayers.Count - 1;

        if(index < scoreCards.Count)
        {
            scoreCards[index].AssignPlayer(player, playerColor, playerName);
        }
    }

    public void DisablePlayerMovement()
    {
        foreach (PlayerInput player in players)
        {
            player.GetComponent<PlayerMovement>().canMove = false;
        }
    }

    private void Update()
    {
        ColorFillSlider();
    }

    private void ColorFillSlider()
    {
        if (hasGameStarted) return;

        int holdingCount = 0;

        foreach (PlayerInput player in players)
        {
            var action = player.actions.FindAction("Hold");

            if (action == null) continue;

            // Count who is currently holding
            if (action.IsPressed())
            {
                holdingCount++;
            }

            // Detect NEW press (this is the key part)
            if (action.WasPressedThisFrame())
            {
                lastPlayerToHold = player;
                ShakeUI();
            }
        }

        if (holdingCount > 0)
        {
            float baseSpeed = 0.3f;
            float speedMultiplier = Mathf.Pow(2f, holdingCount - 1);

            startSlider.value += baseSpeed * speedMultiplier * Time.deltaTime;

            if (lastPlayerToHold != null)
            {
                int index = players.IndexOf(lastPlayerToHold);
                Color targetColor = playerColors[index];

                startFillImage.color = Color.Lerp(
                    startFillImage.color,
                    targetColor,
                    10f * Time.deltaTime
                );
            }

            if (startSlider.value >= startSlider.maxValue)
            {
                StartGame();
            }
        }
        else
        {
            startSlider.value -= 0.6f * Time.deltaTime;
        }
    }

    void ShakeUI()
    {
        startSliderFeedback.PlayFeedbacks();
    }
}
