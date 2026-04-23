using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DestructibleObjectManager : MonoBehaviour
{
    public List<GameObject> objects;
    public List<Vector3> spawnPositions;
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject obj in objects)
        {
            spawnPositions.Add(obj.transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void RoundStart()
    {
        foreach (GameObject gameObject in objects)
        {
            if (gameObject.GetComponent<Destructible>() != null)
            {
                gameObject.GetComponent<Destructible>().Reset();
            }
            gameObject.transform.position = spawnPositions[objects.IndexOf(gameObject)];
            gameObject.SetActive(true);

        }
    }

}
