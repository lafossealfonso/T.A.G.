using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ZoomLevel
{
    public float size;
    public float xDistance;
    public float yDistance;
}

public class PlayerCamera : MonoBehaviour
{
    public List<Transform> targets = new List<Transform>(); // the players to follow
    public float smoothTime = 0.5f; // smoothing time for camera movement
    public List<ZoomLevel> zoomLevels; // zoom levels based on the distance between players
    public float maxZoom = 15f; // maximum zoom level
    public float zoomSpeed = 5f; // how fast the camera should zoom in/out
    public float minDistance = 5f; // minimum distance between players before camera starts zooming out

    private Vector3 velocity;
    private Camera cam;
    private PlayerandSoawnManager playerandSoawnManager;

    private void Awake()
    {
        if (playerandSoawnManager == null)
            playerandSoawnManager = FindObjectOfType<PlayerandSoawnManager>();
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = zoomLevels[0].size;
        FindTargets();
    }

    private void LateUpdate()
    {
        if (targets.Count == 0)
            return;

        Move();
        Zoom();
    }

    private void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + Vector3.back * 10f; // 10 units behind the center point
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    private void Zoom()
    {
        float greatestXDistance = GetGreatestXDistance();
        float greatestYDistance = GetGreatestYDistance();

        int zoomLevelX = 0;
        for (int i = 0; i < zoomLevels.Count; i++)
        {
            if (greatestXDistance > zoomLevels[i].xDistance)
                zoomLevelX = i;
        }

        int zoomLevelY = 0;
        for (int i = 0; i < zoomLevels.Count; i++)
        {
            if (greatestYDistance > zoomLevels[i].yDistance)
                zoomLevelY = i;
        }

        int zoomLevel = Mathf.Max(zoomLevelX, zoomLevelY);
        zoomLevel = Mathf.Clamp(zoomLevel, 0, zoomLevels.Count - 1);

        float newZoom = Mathf.Lerp(cam.orthographicSize, zoomLevels[zoomLevel].size, Time.deltaTime * zoomSpeed);
        cam.orthographicSize = Mathf.Clamp(newZoom, zoomLevels[0].size, maxZoom);
    }

    private Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
            return targets[0].position;

        Bounds bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.center;
    }

    private float GetGreatestXDistance()
    {
        float greatestXDistance = 0f;

        for (int i = 0; i < targets.Count; i++)
        {
            for (int j = i + 1; j < targets.Count; j++)
            {
                float distance = Mathf.Abs(targets[i].position.x - targets[j].position.x);
                if (distance > greatestXDistance)
                {
                    greatestXDistance = distance;
                }
            }
        }

        return greatestXDistance;
    }

    private float GetGreatestYDistance()
    {
        float greatestYDistance = 0f;

        for (int i = 0; i < targets.Count; i++)
        {
            for (int j = i + 1; j < targets.Count; j++)
            {
                float distance = Mathf.Abs(targets[i].position.y - targets[j].position.y);
                if (distance > greatestYDistance)
                {
                    greatestYDistance = distance;
                }
            }
        }

        return greatestYDistance;
    }

    public void FindTargets()
    {
        targets.Clear();

        if (playerandSoawnManager != null)
        {
            foreach (GameObject obj in playerandSoawnManager._PlayerObject)
            {
                targets.Add(obj.transform);
            }

            if (playerandSoawnManager._CrownObject != null)
            {
                targets.Add(playerandSoawnManager._CrownObject.transform);
            }
        }
    }
}
