using UnityEngine;
using Photon.Pun;
public class Recoil : MonoBehaviourPun
{
    // Rotations
    private Vector3 currentRotation;
    private Vector3 targetRotation;
    private Quaternion initialRotation;
    // Hipfire Recoil
    public float recoilX;  // Changed to public
    public float recoilY;  // Changed to public
    [SerializeField] private float recoilZ;
    // Settings
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;
    private Gun gunScript;
    void Start()
    {
        // Store the original local rotation so recoil is applied relative to it
        initialRotation = transform.localRotation;
        gunScript = GetComponentInParent<Gun>();
        currentRotation = transform.localRotation.eulerAngles;
        targetRotation = transform.localRotation.eulerAngles;
    }
    void Update()
    {
        if (gunScript != null && gunScript.isReloading) return;
        if (!photonView.IsMine) return;
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        // Smooth return to rest
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
        // Apply recoil relative to starting orientation
        transform.localRotation = initialRotation * Quaternion.Euler(currentRotation);
    }
    // call this when firing to add recoil
    public void RecoilFire()
    {
        if (gunScript != null && gunScript.isReloading) return;
        if (!photonView.IsMine) return;
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
        Debug.Log("RECOIL INSTANSIATED");
    }
}