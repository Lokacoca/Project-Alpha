using UnityEngine;
using Photon.Pun;

public class Gun_Anchor : MonoBehaviourPun
{
    public Transform playerCam;
    private float desiredX;
    private float xRotation;
    private float xTotal;
    private float sensMultiplier = 1f;
    public float sensitivity = 50f;
    [HideInInspector] public float reloadOffset = 0f;
    void Start()
    {
        if (playerCam != null)
        {
            Camera cam = playerCam.GetComponent<Camera>();
            if (cam != null)
            {
                cam.enabled = false;               // disables rendering
                cam.cullingMask = 0;               // hides everything
                cam.clearFlags = CameraClearFlags.Nothing;
            }
        }

    }



    void Update()
    {
        if (!photonView.IsMine) return;
        Look();
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * 5 * sensitivity * Time.deltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * 5 * sensitivity * Time.deltaTime * sensMultiplier;

        desiredX += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        xTotal = xRotation + reloadOffset;
        xTotal = Mathf.Clamp(xTotal, -90f, 90f);

        // ✅ apply reload offset additively
        playerCam.localRotation = Quaternion.Euler(xTotal, 0, 0);
    }
}
