using UnityEngine;
using UnityEngine.Playables;

public class PlayerAttack : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform aimPoint;
    [SerializeField] private Transform hitPoint;
    [SerializeField] private Transform pfBulletProjectile;
    [SerializeField] private Transform bulletSpawnPosition;
    [SerializeField] private LayerMask aimColliderMask = new LayerMask();

    [Header("Stats")]
    public float damage = 10f;


    public float lerpRotationSpeed = 10;

    //get components stuff
    private PlayerState playerState;
    private PlayerActionInput playerActionInput;

    private void Awake()
    {
        playerState = GetComponent<PlayerState>();
        playerActionInput = GetComponent<PlayerActionInput>();
    }

    private void Update()
    {
        HandleShootPosition();
    }

    private void HandleShootPosition()
    {
        Vector3 mouseWorldPosition = Vector3.zero;

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        Transform hitTransform = null;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderMask))
        {
            mouseWorldPosition = raycastHit.point;
            hitTransform = raycastHit.transform;
            hitPoint.position = raycastHit.point;
        }

        //lerp aim point to hit point
        aimPoint.position = Vector3.Lerp(aimPoint.position, hitPoint.position, Time.deltaTime * lerpRotationSpeed);

        if (playerActionInput.attackPressed)
        {
            Vector3 aimDir = (mouseWorldPosition - bulletSpawnPosition.position).normalized;
            Transform bulletTransform = Instantiate(pfBulletProjectile, bulletSpawnPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
            BulletProjectile bulletProjectile = bulletTransform.GetComponent<BulletProjectile>();
            bulletProjectile.SetTarget(hitPoint.position);

            bool hitTarget = hitTransform != null && hitTransform.GetComponent<BulletTarget>() != null;
            bulletProjectile.HandleHit(hitPoint.position, hitTarget);

            var rb2d = hitTransform.GetComponent<Rigidbody>();
            if(rb2d)
            {
                rb2d.AddForceAtPosition(ray.direction * 20, hitPoint.position, ForceMode.Impulse);
            }
            var hitBox = hitTransform.GetComponent<HitBox>();
            if (hitBox)
            {
                hitBox.OnRaycastHit(this, ray.direction);
            }

            playerActionInput.IsAttackPressed(false);
            playerState.SetPlayerCombatState(PlayerCombatState.InCombat);
        }
    }
}
