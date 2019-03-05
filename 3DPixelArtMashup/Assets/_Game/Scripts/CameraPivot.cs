using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPivot : MonoBehaviour
{
    public Transform playerTransform;
    public Character characterScript;
    public float followDampening;
    public Transform camTransform;
    public float targetLookDampening;
    public RetroLookPro retroLookProScript;

    void Update()
    {
        Follow();

        if (!retroLookProScript.enabled)
            retroLookProScript.enabled = true;
    }

    void Follow()
    {
        if (characterScript.isTargeting)
        {
            var targetPosition = playerTransform.position + (characterScript.targetTransform.position - playerTransform.position) / 3;
            transform.position = Vector3.Slerp(transform.position, targetPosition, Time.deltaTime * followDampening);
            
            transform.rotation = playerTransform.rotation;
        }
        else
        {
            transform.position = Vector3.Slerp(transform.position, playerTransform.position, Time.deltaTime * followDampening);

            var rotation = Quaternion.LookRotation(new Vector3(0f, 0f, 0f));
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * targetLookDampening);
        }
    }
}
