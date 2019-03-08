using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitColliderEnemy : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        other.GetComponent<Character>().hit = true;
    }
}
