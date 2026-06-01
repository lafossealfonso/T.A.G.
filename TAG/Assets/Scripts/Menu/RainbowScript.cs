using TMPro;
using UnityEngine;

public class RainbowScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float speed = 1f;

    private void Update()
    {
        float hue = Mathf.PingPong(Time.time * speed, 1f);

        text.color = Color.HSVToRGB(hue, 1f, 1f);
    }
}
