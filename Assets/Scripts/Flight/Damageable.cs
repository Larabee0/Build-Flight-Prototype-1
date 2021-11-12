using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public ComponentStatsManager Component;
    public void TakeDamage(float Damage)
    {
        Component.Health -= Damage;
    }
}
