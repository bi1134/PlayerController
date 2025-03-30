using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : HealthSystem
{
    private Ragdoll ragdoll;
    private ActiveWeapon weapons;
    private PlayerController controller;
    VolumeProfile postProcessing;
    private CameraManager cameraManager;

    protected override void OnStart()
    {
        ragdoll = GetComponent<Ragdoll>();
        weapons = GetComponent<ActiveWeapon>();
        controller = GetComponent<PlayerController>();
        postProcessing = GetComponent<Volume>().profile;
        cameraManager = FindFirstObjectByType<CameraManager>();
    }

    protected override void OnDeath(Vector3 direction)
    {
        ragdoll.ActivateRagdoll();
        direction.y = 1;
        ragdoll.ApplyForce(direction);
        weapons.DropWeapon();
        weapons.DisableAiming();
        controller.enabled = false;
        cameraManager.EnableKillCam();
    }

    protected override void OnDamage(Vector3 direction)
    {
        Vignette vignette;
        if(postProcessing.TryGet(out vignette))
        {
            float percent = 1.0f - (currentHealth / maxHealth);
            vignette.intensity.value = percent * 0.5f;
        }
    }
}
