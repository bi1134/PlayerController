using System;
using System.Collections;
using System.Collections.Generic;
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


    //hit blink
    public float blinkDuration = 0.2f;
    private float blinkTimer;
    private Dictionary<Material, ToonShadeProperties> originalMaterialProperties = new Dictionary<Material, ToonShadeProperties>();


    //get components stuff
    private SkinnedMeshRenderer[] skinnedMeshRenderer;
    #endregion

    #region Startup
    private void Start()
    {
        currentHealth = maxHealth;
        skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();

        //store original material values
        foreach (var skin in skinnedMeshRenderer)
        {
            foreach (var material in skin.materials) // Handle multiple materials per SkinnedMeshRenderer
            {
                if (!originalMaterialProperties.ContainsKey(material)) // Avoid duplicate entries
                {
                    ToonShadeProperties properties = new ToonShadeProperties(material);
                    originalMaterialProperties[material] = properties;
                }
            }
        }

        //add hitboxes
        var rigidBodies = GetComponentsInChildren<Rigidbody>();
        foreach (var rigidBody in rigidBodies)
        {
            HitBox hitbox = rigidBody.gameObject.AddComponent<HitBox>();
            hitbox.healthSystem = this;
            if (hitbox.gameObject != gameObject)
            {
                hitbox.gameObject.layer = LayerMask.NameToLayer("Hitbox");
            }
        }

        OnStart();
    }

    #endregion

    #region Update
    private void Update()
    {

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
        OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs
        {
            healthNormalized = currentHealth / maxHealth
        });

        OnDamage(direction);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die(direction);
        }

        blinkTimer = blinkDuration;
        ApplyHitBlinkEffect(); // Check if this runs

        StopAllCoroutines();
        StartCoroutine(LerpBackToOriginalMaterials());
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

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    private void Die(Vector3 direction)
    {
        OnDeath(direction);
    }

    private void ApplyHitBlinkEffect()
    {
        foreach (var skin in skinnedMeshRenderer)
        {
            foreach (var material in skin.materials)
            {
                if (material.HasProperty("_DiffuseColor"))
                    material.SetColor("_DiffuseColor", Color.white);

                if (material.HasProperty("_AmbientStrength"))
                    material.SetFloat("_AmbientStrength", 0f);

                if (material.HasProperty("_FresnelSize"))
                    material.SetFloat("_FresnelSize", -0.28f);

                if (material.HasProperty("_LightingCutoff"))
                    material.SetFloat("_LightingCutoff", 0.42f);
            }
        }
    }

    private IEnumerator LerpBackToOriginalMaterials()
    {
        float elapsedTime = 0f;
        float lerpDuration = blinkDuration; // How long the lerp takes

        // Create dictionaries to store initial values
        Dictionary<Material, ToonShadeProperties> startValues = new Dictionary<Material, ToonShadeProperties>();

        // Store the initial (modified) values
        foreach (var kvp in originalMaterialProperties)
        {
            Material material = kvp.Key;
            startValues[material] = new ToonShadeProperties(material);
        }

        while (elapsedTime < lerpDuration)
        {
            elapsedTime += Time.deltaTime;
            float lerpFactor = elapsedTime / lerpDuration;

            foreach (var kvp in originalMaterialProperties)
            {
                Material material = kvp.Key;
                ToonShadeProperties originalProps = kvp.Value;
                ToonShadeProperties startProps = startValues[material];

                if (material.HasProperty("_DiffuseColor"))
                    material.SetColor("_DiffuseColor", Color.Lerp(startProps.diffuseColor, originalProps.diffuseColor, lerpFactor));

                if (material.HasProperty("_AmbientStrength"))
                    material.SetFloat("_AmbientStrength", Mathf.Lerp(startProps.ambientStrength, originalProps.ambientStrength, lerpFactor));

                if (material.HasProperty("_FresnelSize"))
                    material.SetFloat("_FresnelSize", Mathf.Lerp(startProps.fresnelSize, originalProps.fresnelSize, lerpFactor));

                if (material.HasProperty("_LightingCutoff"))
                    material.SetFloat("_LightingCutoff", Mathf.Lerp(startProps.lightingCutoff, originalProps.lightingCutoff, lerpFactor));
            }

            yield return null;
        }

        // Ensure materials fully return to their original state
        RestoreOriginalMaterialValues();
    }

    private void RestoreOriginalMaterialValues()
    {
        foreach (var kvp in originalMaterialProperties)
        {
            Material material = kvp.Key;
            ToonShadeProperties properties = kvp.Value;

            if (material.HasProperty("_DiffuseColor"))
                material.SetColor("_DiffuseColor", properties.diffuseColor);

            if (material.HasProperty("_AmbientStrength"))
                material.SetFloat("_AmbientStrength", properties.ambientStrength);

            if (material.HasProperty("_FresnelSize"))
                material.SetFloat("_FresnelSize", properties.fresnelSize);

            if (material.HasProperty("_LightingCutoff"))
                material.SetFloat("_LightingCutoff", properties.lightingCutoff);
        }
    }

    #endregion

    #region Virtual Func
    protected virtual void OnStart() 
    {

    }
    protected virtual void OnDeath(Vector3 direction) 
    {

    }
    protected virtual void OnDamage(Vector3 direction) 
    {

    }

    #endregion
}
