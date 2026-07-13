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
    [SerializeField] List<PlayerProfile> playerProfiles;
    private Dictionary<PlayerInput, int> selectedProfiles = new();
    private Dictionary<PlayerInput, float> profileInputCooldown = new();
    public InputAction holdButton;
    public bool hasGameStarted = false;
    [SerializeField] Slider startSlider;
    [SerializeField] Image startFillImage;
    private PlayerInput lastPlayerToHold;
    public MMF_Player startSliderFeedback;
    public Sprite defaultPlayerSprite;

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
        targetGroup.AddMember(player.transform, 0.5f, 1);
        players.Add(player);

        int profileIndex = GetFirstAvailableProfile();
        selectedProfiles[player] = profileIndex;
        profileInputCooldown[player] = 0f;

        ApplyProfile(player, profileIndex);

        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        int playerIndex = players.IndexOf(player);

        movement.playerIndex = playerIndex;
        player.transform.position = spawnPoints[playerIndex].position;

        RegisterPlayer(
            player.gameObject,
            playerProfiles[playerIndex]);

        ShakeUI();
    }

    public void ResetPlayerPositions()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.position = spawnPoints[i].position;
            PlayerMovement playerMovement = players[i].GetComponent<PlayerMovement>();
            if (playerMovement != null) playerMovement.setIsIt(false);
            scoreCards[i].ResetSliders();
        }
    }
    public void CycleProfile(PlayerInput player, int direction)
    {
        if (!selectedProfiles.ContainsKey(player))
            return;

        int current = selectedProfiles[player];
        int count = playerProfiles.Count;

        for (int i = 1; i <= count; i++)
        {
            int next = (current + direction * i + count) % count;

            if (!selectedProfiles.ContainsValue(next))
            {
                selectedProfiles[player] = next;

                ApplyProfile(player, next); // 🔥 everything updates here
                return;
            }
        }
    }   
    private void OnPlayerLeft(PlayerInput player)
    {
        int index = players.IndexOf(player);

        if (index < 0 || index >= playerMenuItems.Count)
            return;

        targetGroup.RemoveMember(player.gameObject.transform);

        players.Remove(player);

        playerMenuItems[index].menuText.text = "Press X / Enter\r\nto Join";
    }
    public void StartGame()
    {
        MMF_Player menuParentFeedbackPLayer = menuParent.GetComponent<MMF_Player>();
        if(menuParentFeedbackPLayer != null) menuParentFeedbackPLayer.PlayFeedbacks();
        SetPlayerCanMove(true);
        
        playerInputManager.DisableJoining();
    }

    private int GetFirstAvailableProfile()
    {
        for (int i = 0; i < playerProfiles.Count; i++)
        {
            bool inUse = selectedProfiles.ContainsValue(i);

            if (!inUse)
                return i;
        }

        return 0;
    }

    private void ApplyProfile(PlayerInput player, int profileIndex)
    {
        PlayerProfile profile = playerProfiles[profileIndex];

        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        int playerIndex = players.IndexOf(player);

        // ------------------------
        // VISUAL PLAYER SETUP
        // ------------------------

        movement.SetPlayerVisualColour(profile.playerColor);

        if (profile.isSprite)
        {
            movement.SetPlayerSprite(profile.playerSprite, true);
        }
        else
        {
            movement.SetPlayerSprite(defaultPlayerSprite, false);
        }

        // ------------------------
        // MENU CARD UPDATE
        // ------------------------

        PlayerMenuCard card = playerMenuItems[playerIndex];
        scoreCards[playerIndex].UpdateProfileCard(profile.playerColor, profile.playerName);

        movement.SetPlayerMenuCard(card);

        card.menuText.text = profile.playerName;
        card.ReadyUp(profile.playerColor);
    }

    public void RegisterPlayer(GameObject player, PlayerProfile playerProfile)
    {
        joinedPlayers.Add(player);

        int index = joinedPlayers.Count - 1;

        if(index < scoreCards.Count)
        {
            scoreCards[index].AssignPlayer(player, playerProfile.playerColor, playerProfile.playerName);
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
        if(hasGameStarted == false)
        {
            HandleProfileSelection();
        }
    }

    private void HandleProfileSelection()
    {
        foreach (PlayerInput player in players)
        {
            if (!profileInputCooldown.ContainsKey(player))
                continue;

            profileInputCooldown[player] -= Time.deltaTime;

            Vector2 moveInput =
                player.actions.FindAction("Move").ReadValue<Vector2>();

            if (profileInputCooldown[player] > 0f)
                continue;

            if (moveInput.x > 0.5f)
            {
                CycleProfile(player, 1);
                profileInputCooldown[player] = 0.25f;
            }
            else if (moveInput.x < -0.5f)
            {
                CycleProfile(player, -1);
                profileInputCooldown[player] = 0.25f;
            }
        }
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
                int index = selectedProfiles[lastPlayerToHold];
                Color targetColor = playerProfiles[index].playerColor;

                startFillImage.color = Color.Lerp(
                    startFillImage.color,
                    targetColor,
                    10f * Time.deltaTime
                );
            }

            if (startSlider.value >= startSlider.maxValue)
            {
                StartGame();
                hasGameStarted = true;
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

    public void SetPlayerCanMove(bool canMove) 
    {
        foreach (PlayerInput player in players)
        {
            player.GetComponent<PlayerMovement>().canMove = canMove;
        }
    }
}
