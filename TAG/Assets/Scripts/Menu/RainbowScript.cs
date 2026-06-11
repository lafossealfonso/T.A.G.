using TMPro;
using UnityEngine;

public class RainbowScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField]bool isText = true;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] bool isSprite;
    [SerializeField] private float speed = 1f;

    private void Update()
    {
        float hue = Mathf.PingPong(Time.time * speed, 1f);

        if(isText && text != null)
        {
            text.color = Color.HSVToRGB(hue, 1f, 1f);
        }

        if(isSprite && sprite != null)
        {
            sprite.color = Color.HSVToRGB(hue, 1f, 1f);
        }
        
    }
}
