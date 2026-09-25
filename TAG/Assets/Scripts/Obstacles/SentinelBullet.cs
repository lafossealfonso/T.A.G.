using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SentinelBullet : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 6f;

    [Header("Fade Out")]
    [SerializeField] private float fadeDuration = 0.3f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private bool hasHit;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Collided with " + other);
        if (hasHit)
            return;

        if (other.gameObject.CompareTag("Player"))
        {
            hasHit = true;
            Rigidbody2D playerRb = other.gameObject.GetComponent<Rigidbody2D>(); 

            if (playerRb != null)
            {
                Vector2 pushDirection =
                    ((Vector2)other.transform.position - (Vector2)transform.position).normalized;

                playerRb.AddForce(pushDirection * knockbackForce, ForceMode2D.Impulse);
                Debug.Log("collided with player:"+other.gameObject.name);
            }

            else
            {
                Debug.Log("player no rb");
            }
        }

        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        

        Color startColor = spriteRenderer.color;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            spriteRenderer.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}