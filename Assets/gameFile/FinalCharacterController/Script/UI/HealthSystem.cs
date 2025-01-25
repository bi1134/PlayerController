using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    #region Class Varialbles
    public event EventHandler<OnHealthChangedEventArgs> OnHealthChanged;
    public class OnHealthChangedEventArgs : EventArgs
    {
        public float healthNormalized;
    }

    [Header("Stats")]
    public float maxHealth;
    public float currentHealth;


    public float ragdollForce;

    //hit blink
    public float blinkIntensity = 10f;
    public float blinkSize = 0.25f;
    public float blinkDuration;
    private float blinkTimer;

    //get components stuff
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Ragdoll ragdoll;

    #endregion

    #region Startup
    private void Start()
    {
        currentHealth = maxHealth;
        ragdoll = GetComponent<Ragdoll>();
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        var rigidBodies = GetComponentsInChildren<Rigidbody>();
        foreach (var rigidBody in rigidBodies)
        {
            HitBox hitbox = rigidBody.gameObject.AddComponent<HitBox>();
            hitbox.healthSystem = this;
        }
    }

    #endregion

    #region Update
    private void Update()
    {
        blinkTimer = Time.deltaTime;
        float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
        float intensity = lerp * blinkIntensity;

        skinnedMeshRenderer.material.color = Color.yellow * intensity;
    }

    #endregion

    #region Function
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    public void TakeDamage(float amount, Vector3 direction)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die(direction);
        }
        blinkTimer = blinkDuration;
        if (OnHealthChanged != null) OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs
        {
            healthNormalized = currentHealth / maxHealth
        });
    }


    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        if (OnHealthChanged != null) OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs
        {
            healthNormalized = currentHealth / maxHealth
        });
    }

    private void Die(Vector3 direction)
    {
        ragdoll.ActivateRagdoll();
        direction.y = 1;
        ragdoll.ApplyForce(direction * ragdollForce);
    }
    #endregion
}
