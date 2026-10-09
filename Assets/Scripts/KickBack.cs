using UnityEngine;
public class GunKickback : MonoBehaviour
{
    [Header("Kickback Settings")]
    public float kickbackDistance = 0.05f; // Changed to public - how far back the gun moves
    public float kickbackSpeed = 25f;      // how fast it kicks back
    public float returnSpeed = 10f;        // how fast it returns
    private Vector3 originalLocalPos;
    private Vector3 currentOffset;
    private Vector3 targetOffset;
    private Gun gunScript;
    void Start()
    {
        originalLocalPos = transform.localPosition;
        gunScript = GetComponentInParent<Gun>();
    }
    void Update()
    {
        if (gunScript != null && gunScript.isReloading) return;
        // Smoothly move towards target offset
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * kickbackSpeed);
        // Apply the offset
        transform.localPosition = originalLocalPos + currentOffset;
        // Gradually return to zero (rest position)
        targetOffset = Vector3.Lerp(targetOffset, Vector3.zero, Time.deltaTime * returnSpeed);
    }
    // Call this from your shooting script when firing
    public void AddKickback()
    {
        if (gunScript != null && gunScript.isReloading) return;
        // Moves slightly backward in local space (Z axis)
        targetOffset -= Vector3.forward * kickbackDistance;
    }
}