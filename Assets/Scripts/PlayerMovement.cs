// PlayerMovement
using System;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun
{
    [Header("Assignables")]
    //Assignables
    public Transform playerCam;
    public Transform orientation;
    public Transform CameraCarrier;
    private Collider playerCollider;
    public Rigidbody rb;

    [Space(10)]

    public LayerMask whatIsGround;
    public LayerMask whatIsWallrunnable;

    [Header("MovementSettings")]
    //Movement Settings 
    public float sensitivity = 50f;
    public float moveSpeed = 4500f;
    public float walkSpeed = 20f;
    public float runSpeed = 10f;
    public bool grounded;
    public bool onWall;

    //Private Floats
    private float wallRunGravity = 1f;
    private float maxSlopeAngle = 35f;
    private float wallRunRotation;
    private float slideSlowdown = 0.2f;
    private float actualWallRotation;
    private float wallRotationVel;
    private float desiredX;
    private float xRotation;
    private float sensMultiplier = 1f;
    private float jumpCooldown = 0.25f;
    private float jumpForce = 350f;
    private float x;
    private float y;
    private float vel;

    //Private bools
    private bool readyToJump;
    private bool jumping;
    private bool sprinting;
    private bool crouching;
    private bool wallRunning;
    private bool cancelling;
    private bool readyToWallrun = true;
    private bool airborne;
    private bool onGround;
    private bool surfing;
    private bool cancellingGrounded;
    private bool cancellingWall;
    private bool cancellingSurf;

    //Private Vector3's
    private Vector3 grapplePoint;
    private Vector3 normalVector;
    private Vector3 wallNormalVector;
    private Vector3 wallRunPos;
    private Vector3 previousLookdir;
    private Vector3 originalCamPos;
    private Vector3 originalColCenter;
    private bool hasCachedColCenter = false;
    //Private int
    private int nw;
    private float originalColHeight;

    [Header("Gun Handling - Public Gun")]
    public Transform gunTransform;          // ✅ drag your public gun here
    private Vector3 originalGunPos;         // cached original local position
    private Vector3 crouchGunPos;           // target crouch position
    private Vector3 adsGunPos;              // target ADS position
    public float gunLerpSpeed = 5f;         // speed of lerp
    public float camCrouchLerpSpeed = 10f;  // speed of camera crouch lerp
    private bool isCrouching = false;
    private bool isAiming = false;

    [Header("ADS Settings - Public Gun")]
    public Vector3 adsOffset = new Vector3(0f, -0.2f, 0f);  // X, Y, and Z offset for ADS
    public Vector3 adsCrouchOffset = new Vector3(0f, -0.9f, 0f);  // X, Y, and Z offset for ADS while crouching
    public float adsRoll = 0f;  // Roll rotation for ADS
    public float adsCrouchRoll = 0f;  // Roll rotation for ADS while crouching

    [Header("Gun Handling - Local Gun")]
    public Transform localGunTransform;     // ✅ drag your local gun here
    private Vector3 originalLocalGunPos;    // cached original local position
    private Quaternion originalLocalGunRot; // cached original local rotation
    private Vector3 crouchLocalGunPos;      // target crouch position
    private Vector3 adsLocalGunPos;         // target ADS position

    [Header("ADS Settings - Local Gun")]
    public Vector3 localAdsOffset = new Vector3(0f, -0.2f, 0f);  // X, Y, and Z offset for ADS
    public Vector3 localAdsCrouchOffset = new Vector3(0f, -0.9f, 0f);  // X, Y, and Z offset for ADS while crouching
    public float localAdsRoll = 0f;  // Roll rotation for ADS
    public float localAdsCrouchRoll = 0f;  // Roll rotation for ADS while crouching
    public float localAdsYAW = 0f;

    [Space(10)]
    [Header("ADS Recoil & Sway")]
    public Transform recoilObject;  // ✅ Drag the recoil object here (has GunKickback component)
    public Transform swayObject;    // ✅ Drag the sway object here (has SwayNBobScript component)

    [Header("ADS Movement Sway")]
    public float adsMovementSwayAmount = 0.01f;  // How much the gun sways when moving during ADS
    public float adsMovementSwaySmooth = 10f;    // Smoothness of the movement sway
    private Vector3 adsMovementSwayPos;

    // Original values to restore
    private float originalKickbackDistance;
    private float originalSwayStep;
    private float originalSwayMaxStepDistance;
    private float originalSwayRotationStep;
    private float originalSwayBobFrequency;
    private float originalSwayBobExaggeration;
    private float originalSwayWalkThreshold;
    private Vector3 originalSwayTravelLimit;
    private Vector3 originalSwayBobLimit;
    private Vector3 originalSwayMultiplier;
    private bool hasStoredRecoilSwayValues = false;

    [Space(10)]
    [Header("Model Correction")]
    public Transform modelTransform; // ✅ Drag your player model here

    [Space(10)]
    [Header("FOV Effects")]
    public Camera mainCamera; // ✅ Drag your camera here
    public float slideFOVIncrease = 10f; // How much to increase FOV
    public float wallrunFOVIncrease = 15f; // How much to increase FOV during wallrun
    public float adsFOVDecrease = 10f; // How much to decrease FOV during ADS
    public float slideFOVDuration = 0.3f; // How long the boost lasts
    private float normalFOV;
    private float targetFOV;
    private float fovVelocity;
    private float slideFOVTimer = 0f;

    //Instance
    public static PlayerMovement Instance { get; private set; }

    private void Awake()
    {
        if (photonView.IsMine)
        {
            Instance = this;
            rb = GetComponent<Rigidbody>();
        }
    }

    private void Start()
    {
        Debug.Log($"{gameObject.name} spawned. IsMine: {photonView.IsMine}");
        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
            GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>().enabled = false;
        }
        if (photonView.IsMine)
        {
            playerCollider = GetComponent<Collider>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            readyToJump = true;
            wallNormalVector = Vector3.up;

            if (gunTransform != null)
            {
                originalGunPos = gunTransform.localPosition;
                crouchGunPos = originalGunPos + new Vector3(0f, -0.7f, 0f);
                adsGunPos = originalGunPos + adsOffset;
            }

            if (localGunTransform != null)
            {
                originalLocalGunPos = localGunTransform.localPosition;
                originalLocalGunRot = localGunTransform.localRotation;
                crouchLocalGunPos = originalLocalGunPos + new Vector3(0f, -0.7f, 0f);
                adsLocalGunPos = originalLocalGunPos + localAdsOffset;
            }

            if (mainCamera != null)
            {
                normalFOV = mainCamera.fieldOfView;
                targetFOV = normalFOV;
            }
        }
        originalCamPos = playerCam.localPosition;
        originalColHeight = GetComponent<CapsuleCollider>().height;
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine) return;
        if (GameState.InMenu) return;
        WallRunning();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        if (GameState.InMenu) return;
        Movement();
    }

    private void Update()
    {
        if (GameState.InMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Correct model position for ALL players (local and remote)
        CorrectModelPosition();

        if (!photonView.IsMine) return;
        MyInput();
        Look();
        HandleGunCrouch();
        HandleLocalGunCrouch();
        HandleADSRecoilAndSway();
        HandleADSMovementSway();
        HandleCameraCrouch();
        HandleSlideFOV();
    }

    private void CorrectModelPosition()
    {
        if (modelTransform == null) return;

        // Lock the model to zero position for ALL players
        // This ensures animations don't offset the model incorrectly
        modelTransform.localPosition = Vector3.zero;
    }

    //Player input
    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        crouching = Input.GetKey(KeyCode.LeftShift);
        isAiming = Input.GetMouseButton(1);  // Right click for ADS

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCrouch();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            StopCrouch();
        }
    }

    private void HandleGunCrouch()
    {
        if (gunTransform == null) return;

        Vector3 targetPos;
        float targetRoll;

        // Determine target position and rotation based on states
        if (isAiming && crouching)
        {
            // ADS while crouching - use crouch ADS offset
            targetPos = originalGunPos + adsCrouchOffset;
            targetRoll = adsCrouchRoll;
        }
        else if (isAiming)
        {
            // Just ADS - use normal ADS offset
            targetPos = originalGunPos + adsOffset;
            targetRoll = adsRoll;
        }
        else if (crouching)
        {
            // Just crouching - use crouch position
            targetPos = crouchGunPos;
            targetRoll = 0f;
        }
        else
        {
            // Normal position
            targetPos = originalGunPos;
            targetRoll = 0f;
        }

        gunTransform.localPosition = Vector3.Lerp(
            gunTransform.localPosition,
            targetPos,
            Time.deltaTime * gunLerpSpeed
        );

        // Apply roll rotation
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetRoll);
        gunTransform.localRotation = Quaternion.Lerp(
            gunTransform.localRotation,
            targetRotation,
            Time.deltaTime * gunLerpSpeed
        );
    }

    private void HandleLocalGunCrouch()
    {
        if (localGunTransform == null) return;

        Vector3 targetPos;
        float targetRoll;
        float targetYaw;

        // Determine target position and rotation based on states
        if (isAiming && crouching)
        {
            // ADS while crouching - use crouch ADS offset
            targetPos = originalLocalGunPos + localAdsCrouchOffset;
            targetRoll = localAdsCrouchRoll;
            targetYaw = localAdsYAW;
        }
        else if (isAiming)
        {
            // Just ADS - use normal ADS offset
            targetPos = originalLocalGunPos + localAdsOffset;
            targetRoll = localAdsRoll;
            targetYaw = localAdsYAW;
        }
        else
        {
            // Normal position (no crouch offset for local gun)
            targetPos = originalLocalGunPos;
            targetRoll = 0f;
            targetYaw = 0f;
        }

        localGunTransform.localPosition = Vector3.Lerp(
            localGunTransform.localPosition,
            targetPos,
            Time.deltaTime * gunLerpSpeed
        );

        // Apply roll rotation
        Quaternion targetRotation = originalLocalGunRot * Quaternion.Euler(0f, targetYaw, targetRoll);
        localGunTransform.localRotation = Quaternion.Lerp(
            localGunTransform.localRotation,
            targetRotation,
            Time.deltaTime * gunLerpSpeed
        );
    }

    private void HandleADSRecoilAndSway()
    {
        // Handle Gun Kickback (Recoil Object)
        if (recoilObject != null)
        {
            GunKickback kickbackScript = recoilObject.GetComponent<GunKickback>();

            if (kickbackScript != null)
            {
                // Store original value on first run
                if (!hasStoredRecoilSwayValues)
                {
                    originalKickbackDistance = kickbackScript.kickbackDistance;
                }

                // Apply ADS kickback reduction when aiming
                if (isAiming)
                {
                    kickbackScript.kickbackDistance = 0.02f;
                }
                else
                {
                    kickbackScript.kickbackDistance = originalKickbackDistance;
                }
            }
        }

        // Handle SwayNBobScript (Sway Object)
        if (swayObject != null)
        {
            SwayNBobScript swayScript = swayObject.GetComponent<SwayNBobScript>();

            if (swayScript != null)
            {
                // Store original values on first run
                if (!hasStoredRecoilSwayValues)
                {
                    originalSwayStep = swayScript.step;
                    originalSwayMaxStepDistance = swayScript.maxStepDistance;
                    originalSwayRotationStep = swayScript.rotationStep;
                    originalSwayBobFrequency = swayScript.bobFrequency;
                    originalSwayBobExaggeration = swayScript.bobExaggeration;
                    originalSwayWalkThreshold = swayScript.walkThreshold;
                    originalSwayTravelLimit = swayScript.travelLimit;
                    originalSwayBobLimit = swayScript.bobLimit;
                    originalSwayMultiplier = swayScript.multiplier;
                    hasStoredRecoilSwayValues = true;
                }

                // Apply ADS sway changes when aiming
                if (isAiming)
                {
                    swayScript.step = 0f;
                    swayScript.maxStepDistance = 0f;
                    swayScript.rotationStep = 0.15f;
                    swayScript.bobFrequency = 0f;
                    swayScript.bobExaggeration = 0f;
                    swayScript.walkThreshold = 999f;
                    swayScript.travelLimit = Vector3.zero;
                    swayScript.bobLimit = Vector3.zero;
                    swayScript.multiplier = Vector3.zero;
                }
                else
                {
                    swayScript.step = originalSwayStep;
                    swayScript.maxStepDistance = originalSwayMaxStepDistance;
                    swayScript.rotationStep = originalSwayRotationStep;
                    swayScript.bobFrequency = originalSwayBobFrequency;
                    swayScript.bobExaggeration = originalSwayBobExaggeration;
                    swayScript.walkThreshold = originalSwayWalkThreshold;
                    swayScript.travelLimit = originalSwayTravelLimit;
                    swayScript.bobLimit = originalSwayBobLimit;
                    swayScript.multiplier = originalSwayMultiplier;
                }
            }
        }
    }

    private void HandleADSMovementSway()
    {
        if (swayObject == null) return;

        // Only apply custom movement sway when aiming
        if (isAiming)
        {
            // Create symmetrical sway based on movement input
            // Negative values so gun moves opposite to movement direction (feels more natural)
            Vector3 targetSway = new Vector3(-x * adsMovementSwayAmount, -y * adsMovementSwayAmount * 0.5f, 0f);

            // Smoothly lerp to target
            adsMovementSwayPos = Vector3.Lerp(adsMovementSwayPos, targetSway, Time.deltaTime * adsMovementSwaySmooth);

            // Apply the sway to the sway object's local position (additive to existing position)
            swayObject.localPosition = swayObject.GetComponent<SwayNBobScript>().transform.localPosition + adsMovementSwayPos;
        }
        else
        {
            // Reset sway when not aiming
            adsMovementSwayPos = Vector3.Lerp(adsMovementSwayPos, Vector3.zero, Time.deltaTime * adsMovementSwaySmooth);
        }
    }

    private void HandleSlideFOV()
    {
        if (mainCamera == null) return;

        // Priority order: ADS > Wallrun > Slide > Normal
        if (isAiming)
        {
            // ADS FOV - reduce FOV for zoom effect
            targetFOV = normalFOV - adsFOVDecrease;
        }
        else if (wallRunning)
        {
            // Wallrun FOV takes priority over slide FOV
            targetFOV = normalFOV + wallrunFOVIncrease;
        }
        // Count down the slide timer
        else if (slideFOVTimer > 0f)
        {
            slideFOVTimer -= Time.deltaTime;

            // Lerp between boosted and normal FOV based on timer
            float t = slideFOVTimer / slideFOVDuration;
            targetFOV = Mathf.Lerp(normalFOV, normalFOV + slideFOVIncrease, t);
        }
        else
        {
            targetFOV = normalFOV;
        }

        // Smoothly lerp FOV to target
        mainCamera.fieldOfView = Mathf.SmoothDamp(
            mainCamera.fieldOfView,
            targetFOV,
            ref fovVelocity,
            0.1f
        );
    }

    private void HandleCameraCrouch()
    {
        // Smoothly lerp camera position based on crouch state
        Vector3 targetCamPos = crouching ? originalCamPos + new Vector3(0f, -0.6f, 0f) : originalCamPos;
        playerCam.localPosition = Vector3.Lerp(playerCam.localPosition, targetCamPos, Time.deltaTime * camCrouchLerpSpeed);
    }

    private void StartCrouch()
    {
        // Camera position is now handled by HandleCameraCrouch() with lerping

        CapsuleCollider col = GetComponent<CapsuleCollider>();

        // Cache original collider values
        if (!hasCachedColCenter)
        {
            originalColCenter = col.center;
            hasCachedColCenter = true;
        }

        col.height = 1.75f;
        col.center = new Vector3(col.center.x, col.center.y - 0.25f, col.center.z);

        Vector3 horizontalVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        float horizontalSpeed = horizontalVel.magnitude;
        if (horizontalSpeed > 0.5f && grounded)
        {
            rb.AddForce(orientation.transform.forward * 400f);
            slideFOVTimer = slideFOVDuration;
        }
    }

    //Scale player to original size
    private void StopCrouch()
    {
        isCrouching = false;

        // Camera position is now handled by HandleCameraCrouch() with lerping
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        col.height = originalColHeight;
        col.center = originalColCenter;
        slideFOVTimer = 0f;
    }

    //Moving around with WASD
    private void Movement()
    {
        rb.AddForce(Vector3.down * Time.deltaTime * 10f);
        Vector2 mag = FindVelRelativeToLook();
        float num = mag.x;
        float num2 = mag.y;
        CounterMovement(x, y, mag);
        if (readyToJump && jumping)
        {
            Jump();
        }
        float num3 = walkSpeed;
        if (sprinting)
        {
            num3 = runSpeed;
        }
        if (crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000f);
            return;
        }
        if (x > 0f && num > num3)
        {
            x = 0f;
        }
        if (x < 0f && num < 0f - num3)
        {
            x = 0f;
        }
        if (y > 0f && num2 > num3)
        {
            y = 0f;
        }
        if (y < 0f && num2 < 0f - num3)
        {
            y = 0f;
        }
        float num4 = 1f;
        float num5 = 1f;
        if (!grounded)
        {
            num4 = 0.8f;
            num5 = 0.8f;
        }
        if (grounded && crouching)
        {
            num5 = 0f;
        }
        if (wallRunning)
        {
            num5 = 0.3f;
            num4 = 0.3f;
        }
        if (surfing)
        {
            num4 = 0.7f;
            num5 = 0.3f;
        }

        Vector3 moveDir = (orientation.transform.forward * y + orientation.transform.right * x).normalized;
        rb.AddForce(moveDir * moveSpeed * Time.deltaTime * num4 * num5);
    }

    //Ready to jump again
    private void ResetJump()
    {
        readyToJump = true;
    }

    //Player go fly
    private void Jump()
    {
        if ((grounded || wallRunning || surfing) && readyToJump)
        {
            MonoBehaviour.print("jumping");
            Vector3 velocity = rb.velocity;
            readyToJump = false;
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);
            if (rb.velocity.y < 0.5f)
            {
                rb.velocity = new Vector3(velocity.x, 0f, velocity.z);
            }
            else if (rb.velocity.y > 0f)
            {
                rb.velocity = new Vector3(velocity.x, velocity.y / 2f, velocity.z);
            }
            if (wallRunning)
            {
                rb.AddForce(wallNormalVector * jumpForce * 3f);
            }
            Invoke("ResetJump", jumpCooldown);
            if (wallRunning)
            {
                wallRunning = false;
            }
        }
    }

    //Looking around by using your mouse
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * 5 * sensitivity * Time.deltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * 5 * sensitivity * Time.deltaTime * sensMultiplier;

        desiredX += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        FindWallRunRotation();
        actualWallRotation = Mathf.SmoothDamp(actualWallRotation, wallRunRotation, ref wallRotationVel, 0.2f);

        playerCam.localRotation = Quaternion.Euler(xRotation, desiredX, actualWallRotation);
        orientation.localRotation = Quaternion.Euler(0f, desiredX, 0f);
    }

    //Make the player movement feel good 
    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping)
        {
            return;
        }
        float num = 0.4f;
        float num2 = 0.01f;
        if (crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideSlowdown);
            return;
        }
        if ((Math.Abs(mag.x) > num2 && Math.Abs(x) < 0.05f) || (mag.x < 0f - num2 && x > 0f) || (mag.x > num2 && x < 0f))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * (0f - mag.x) * num);
        }
        if ((Math.Abs(mag.y) > num2 && Math.Abs(y) < 0.05f) || (mag.y < 0f - num2 && y > 0f) || (mag.y > num2 && y < 0f))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * (0f - mag.y) * num);
        }
        if (Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2f) + Mathf.Pow(rb.velocity.z, 2f)) > walkSpeed)
        {
            float num3 = rb.velocity.y;
            Vector3 vector = rb.velocity.normalized * walkSpeed;
            rb.velocity = new Vector3(vector.x, num3, vector.z);
        }
    }

    public Vector2 FindVelRelativeToLook()
    {
        float current = orientation.transform.eulerAngles.y;
        float target = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * 57.29578f;
        float num = Mathf.DeltaAngle(current, target);
        float num2 = 90f - num;
        float magnitude = rb.velocity.magnitude;
        return new Vector2(y: magnitude * Mathf.Cos(num * ((float)Math.PI / 180f)), x: magnitude * Mathf.Cos(num2 * ((float)Math.PI / 180f)));
    }

    private void FindWallRunRotation()
    {
        if (!wallRunning)
        {
            wallRunRotation = 0f;
            return;
        }
        _ = new Vector3(0f, playerCam.transform.rotation.y, 0f).normalized;
        new Vector3(0f, 0f, 1f);
        float num = 0f;
        float current = playerCam.transform.rotation.eulerAngles.y;
        if (Math.Abs(wallNormalVector.x - 1f) < 0.1f)
        {
            num = 90f;
        }
        else if (Math.Abs(wallNormalVector.x - -1f) < 0.1f)
        {
            num = 270f;
        }
        else if (Math.Abs(wallNormalVector.z - 1f) < 0.1f)
        {
            num = 0f;
        }
        else if (Math.Abs(wallNormalVector.z - -1f) < 0.1f)
        {
            num = 180f;
        }
        num = Vector3.SignedAngle(new Vector3(0f, 0f, 1f), wallNormalVector, Vector3.up);
        float num2 = Mathf.DeltaAngle(current, num);
        wallRunRotation = (0f - num2 / 90f) * 15f;
        if (!readyToWallrun)
        {
            return;
        }
        if ((Mathf.Abs(wallRunRotation) < 4f && y > 0f && Math.Abs(x) < 0.1f) || (Mathf.Abs(wallRunRotation) > 22f && y < 0f && Math.Abs(x) < 0.1f))
        {
            if (!cancelling)
            {
                cancelling = true;
                CancelInvoke("CancelWallrun");
                Invoke("CancelWallrun", 0.2f);
            }
        }
        else
        {
            cancelling = false;
            CancelInvoke("CancelWallrun");
        }
    }

    private void CancelWallrun()
    {
        MonoBehaviour.print("cancelled");
        Invoke("GetReadyToWallrun", 0.1f);
        rb.AddForce(wallNormalVector * 600f);
        readyToWallrun = false;
    }

    private void GetReadyToWallrun()
    {
        readyToWallrun = true;
    }

    private void WallRunning()
    {
        if (wallRunning)
        {
            rb.AddForce(-wallNormalVector * Time.deltaTime * moveSpeed);
            rb.AddForce(Vector3.up * Time.deltaTime * rb.mass * 100f * wallRunGravity);
        }
    }

    private bool IsFloor(Vector3 v)
    {
        return Vector3.Angle(Vector3.up, v) < maxSlopeAngle;
    }

    private bool IsSurf(Vector3 v)
    {
        float num = Vector3.Angle(Vector3.up, v);
        if (num < 89f)
        {
            return num > maxSlopeAngle;
        }
        return false;
    }

    private bool IsWall(Vector3 v)
    {
        return Math.Abs(90f - Vector3.Angle(Vector3.up, v)) < 0.1f;
    }

    private bool IsRoof(Vector3 v)
    {
        return v.y == -1f;
    }

    private void StartWallRun(Vector3 normal)
    {
        if (!grounded && readyToWallrun)
        {
            wallNormalVector = normal;
            float num = 14f;
            if (!wallRunning)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.up * num, ForceMode.Impulse);
            }
            wallRunning = true;
        }
    }

    private void OnCollisionStay(Collision other)
    {
        int layer = other.gameObject.layer;
        if ((int)whatIsGround != ((int)whatIsGround | (1 << layer)))
        {
            return;
        }
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            if (IsFloor(normal))
            {
                if (wallRunning)
                {
                    wallRunning = false;
                }
                grounded = true;
                normalVector = normal;
                cancellingGrounded = false;
                CancelInvoke("StopGrounded");
            }
            if (IsWall(normal) && layer == LayerMask.NameToLayer("Ground"))
            {
                StartWallRun(normal);
                onWall = true;
                cancellingWall = false;
                CancelInvoke("StopWall");
            }
            if (IsSurf(normal))
            {
                surfing = true;
                cancellingSurf = false;
                CancelInvoke("StopSurf");
            }
            IsRoof(normal);
        }
        float num = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke("StopGrounded", Time.deltaTime * num);
        }
        if (!cancellingWall)
        {
            cancellingWall = true;
            Invoke("StopWall", Time.deltaTime * num);
        }
        if (!cancellingSurf)
        {
            cancellingSurf = true;
            Invoke("StopSurf", Time.deltaTime * num);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }

    private void StopWall()
    {
        onWall = false;
        wallRunning = false;
    }

    private void StopSurf()
    {
        surfing = false;
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    public bool GetWall()
    {
        return onWall;
    }

    public bool GetRunning()
    {
        return wallRunning;
    }

    public float GetFallSpeed()
    {
        return rb.velocity.y;
    }

    public Collider GetPlayerCollider()
    {
        return playerCollider;
    }

    public Transform GetPlayerCamTransform()
    {
        return playerCam.transform;
    }

    public bool IsCrouching()
    {
        return crouching;
    }

    public Rigidbody GetRb()
    {
        return rb;
    }

    public Vector2 GetInput()
    {
        Vector3 input = new Vector3(x, 0, y);
        input = Vector3.ClampMagnitude(input, 1f);
        return new Vector2(input.x, input.z);
    }

    public bool Getgrounded()
    {
        return grounded;
    }
}