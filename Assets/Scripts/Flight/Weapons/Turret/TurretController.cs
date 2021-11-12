using UnityEngine;


public class TurretController : MonoBehaviour
{
    [SerializeField] private TurretAim TurretAim = null;

    public Transform TargetPoint = null;
    //[Range(0f, 100f)] public float ThrottlePosition = 0f;
    [Range(0f, 500f)] public float Range = 25f;
    public ShipManager Root = null;

    private void Awake()
    {
        if (TurretAim == null)
            Debug.LogError(name + ": TurretController not assigned a TurretAim!");
    }

    private void Update()
    {
        if (TurretAim == null)
            return;

        //if (TargetPoint == null)
        //    TurretAim.IsIdle = TargetPoint == null;
        //else
        //    TurretAim.AimPosition = TargetPoint.position;
        if (Root != null)
        {
            TurretAim.AimPosition = Root.MouseFlight.MouseAimPos;
        }
        
        // This sets the turret to idle mode.
        //if (Input.GetMouseButtonDown(0))
        //    TurretAim.IsIdle = !TurretAim.IsIdle;
    }
}
