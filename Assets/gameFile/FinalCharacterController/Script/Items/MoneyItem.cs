using System.Collections;
using UnityEngine;

public class MoneyItem : MonoBehaviour
{
    public int moneyAmount = 1;
    public float pickupRange = 1.2f;
    public float minSpeed = 7f;
    public float maxSpeed = 11f;
    public float hoverTime = 0.3f;
    [SerializeField] private float burstForce = 4f;

    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject visuals; 

    private Vector3 velocity = Vector3.zero;
    private Vector3 initialVelocity;
    private static Transform cachedPlayer;
    private static PlayerStats cachedStats;

    private float burstTimer = 0.35f;
    private float delayBeforeFollow = 0.3f;
    private bool isBursting = true;
    private bool isFollowing = false;
    private bool hasPickedUp = false;

    private void OnEnable()
    {
        if (cachedPlayer == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj)
            {
                cachedPlayer = playerObj.transform;
                cachedStats = playerObj.GetComponent<PlayerStats>();
            }
        }

        Vector3 burstDir = (Random.onUnitSphere + Vector3.up * 0.5f).normalized;
        initialVelocity = burstDir * burstForce;

        velocity = Vector3.zero;
        transform.position += Vector3.up * 0.2f;
        transform.Rotate(Vector3.up, Random.Range(0f, 360f));

        burstTimer = delayBeforeFollow;
        isBursting = true;
        isFollowing = false;
        hasPickedUp = false;

        if (visuals != null)
            visuals.SetActive(true);
    }

    private void Update()
    {
        if (cachedPlayer == null || hasPickedUp) return;

        Vector3 targetPos = cachedPlayer.position + Vector3.up * 1.5f;
        float distance = Vector3.Distance(transform.position, targetPos);

        //Always check for pickup range
        if (distance <= pickupRange)
        {
            HandlePickup();
            return;
        }

        //Animate burst, then follow
        if (isBursting)
        {
            transform.position += initialVelocity * Time.deltaTime;
            burstTimer -= Time.deltaTime;
            if (burstTimer <= 0f)
            {
                isBursting = false;
                isFollowing = true;
            }
        }
        else if (isFollowing)
        {
            float smoothTime = Mathf.Lerp(1f / maxSpeed, 1f / minSpeed, distance / 10f);
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        }
    }

    private void HandlePickup()
    {
        hasPickedUp = true;

        if (visuals != null)
            visuals.SetActive(false); // Hide visuals immediately

        if (cachedStats != null)
            cachedStats.AddMoney(moneyAmount);

        float delay = 0f;
        if (audioSource && pickupSound)
        {
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            AudioManager.Instance?.PlaySound(audioSource, pickupSound);
            delay = pickupSound.length / audioSource.pitch;
        }

        PoolRunner.Instance.RunCoroutine(DelayedPool(delay));
    }

    private IEnumerator DelayedPool(float delay)
    {
        yield return Helpers.GetWaitForSecond(delay);
        ObjectPooler.ReturnToPool("Money", gameObject);
    }
}
