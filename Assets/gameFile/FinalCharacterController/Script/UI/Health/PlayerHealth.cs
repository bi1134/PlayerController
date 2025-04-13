using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : HealthSystem
{
    private Ragdoll ragdoll;
    private ActiveWeapon weapons;
    private PlayerController player;
    private VolumeProfile postProcessing;
    //private CameraManager cameraManager;

    protected override void OnStart()
    {
        ragdoll = GetComponent<Ragdoll>();
        weapons = GetComponent<ActiveWeapon>();
        player = GetComponent<PlayerController>();
        postProcessing = GetComponentInChildren<Volume>().profile;
        //cameraManager = FindFirstObjectByType<CameraManager>();
    }
    protected override void OnDeath(Vector3 direction)
    {
        ragdoll.ActivateRagdoll();
        direction.y = 1f;
        ragdoll.ApplyForce(direction);
        weapons.DropWeapon();
        weapons.DisableAiming();
        player.DisablePlayerLogic();
        //cameraManager.EnableKillCam();
    }
    protected override void OnDamage(Vector3 direction)
    {
        Vignette vignette;
        if (postProcessing.TryGet(out vignette))
        {
            float percent = 1.0f - (currentHealth / maxHealth);
            vignette.intensity.value = percent * 0.5f;
        }
    }

   
}
