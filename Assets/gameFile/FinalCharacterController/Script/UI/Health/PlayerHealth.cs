using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : HealthSystem
{
    private Ragdoll ragdoll;
    private ActiveWeapon weapons;
    private PlayerController player;
    private VolumeProfile postProcessing;
    private Vignette vignette;

    [SerializeField] private float maxIntensity = 0.5f;
    [SerializeField] private float fadeOutTime = 1.5f;

    protected override void OnStart()
    {
        ragdoll = GetComponent<Ragdoll>();
        weapons = GetComponent<ActiveWeapon>();
        player = GetComponent<PlayerController>();
        postProcessing = GetComponentInChildren<Volume>().profile;

        postProcessing.TryGet(out vignette);
        if (vignette != null)
        {
            vignette.intensity.value = 0f;
        }
    }

    protected override void OnDamage(Vector3 direction)
    {
        if (!postProcessing.TryGet(out Vignette vignette)) return;

        vignette.intensity.value = maxIntensity;
        PoolRunner.Instance.RunCoroutine(FadeOutVignette(vignette));
    }

    private IEnumerator FadeOutVignette(Vignette vignette)
    {
        float timer = 0f;
        float start = vignette.intensity.value;

        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeOutTime;
            vignette.intensity.value = Mathf.Lerp(start, 0f, t);
            yield return null;
        }

        vignette.intensity.value = 0f;
    }

    protected override void OnDeath(Vector3 direction)
    {
        ragdoll.ActivateRagdoll();
        direction.y = 1f;
        ragdoll.ApplyForce(direction);
        weapons.DropWeapon();
        weapons.DisableAiming();
        player.DisablePlayerLogic();
        GameHandler handler = FindFirstObjectByType<GameHandler>();
        if (handler != null)
        {
            handler.TriggerGameOver();
        }
    }

}
