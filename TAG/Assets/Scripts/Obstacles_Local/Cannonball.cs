using System.Collections;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
    [Header("Shrink Timing")]
    [SerializeField] private float delayBeforeShrink = 0f;
    [SerializeField] private float shrinkDuration = 1f;

    [Header("Player Knockback")]
    [SerializeField] private float knockbackForce = 10f;

    

    private Vector3 startScale;

    public void BeginShrinking()
    {
        startScale = transform.localScale;
        StartCoroutine(ShrinkRoutine());
    }

    private IEnumerator ShrinkRoutine()
    {
        if (delayBeforeShrink > 0f)
            yield return new WaitForSeconds(delayBeforeShrink);

        float timer = 0f;

        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / shrinkDuration);

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        transform.localScale = Vector3.zero;
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (playerRb == null)
            return;

        Vector2 pushDirection =
            ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;

        playerRb.AddForce(pushDirection * knockbackForce, ForceMode2D.Impulse);
    }
}

