using System.Collections;
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

    public bool canMove = false;

    //me when I begin the day
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    //me when the player moves
    public void OnMove(InputValue value)
    {
        if (!canMove)
            return;
        Vector2 input = value.Get<Vector2>();

        if(Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            moveInput = new Vector2(Mathf.Sign(input.x), 0);
        }

        else if(Mathf.Abs(input.y) > 0)
        {
            moveInput = new Vector2(0, Mathf.Sign(input.y));
        }
        
        else
        {
            moveInput = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        Move();
        RotateDirection();
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            moveInput.y * moveSpeed
        ) + rb.linearVelocity * 0.9f;
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
        Vector2 direction = moveInput;
        rb.AddForce(direction.normalized * boostForce, ForceMode2D.Impulse);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.PlayerTagged(this.gameObject, collision.transform.parent.gameObject);
        }
    }
}
