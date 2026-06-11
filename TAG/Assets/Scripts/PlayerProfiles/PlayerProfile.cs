using UnityEngine;

[CreateAssetMenu(fileName = "New Player Profile", menuName = "Game/PlayerProfile")]
public class PlayerProfile : ScriptableObject
{
    public string playerName;
    public Color playerColor;
    public Sprite playerSprite;
    public bool isSprite = false;
}
