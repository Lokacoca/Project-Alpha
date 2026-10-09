using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSway : MonoBehaviour
{
    [Header("Ground Detection")]
    public LayerMask whatIsGround;
    private bool grounded;
    private bool wasGroundedLastFrame;

    [Header("Settings")]
    public Rigidbody playerRb;
    public Transform orientation; // forward/right reference

    [Header("Positional Sway (Landing / Jump)")]
    public float verticalImpactAmount = 50f; // bump intensity
    public float verticalSmooth = 10f;      // downward smoothing
    public float recoverySpeed = 20f;       // speed of going back up

    [Header("Tilt Sway (Rotation)")]
    public float tiltAmount = 0.05f;
    public float maxTilt = 0.5f;
    public float tiltSmooth = 1f;

    private float targetVerticalOffset = 0f;

    private Vector3 lastVelocity;
    private Vector3 currentPosOffset;
    private Quaternion currentTilt;
    private Vector3 originalLocalPos;

    void Start()
    {
        if (playerRb == null)
            playerRb = GetComponentInParent<Rigidbody>();
        if (orientation == null)
            orientation = transform.parent;

        lastVelocity = playerRb.velocity;
        originalLocalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        if (playerRb == null || orientation == null) return;

        Vector3 velocity = playerRb.velocity;
        Vector3 velocityChange = velocity - lastVelocity;
        lastVelocity = velocity;

        bool grounded = PlayerMovement.Instance.Getgrounded();

        // Landing detection
        if (!wasGroundedLastFrame && grounded)
        {
            targetVerticalOffset = Mathf.Clamp(-velocityChange.y * verticalImpactAmount * 1f, -1.15f, 1.1f);
        }

        wasGroundedLastFrame = grounded;

        // Smoothly recover back to zero over time
        targetVerticalOffset = Mathf.Lerp(targetVerticalOffset, 0f, Time.deltaTime * recoverySpeed);

        currentPosOffset.y = Mathf.Lerp(currentPosOffset.y, targetVerticalOffset, Time.deltaTime * verticalSmooth);

        // Apply position
        transform.localPosition = originalLocalPos + currentPosOffset;
    }
}