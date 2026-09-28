using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sentinel : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private LineRenderer targetLine;

    [Header("Firing")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float heatUpDuration = 1.5f;
    [SerializeField] private int ammoCount = 6;
    [SerializeField] private float rateOfFire = 8f;
    [SerializeField] private float bulletForce = 15f;   // renamed from bulletSpeed
    [SerializeField] private float shotAngularVelocity = 360f;  
    [Header("Line of Sight")]
    [SerializeField] private LayerMask obstructionMask;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;


    [Header("Idle Feedback")]
    [SerializeField] private MMF_Player idleFeedbackPlayer;
    [SerializeField] private float idleFeedbackMinDelay = 0.5f;
    [SerializeField] private float idleFeedbackMaxDelay = 2f;

    public bool isActive;


    private readonly HashSet<Transform> playersInRange = new HashSet<Transform>();
    private Transform currentTarget;

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
    }
    private void Start()
    {
        targetLine.positionCount = 2;
        targetLine.enabled = false;

        StartCoroutine(SentinelLoop());
        StartCoroutine(IdleFeedbackLoop());
    }

    private void Update()
    {
        if (isActive == false) return;
        UpdateTargeting();
        RotateTowardsTarget();
    }

    private void UpdateTargeting()
    {
        playersInRange.RemoveWhere(p => p == null);

        currentTarget = GetClosestPlayer();

        if (currentTarget == null || !HasLineOfSightToTarget())
        {
            targetLine.enabled = false;
            return;
        }

        targetLine.enabled = true;
        targetLine.SetPosition(0, firePoint.position);
        targetLine.SetPosition(1, currentTarget.position);
    }

    private void RotateTowardsTarget()
    {
        if (currentTarget == null)
            return;

        Vector2 direction = (Vector2)currentTarget.position - (Vector2)transform.position;

        if (direction == Vector2.zero)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Same -90 adjustment PlayerMovement.RotateDirection uses, assuming
        // this sprite also faces "up" by default.
        angle -= 90f;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private Transform GetClosestPlayer()
    {
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

    private IEnumerator SentinelLoop()
    {
        while (true)
        {
            // Sit idle until someone wanders into range.
            yield return new WaitUntil(() => currentTarget != null);

            float timer = 0f;
            bool lockMaintained = true;

            while (timer < heatUpDuration)
            {
                if (currentTarget == null)
                {
                    lockMaintained = false;
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            if (!lockMaintained || currentTarget == null)
                continue; // lost the lock mid heat-up, go back to waiting

            if (HasLineOfSightToTarget())
            {
                yield return FireAllAmmo();
            }
        }
    }

    private bool HasLineOfSightToTarget()
    {
        Vector2 origin = firePoint.position;
        Vector2 targetPos = currentTarget.position;
        Vector2 direction = (targetPos - origin).normalized;
        float distance = Vector2.Distance(origin, targetPos);

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, obstructionMask);

        // If nothing on the obstruction layer is in the way before we
        // reach the target's distance, the shot is clear.
        return hit.collider == null;
    }

    private IEnumerator FireAllAmmo()
    {
        float delayBetweenShots = 1f / rateOfFire;

        for (int i = 0; i < ammoCount; i++)
        {
            if (currentTarget == null)
                yield break; // target left mid-burst

            FireBullet();

            yield return new WaitForSeconds(delayBetweenShots);
        }
    }

    private void FireBullet()
    {
        Vector2 direction = ((Vector2)currentTarget.position - (Vector2)firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogWarning("SentinelBullet prefab has no Rigidbody2D on its root GameObject — bullet won't move.", bullet);
            return;
        }

        rb.AddForce(direction * bulletForce, ForceMode2D.Impulse);
        rb.angularVelocity = shotAngularVelocity;

        Debug.Log("bullet shot target" + currentTarget.gameObject + "fire point:" + firePoint + ".");
    }

    private IEnumerator IdleFeedbackLoop()
    {
        while (true)
        {
            float delay = Random.Range(idleFeedbackMinDelay, idleFeedbackMaxDelay);
            yield return new WaitForSeconds(delay);

            bool canSeePlayer = currentTarget != null && HasLineOfSightToTarget();

            if (!canSeePlayer)
            {
                idleFeedbackPlayer.PlayFeedbacks();
            }
        }
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