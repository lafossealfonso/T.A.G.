using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float stopSpeed = 10f;
    public float slipperyFactor = 0.5f;
    [SerializeField] private PlayerInput playerInput;

    private Rigidbody2D rb;
    public Vector2 moveDirection = Vector2.zero;

    private bool i;
    public bool inputActive;
    public bool dom;
    public bool check;
    public PlayerandSoawnManager playerandSoawnManager;
    public PowerUpStorage powerUpStorage;
    public GameObject crownObject;
    public SpriteRenderer playerSprite;
    public GameObject goldTrail;

    public string thisPlayerName;

    public bool hasCrown = false;
    private bool velocityEnable;
    public bool teleportable;
    public PlayerCamera playerCamera;

    private void Awake()
    {
        playerandSoawnManager = FindObjectOfType<PlayerandSoawnManager>();
        playerCamera = FindObjectOfType<PlayerCamera>();
        rb = GetComponent<Rigidbody2D>();
        velocityEnable = true;
    }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        gameObject.name = "Player " + GetComponent<UICommunicator>().playerInt;
        playerInput.actions["Move"].Enable();
        rb.drag = slipperyFactor;
        inputActive = true;
        teleportable = true;
        StartCoroutine(SpawnI());
        playerandSoawnManager._PlayerObject.Add(this.gameObject);
        transform.position = playerandSoawnManager.playerSpawnPositions[playerandSoawnManager._PlayerObject.IndexOf(this.gameObject)].position;
        //TeamJoin();
        playerCamera.FindTargets();

    }

    private void FixedUpdate()
    {

        float horizontalInput = playerInput.actions["Move"].ReadValue<Vector2>().x;
        float verticalInput = playerInput.actions["Move"].ReadValue<Vector2>().y;
        if (velocityEnable)
        {
            if (inputActive)
            {
                if (horizontalInput > .75f || horizontalInput < -.75f || verticalInput > .75f || verticalInput < -.75f)
                {
                    if (horizontalInput > verticalInput)
                    {
                        if (horizontalInput > 0f)
                        {
                            StopMovement();
                            rb.velocity = moveDirection * moveSpeed;
                            moveDirection = new Vector2(Mathf.Sign(horizontalInput), 0f);
                        }
                        if (verticalInput < 0f)
                        {
                            StopMovement();
                            rb.velocity = moveDirection * moveSpeed;
                            moveDirection = new Vector2(0f, Mathf.Sign(verticalInput));
                        }
                    }
                    if (horizontalInput < verticalInput)
                    {
                        if (horizontalInput < 0f)
                        {
                            StopMovement();
                            rb.velocity = moveDirection * moveSpeed;
                            moveDirection = new Vector2(Mathf.Sign(horizontalInput), 0f);
                        }
                        if (verticalInput > 0f)
                        {
                            StopMovement();
                            rb.velocity = moveDirection * moveSpeed;
                            moveDirection = new Vector2(0f, Mathf.Sign(verticalInput));
                        }
                    }
                }
            }
            else
            {
                StopMovement();
                rb.velocity = moveDirection * moveSpeed;
            }
        }

        if (hasCrown)
        {
            goldTrail.SetActive(true);
        }
        else
        {
            goldTrail.SetActive(false);
        }


        //Alfonso Code//
        if (Input.GetKey(KeyCode.LeftShift) && powerUpStorage.PowerUpEquipped == true)
        {
            powerUpStorage.ExecuteCurrentPowerUp();
        }

        crownObject = transform.Find("Crown").gameObject;
        crownObject.SetActive(hasCrown);
    }

    
    private void StopMovement()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.drag = stopSpeed;
    }
    public IEnumerator BoostDelay()
    {
        velocityEnable = false;
        yield return new WaitForSeconds(.25f);
        velocityEnable = true;
    }
    public IEnumerator TeleportDelay()
    {
        teleportable = false;
        yield return new WaitForSeconds(.1f);
        teleportable = true;
    }
    public IEnumerator ClashDelay()
    {
        inputActive = false;
        i = true;
        moveDirection = -moveDirection;
        Debug.Log("Stopped" + gameObject);
        yield return new WaitForSeconds(.15f);
        dom = false;
        inputActive = true;
        i = false;
    }
    public IEnumerator SpawnI()
    {
        i = true;
        yield return new WaitForSeconds(0.5f);
        i = false;
    }
/*    private void TeamJoin()
    {
        if (playerandSoawnManager.teamCheck == true)
        {
            playerandSoawnManager.team1.Add(this.gameObject);
            playerandSoawnManager.teamCheck = false;
            var visual = Instantiate(visuals[0], transform.position, Quaternion.identity);
            visual.transform.parent = transform;
        }
        else
        {
            playerandSoawnManager.team2.Add(this.gameObject);
            playerandSoawnManager.teamCheck = true;
            var visual = Instantiate(visuals[1], transform.position, Quaternion.identity);
            visual.transform.parent = transform;
        }
    }*/
    public void OnTriggerEnter2D(Collider2D other)
    {
/*        if (other.gameObject.tag == "PowerUp" && powerUpStorage.PowerUpEquipped == false)
        {
            powerUpStorage.GetPowerUp();
            Debug.Log("Collided with a " + other.gameObject.name);
            Destroy(other.gameObject);
        }*/

        if(other.gameObject.tag == "Crown" && playerandSoawnManager.canCollectCrown == true)
        {
            playerandSoawnManager._CrownObject = null;
            Destroy(other.gameObject);
            playerCamera.FindTargets();
            hasCrown = true;
        }
        
        if (other.gameObject.tag == "Player" && inputActive == true && other.transform.GetComponent<CharacterController>().i == false)
        {
            if (other.gameObject.GetComponent<CharacterController>().dom == false)
            {
                dom = true;
            }
            playerandSoawnManager.PlayersCollided(this.gameObject, this.GetComponent<CharacterController>(), other.gameObject, other.gameObject.GetComponent<CharacterController>());
        }
    }
}
