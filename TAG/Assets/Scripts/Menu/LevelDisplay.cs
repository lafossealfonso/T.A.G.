using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LevelDisplay : MonoBehaviour
{
    public int currentIndexDisplayed;
    [SerializeField] List<Transform> levelDisplays;

    private void Start()
    {
        ResetDisplay();
    }
    public void SetIndexTo(int index)
    {
        foreach (Transform t in levelDisplays) 
        {
            if(index != levelDisplays.IndexOf(t))
            {
                t.gameObject.SetActive(false);
            }

            else
            {
                t.gameObject.SetActive(true);
            }

            
        }
    }

    public void ResetDisplay()
    {
        foreach (Transform t in levelDisplays) { t.gameObject.SetActive(false); }
    }
}
