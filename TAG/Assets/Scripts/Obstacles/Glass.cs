using TMPro;
using UnityEngine;

public class Glass : MonoBehaviour
{
    [SerializeField] float health;
    [SerializeField] TextMeshProUGUI healthText;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        health--;
        healthText.text = "break(" + health + ")";
        if(health <= 0f)
        {
            Destroy(healthText.gameObject);
            Destroy(this.gameObject);
        }
    }
}
