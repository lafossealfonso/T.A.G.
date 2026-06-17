using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float boostForce;
    [SerializeField] private float slowDuration;
    [SerializeField] private float slowMultiplier;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    [SerializeField] float isItSpeedMultiplier;

    public bool canMove = false;
    public bool isIt = false;

    public SpriteRenderer playerVisual;
    public SpriteRenderer directionIndicator;
    public GameObject itIndicator;
    public TrailRenderer trail;

    public Basic_PlayerScoreCard thisPlayerScoreCard;
    public PlayerMenuCard thisPlayerMenuCard;
    public int playerIndex;

    //me when I begin the day
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetPlayerMenuCard(PlayerMenuCard playerMenuCard)
    {
        thisPlayerMenuCard = playerMenuCard;
    }

    public void SetPlayerSprite(Sprite sprite, bool isWhite)
    {
        playerVisual.sprite = sprite;
        if (isWhite)
        {
            playerVisual.color = Color.white;
        }
    }

    public void SetPlayerVisualColour(Color color)
    {
        playerVisual.color = color;
        directionIndicator.color = Color.Lerp(color, Color.white, 0.6f);
        Gradient gradient = new Gradient();

        gradient.SetKeys(
    new GradientColorKey[]
    {
        new GradientColorKey(color, 0.0f),
        new GradientColorKey(color, 1.0f)
    },
    new GradientAlphaKey[]
    {
        new GradientAlphaKey(0.6f, 0.0f), // fully visible at start
        new GradientAlphaKey(0.6f, 0.4f), // fully visible at start
        new GradientAlphaKey(0.0f, 1.0f)  // fully transparent at end
    }
);
        trail.colorGradient = gradient;
    }

    //me when the player moves
    public void OnMove(InputValue value)
    {
        if (!canMove)
        {
            thisPlayerMenuCard.PlayFeedback(playerIndex);
            return;
        }

        moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        Move();
        RotateDirection();
    }

    private void Move()
    {
        if (isIt)
        {
            rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            moveInput.y * moveSpeed
        ) + rb.linearVelocity * 0.88f;
        }

        else
        {
            rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            moveInput.y * moveSpeed
        ) + rb.linearVelocity * 0.9f;
        }
    }

    private void RotateDirection()
    {
        if (moveInput == Vector2.zero)
            return;

        float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;

        //if faces up by default

        angle -= 90f;

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    //-----------------------------------------------
    public void PlayerTeleported()
    {
        StartCoroutine(DisableCollider());
    }

    private IEnumerator DisableCollider()
    {
        GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(0.3f);

        GetComponent<Collider2D>().enabled = true;
    }
    //-----------------------------------------------
    public void BoostPad()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 direction = rb.linearVelocity.normalized;
        rb.AddForce(direction * boostForce, ForceMode2D.Impulse);
    }
    //-----------------------------------------------
    public void SlowPad()
    {
        StartCoroutine(SlowMovement());
    }

    private IEnumerator SlowMovement()
    {
        moveSpeed = moveSpeed * slowMultiplier;

        yield return new WaitForSeconds(slowDuration);

        moveSpeed = moveSpeed / slowMultiplier;
    }
    //-----------------------------------------------

    private bool canTag = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canTag) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement otherPlayer =
                collision.gameObject.GetComponent<PlayerMovement>();

            if (otherPlayer != null &&
                otherPlayer.returnIsIt() &&
                !isIt)
            {
                canTag = false;
                otherPlayer.canTag = false;

                Debug.Log("Tag successful");

                // old It loses It
                otherPlayer.setIsIt(false);

                // this player becomes new It
                GameManager.Instance.PlayerTagged(
                    this.gameObject,
                    collision.gameObject
                );

                Invoke(nameof(ResetTagCooldown), 0.5f);
                otherPlayer.Invoke(nameof(ResetTagCooldown), 0.5f);
            }
        }
    }

    void ResetTagCooldown()
    {
        canTag = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Star"))
        {
            collision.gameObject.SetActive(false);
            //GameManager.Instance.RemoveFromCinemachineTargetGroup(collision.gameObject.transform);
            setIsIt(true);
            
            Debug.Log("isit");
        }
    }


    //-----------------------------------------------
    public void setIsIt(bool isIt)
    {
        this.isIt = isIt;
        itIndicator.SetActive(isIt);
        thisPlayerScoreCard.PlayIsItFeedback();
    }

    public bool returnIsIt()
    {
        return isIt;
    }
    //-----------------------------------------------
}
