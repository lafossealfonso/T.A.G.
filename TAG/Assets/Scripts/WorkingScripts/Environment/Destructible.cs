using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Destructible : MonoBehaviour
{
    public int lifePoints = 3;

    [SerializeField] private List<Sprite> glassSprites;
    public int listIndexValue = 0;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            listIndexValue++;
            if (listIndexValue < lifePoints)
            {
                SpriteRenderer spriteRender = GetComponentInChildren<SpriteRenderer>();
                spriteRender.sprite = glassSprites[listIndexValue];
            }
            else if (listIndexValue >= lifePoints)
            {
                Reset();
                this.gameObject.SetActive(false);
            }
        }      
    }
    public void Reset()
    {
        lifePoints = 3;
        listIndexValue = 0;
        SpriteRenderer spriteRender = GetComponentInChildren<SpriteRenderer>();
        spriteRender.sprite = glassSprites[listIndexValue];
    }
}
