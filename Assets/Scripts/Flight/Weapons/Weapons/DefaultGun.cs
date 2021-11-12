using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultGun : MonoBehaviour
{
    public GameObject pfProjectile;
    public Transform Muzzle;
    public Transform Direction;
    public WeaponTag WeaponTag;

    private void ShootProjectile(Transform muzzleTransform,Transform Direction)
    {
        GameObject bullet = (GameObject)Instantiate(pfProjectile, muzzleTransform.position, Quaternion.identity);
        //Debug.Log(muzzleTransform.position);
        Vector3 shooDir = (Direction.position- muzzleTransform.position).normalized;
        Vector3 ShipVelocity = WeaponTag.ShipRigidbody.velocity;
        bullet.GetComponent<DefaultBullet>().SetUp(shooDir, ShipVelocity, this, WeaponTag.ShipRigidbody.gameObject);
        Destroy(bullet, 10f);
    }

    
    private void Update()
    {
        if(Input.GetButtonDown("Fire1") && WeaponTag.EnableWeapon)
        {
            ShootProjectile(Muzzle,Direction);
        }
    }
}
