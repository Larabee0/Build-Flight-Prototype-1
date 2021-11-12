using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultBullet : MonoBehaviour
{
    [SerializeField] DefaultGun Parent;
    [SerializeField] GameObject OwnerShip;
    // Sets up some variables and then applies the impulse force.
    public void SetUp(Vector3 shootDir,Vector3 ShipVelocity, DefaultGun parent, GameObject ownerShip )
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        float moveSpeed = 10f;
        Parent = parent;
        OwnerShip = ownerShip;
        rb.velocity = ShipVelocity;
        rb.AddForce(shootDir * moveSpeed, ForceMode.Impulse);
    }

    // When we hit something, we do some calculations to see if we should destroy our seleves and do damage to the thing we hit, or just destroy our selves.
    private void OnTriggerEnter(Collider collider)
    {
        
        if (collider.gameObject.GetComponentInParent<DefaultGun>() != Parent)
        {
            Debug.Log("Not Gun Parent");
            if (collider.gameObject.GetComponentInParent<Rigidbody>().gameObject != Parent)
            {
                Damageable target = collider.GetComponent<Damageable>();
                if (target != null)
                {

                    target.TakeDamage(3f);
                }   
            }
            //else if (collider.gameObject.GetComponentInParent<Rigidbody>().gameObject == Parent)
            //{
            //    Debug.Log("Is Parent");
            //    
            //}
            Destroy(gameObject);
        }
        
    }
}
