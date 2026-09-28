using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Header("Cannonball")]
    [SerializeField] private GameObject cannonballPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private bool isActive;

    [Header("Firing")]
    [SerializeField] private float shootForce;
    [SerializeField] private float reloadTime = 2f;

    [Header("Spin")]
    [SerializeField] private float growSpinSpeed = 180f;
    [SerializeField] private float shotAngularVelocity = 720f;

    [Header("Targeting Line")]
    [SerializeField] private LineRenderer targetLine;

    private GameObject loadedBall;
    private Rigidbody2D loadedRigidbody;
    private Vector3 ballFullScale;

    // Everyone currently standing inside this Cannon's trigger area.
    private readonly HashSet<Transform> playersInRange = new HashSet<Transform>();

    private void OnEnable()
    {
        GameManager.GameStarted += SetActive;
    }

    private void OnDisable()
    {
        GameManager.GameStarted -= SetActive;
    }

    private void SetActive(bool active)
    {
        isActive = active;
        StartCoroutine(CannonLoop());
    }
    private void Start()
    {
        targetLine.positionCount = 2;
        targetLine.enabled = false;

        StartCoroutine(CannonLoop());
    }

    private void Update()
    {
        if(isActive) UpdateTargetLine();
    }

    private void UpdateTargetLine()
    {
        Transform target = GetClosestPlayer();

        if (target == null)
        {
            targetLine.enabled = false;
            return;
        }

        targetLine.enabled = true;
        targetLine.SetPosition(0, firePoint.position);
        targetLine.SetPosition(1, target.position);
    }

    private IEnumerator CannonLoop()
    {
        while (true)
        {
            SpawnBall();

            yield return GrowBall(reloadTime);

            Fire();
        }
    }

    private void SpawnBall()
    {
        loadedBall = Instantiate(cannonballPrefab, firePoint.position, firePoint.rotation);
        loadedRigidbody = loadedBall.GetComponent<Rigidbody2D>();
        loadedRigidbody.constraints = RigidbodyConstraints2D.FreezePosition;

        ballFullScale = loadedBall.transform.localScale;
        loadedBall.transform.localScale = Vector3.zero;
    }

    private IEnumerator GrowBall(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            if (loadedBall == null)
                yield break;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            loadedBall.transform.localScale = Vector3.Lerp(Vector3.zero, ballFullScale, t);
            loadedBall.transform.Rotate(0f, 0f, growSpinSpeed * Time.deltaTime);

            yield return null;
        }

        if (loadedBall != null)
            loadedBall.transform.localScale = ballFullScale;
    }

    private void Fire()
    {
        if (loadedBall == null)
            return;

        Rigidbody2D rb = loadedBall.GetComponent<Rigidbody2D>();
        loadedRigidbody.constraints = RigidbodyConstraints2D.None;
        if (rb != null)
        {
            Vector2 direction = GetFireDirection();
            rb.AddForce(direction * shootForce, ForceMode2D.Impulse);
            rb.angularVelocity = shotAngularVelocity;
        }

        Cannonball shrink = loadedBall.GetComponent<Cannonball>();
        if (shrink != null)
        {
            shrink.BeginShrinking();
        }

        loadedBall = null;
    }

    private Vector2 GetFireDirection()
    {
        Transform target = GetClosestPlayer();

        if (target != null)
        {
            return ((Vector2)target.position - (Vector2)firePoint.position).normalized;
        }

        // Nobody in range - pick a random direction instead.
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
    }

    private Transform GetClosestPlayer()
    {
        // In case a player was destroyed/disabled without firing OnTriggerExit2D
        // (e.g. respawned elsewhere via SetActive(false) then true again).
        playersInRange.RemoveWhere(p => p == null);

        Transform closest = null;
        float closestDistanceSqr = float.MaxValue;

        foreach (Transform player in playersInRange)
        {
            float distanceSqr = ((Vector2)player.position - (Vector2)firePoint.position).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closest = player;
            }
        }

        return closest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersInRange.Add(other.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersInRange.Remove(other.transform);
        }
    }
}
