using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitCollider : MonoBehaviour
{
    public Character playerScript;

    void OnTriggerEnter(Collider other)
    {
        other.GetComponent<Enemy>().damageToTake = playerScript.damage * playerScript.stamina;
    }
}
