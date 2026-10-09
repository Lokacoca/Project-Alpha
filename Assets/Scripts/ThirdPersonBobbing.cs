using UnityEngine;

public class GunSway : MonoBehaviour
{
    public Transform playerCam;
    [Range(0f, 1f)] public float yawMultiplier = 0.3f;  // 👈 Reduce sideways rotation (Y)
    [Range(0f, 1f)] public float pitchMultiplier = 1f;  // 👈 Keep full up/down if you want
    [Range(0f, 1f)] public float rollMultiplier = 1f;   // 👈 Optional: control Z
    public float lerpSpeed = 10f;

    private Quaternion initialLocalRotation;

    void Start()
    {
        // Store initial local rotation to keep offsets
        initialLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        // Get camera rotation in world space
        Vector3 euler = transform.rotation.eulerAngles; // Keep current X
        euler.y = playerCam.rotation.eulerAngles.y;     // Only update Y
        euler.z = playerCam.rotation.eulerAngles.z;     // Optional Z

        // Convert to Quaternion
        Quaternion targetRotation = Quaternion.Euler(euler);

        // Lerp smoothly
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * lerpSpeed*10);
    }
}