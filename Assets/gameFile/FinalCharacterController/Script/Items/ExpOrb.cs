using System.Collections;
using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    public int experienceAmount = 5;

    [Header("Orb Movement")]
    public float minSpeed = 7f;
    public float maxSpeed = 11f;
    public float pickupRange = 1.2f;
    public float burstForce = 4f;
    public float burstDuration = 0.35f;
    public float followDelay = 0.35f;

    [SerializeField] private GameObject visuals;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioSource audioSource;

    private static Transform cachedPlayer;
    private static PlayerLevel cachedLevel;

    private Vector3 velocity = Vector3.zero;
    private Vector3 initialVelocity;

    private bool isBursting = true;
    private bool isFollowing = false;
    private bool hasPickedUp = false;

    private float timer;

    private void OnEnable()
    {
        if (cachedPlayer == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                cachedPlayer = playerObj.transform;
                cachedLevel = playerObj.GetComponent<PlayerLevel>();
            }
        }

        Vector3 burstDir = (Random.onUnitSphere + Vector3.up * 0.5f).normalized;
        initialVelocity = burstDir * burstForce;

        transform.position += Vector3.up * 0.2f;
        transform.Rotate(Vector3.up, Random.Range(0f, 360f));

        timer = followDelay;
        isBursting = true;
        isFollowing = false;
        hasPickedUp = false;
        velocity = Vector3.zero;

        if (visuals != null)
            visuals.SetActive(true);
    }

    private void Update()
    {
        if (cachedPlayer == null || hasPickedUp) return;

        Vector3 targetPos = cachedPlayer.position + Vector3.up * 1.5f;
        float distance = Vector3.Distance(transform.position, targetPos);

        // check for pickup range
        if (distance <= pickupRange)
        {
            HandlePickup();
            return;
        }

        // burst movement
        if (isBursting)
        {
            transform.position += initialVelocity * Time.deltaTime;
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isBursting = false;
                isFollowing = true;
            }
        }
        //follow
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

        if (cachedLevel != null)
        {
            cachedLevel.AddExperience(experienceAmount);
        }

        float delay = 0f;
        if (audioSource && pickupSound)
        {
            audioSource.pitch = Random.Range(0.5f, 1.2f);
            AudioManager.Instance?.PlaySound(audioSource, pickupSound);
            delay = pickupSound.length / audioSource.pitch;
        }
        PoolRunner.Instance.RunCoroutine(DelayedPool(delay));
    }

    private IEnumerator DelayedPool(float delay)
    {
        yield return Helpers.GetWaitForSecond(delay);
        ObjectPooler.ReturnToPool("ExpOrb", gameObject);
    }
}
