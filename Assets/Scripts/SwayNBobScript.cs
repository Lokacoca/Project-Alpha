using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SwayNBobScript : MonoBehaviourPun
{
    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckDistance = 0.2f;
    public Transform groundCheckPoint; // ideally a child at the player's feet

    [Header("Sway")]
    public float step = 0.02f;
    public float maxStepDistance = 0.06f;
    Vector3 swayPos;

    [Header("Sway Rotation")]
    public float rotationStep = 4f;
    public float maxRotationStep = 5f;
    Vector3 swayEulerRot;

    [Header("Smoothing")]
    public float smooth = 10f;
    public float smoothRot = 12f;

    [Header("Bobbing")]
    private float speedCurve = 0f;                       // bob phase
    [Tooltip("How fast speedCurve advances when moving")]
    public float bobFrequency = 6f;                      // frequency multiplier
    public float bobExaggeration = 1.0f;                 // scales how fast it advances with input
    public float walkThreshold = 0.05f;                  // magnitude below which we treat as "not moving"

    [Tooltip("Position amplitude when walking")]
    public Vector3 travelLimit = new Vector3(0.02f, 0.02f, 0.02f);
    [Tooltip("Additional bob offsets")]
    public Vector3 bobLimit = new Vector3(0.01f, 0.01f, 0.01f);

    Vector3 bobPosition;
    float CurveSin => Mathf.Sin(speedCurve);
    float CurveCos => Mathf.Cos(speedCurve);

    [Header("Bob Rotation")]
    public Vector3 multiplier = new Vector3(1f, 1f, 1f);
    Vector3 bobEulerRotation;

    // internal
    Vector3 originalLocalPos;
    Quaternion originalLocalRot;

    Vector2 walkInput;
    Vector2 lookInput;
    private bool grounded;

    void Start()
    {
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
    }

    void Update()
    {
        // Only process input and movement for local player
        if (!photonView.IsMine) return;

        GetInput();
        GroundCheck();
        Sway();
        SwayRotation();
        BobOffset();
        BobRotation();
    }

    void LateUpdate()
    {
        // Only apply effects for local player
        if (!photonView.IsMine) return;

        CompositePositionRotation();
    }

    void GetInput()
    {
        walkInput.x = Input.GetAxis("Horizontal");
        walkInput.y = Input.GetAxis("Vertical");
        walkInput = walkInput.normalized;

        lookInput.x = Input.GetAxis("Mouse X");
        lookInput.y = Input.GetAxis("Mouse Y");
    }

    void GroundCheck()
    {
        if (groundCheckPoint == null)
        {
            // if not assigned, assume grounded to avoid weird behavior
            grounded = true;
            return;
        }
        grounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckDistance, groundMask);
    }

    void Sway()
    {
        Vector3 invertLook = new Vector3(-lookInput.x * step, -lookInput.y * step, 0f);
        invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);
        swayPos = invertLook;
    }

    void SwayRotation()
    {
        float rotX = -lookInput.y * rotationStep;
        float rotY = lookInput.x * rotationStep;
        float rotZ = -lookInput.x * rotationStep;

        swayEulerRot = new Vector3(
            Mathf.Clamp(rotX, -maxRotationStep, maxRotationStep),
            Mathf.Clamp(rotY, -maxRotationStep, maxRotationStep),
            Mathf.Clamp(rotZ, -maxRotationStep, maxRotationStep)
        );
    }

    void CompositePositionRotation()
    {
        Vector3 targetPos = originalLocalPos + swayPos + bobPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smooth);

        Quaternion swayRot = Quaternion.Euler(swayEulerRot);
        Quaternion bobRot = Quaternion.Euler(bobEulerRotation);

        // ✅ sway first, then bob
        Quaternion targetRot = originalLocalRot * swayRot * bobRot;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * smoothRot);
    }

    void BobOffset()
    {
        float moveMag = walkInput.magnitude;

        // Advance speedCurve only when moving and grounded, otherwise decay it
        if (grounded && moveMag > walkThreshold)
        {
            // increase phase; bobFrequency controls base speed, bobExaggeration scales by input
            speedCurve += Time.deltaTime * bobFrequency * (moveMag * bobExaggeration);
        }
        else
        {
            // decay the phase and/or slowly move it toward zero to stop movement
            speedCurve = Mathf.Lerp(speedCurve, 0f, Time.deltaTime * 4f);
        }

        // produce bob only when moving enough and grounded
        if (grounded && moveMag > walkThreshold)
        {
            bobPosition.x = CurveCos * bobLimit.x - (walkInput.x * travelLimit.x);
            bobPosition.y = CurveSin * bobLimit.y - (walkInput.y * travelLimit.y);
            bobPosition.z = -(walkInput.y * travelLimit.z);
        }
        else
        {
            bobPosition = Vector3.zero;
        }

        // clamp final bob to safe ranges (prevents huge offsets)
        bobPosition = new Vector3(
            Mathf.Clamp(bobPosition.x, -travelLimit.x * 2f, travelLimit.x * 2f),
            Mathf.Clamp(bobPosition.y, -travelLimit.y * 2f, travelLimit.y * 2f),
            Mathf.Clamp(bobPosition.z, -travelLimit.z * 2f, travelLimit.z * 2f)
        );
    }

    void BobRotation()
    {
        float moveMag = walkInput.magnitude;
        if (moveMag > walkThreshold)
        {
            bobEulerRotation.x = multiplier.x * Mathf.Sin(2f * speedCurve);
            bobEulerRotation.y = multiplier.y * CurveCos;
            bobEulerRotation.z = multiplier.z * CurveCos * walkInput.x;
        }
        else
        {
            bobEulerRotation = Vector3.zero;
        }
    }
}