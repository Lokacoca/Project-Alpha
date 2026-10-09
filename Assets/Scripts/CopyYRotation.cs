using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyYRotation : MonoBehaviour
{
    [Header("Target to Copy Y Rotation From")]
    public Transform target;

    void LateUpdate()
    {
        if (target == null) return;

        // Get current rotation
        Vector3 currentEuler = transform.eulerAngles;

        // Copy Y rotation from target
        currentEuler.y = target.eulerAngles.y;

        // Apply back
        transform.eulerAngles = currentEuler;
    }
}