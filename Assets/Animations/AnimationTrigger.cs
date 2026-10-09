using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Animations.Rigging;
public class AnimationTrigger : MonoBehaviourPun
{
    public PlayerMovement playerMovement;
    [Header("References")]
    private Animator anim;
    public Rigidbody rb;
    public GameObject chestIKObject;
    private MultiAimConstraint chestIK;
    [Header("Animation Settings")]
    public float smoothTime = 0.1f;
    public float crouchSpeed = 12f;
    private float currentMoveX;
    private float currentMoveY;
    private float velocityX;
    private float velocityY;
    private float smoothHorizon = 0f;
    public float horizonSmoothSpeed = 10f;
    [Header("Weapon Settings")]
    public Transform pistolObject;          // 🔫 Dra in pistolen här i Inspector
    public Vector3 pistolCrouchOffset = new Vector3(0f, -1f, 0f); // Flytta ner med -1 på Y
    private Vector3 pistolDefaultPos;


    [Header("Jump Settings")]
    public float fallThreshold = -0.1f; // velocity below this = falling
    public float landThreshold = 0.1f;  // velocity near 0 = landed

    private bool isCrouching = false;
    private float crouchValue = 0f;
    public float chestIKStandingWeight = 0.5f;
    void Start()
    {
        anim = GetComponent<Animator>();
        chestIK = chestIKObject.GetComponent<MultiAimConstraint>();

        if (pistolObject != null)
            pistolDefaultPos = pistolObject.localPosition;
    }

    void LateUpdate()
    {
        if (!photonView.IsMine) return;
        HandleJumpAnimations();
            if (Input.GetKeyDown(KeyCode.Space)) {
            anim.SetTrigger("JumpTrigger"); 
        }
        bool grounded = PlayerMovement.Instance.Getgrounded();

        HandleInput();
        SmoothCrouchBlend();
        UpdateChestIK();
        if (grounded)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                anim.SetBool("Crouch", true);
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                anim.SetBool("Crouch", false);
            }
        }

      
        // --- Get local velocity from Rigidbody ---
        Vector2 moveInput = PlayerMovement.Instance.GetInput(); // We'll add this function

    

        // --- Smooth transitions ---
        currentMoveX = Mathf.Lerp(currentMoveX, moveInput.x, Time.deltaTime * 10f);
        currentMoveY = Mathf.Lerp(currentMoveY, moveInput.y, Time.deltaTime * 10f);

        // --- Apply to animator ---
        anim.SetFloat("MoveX", currentMoveX);
        anim.SetFloat("MoveY", currentMoveY);


        

    }


    void HandleInput()
    {
        Vector2 moveInput = PlayerMovement.Instance.GetInput();
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isCrouching = true;

           
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isCrouching = false;
        }
        Vector3 Speed = PlayerMovement.Instance.GetVelocity();
        Vector3 Xvector = new Vector3(Speed.x, 0f, Speed.z);
        float horizon = Xvector.magnitude;
        if (horizon > 1) horizon = 1;
        smoothHorizon = Mathf.Lerp(smoothHorizon, horizon, Time.deltaTime * horizonSmoothSpeed);

        if(smoothHorizon > 0.95)
        {
            smoothHorizon = 1;
        } else if(smoothHorizon < 0.05)
        {
            smoothHorizon = 0;
        }

            anim.SetFloat("SlideX", smoothHorizon);
        Debug.Log("smoothHorizon: " + smoothHorizon);
      
    }
    void SmoothCrouchBlend()
    {
        float target = isCrouching ? 1f : 0f;

        // Smooth interpolation
        crouchValue = Mathf.Lerp(crouchValue, target, Time.deltaTime * crouchSpeed);
        if(crouchValue < 0.05)
        {
            crouchValue = 0;
        }
        Debug.Log("Crouch" + crouchValue);
        anim.SetFloat("Crouch", crouchValue);

        if (pistolObject == null) return;
       
        Vector3 targetPos = isCrouching ? pistolDefaultPos + pistolCrouchOffset : pistolDefaultPos;
        pistolObject.localPosition = Vector3.Lerp(pistolObject.localPosition, targetPos, Time.deltaTime * crouchSpeed);

    }

    private void UpdateChestIK()
    {
        float target = isCrouching ? 0.4f : chestIKStandingWeight;
        chestIK.weight = Mathf.Lerp(chestIK.weight, target, Time.deltaTime * crouchSpeed);
    }

    void HandleJumpAnimations()
    {

        if (isCrouching)
        {
            anim.SetBool("JumpUp", false);
            anim.SetBool("Falling", false);
            return;
        }
        bool onWall = PlayerMovement.Instance.GetWall();
        bool wallRunning = PlayerMovement.Instance.GetRunning();
        // ✅ If we're wallrunning or touching a wall, skip jumping/falling anims
        if (onWall || wallRunning)
        {
            anim.SetBool("JumpUp", false);
            anim.SetBool("Falling", false);
            return;
        }
        float falling = PlayerMovement.Instance.GetFallSpeed();

        float verticalVel = rb.velocity.y;

        // 🟢 Jumping upwards
        if (falling > 0.1f)
        {
            anim.SetBool("JumpUp", true);
            anim.SetBool("Falling", false);
        }
        // 🔻 Falling downwards
        else if (falling < fallThreshold)
        {
            anim.SetBool("JumpUp", false);
            anim.SetBool("Falling", true);
        }
        // ✅ Landed (velocity ~ 0)
        else if (falling > -landThreshold && falling < landThreshold)
        {
            anim.SetBool("JumpUp", false);
            anim.SetBool("Falling", false);
        }
    }
}