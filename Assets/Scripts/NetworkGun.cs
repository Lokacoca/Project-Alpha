using Photon.Pun;
using UnityEngine;

public class NetworkGun : MonoBehaviourPun
{
    [Header("References")]
    public ParticleSystem muzzleFlash;
    public GameObject hitEffectPrefab;
    public GameObject bulletTracerPrefab;
    public Transform muzzlePoint;

    // Called remotely when the local player fires
    [PunRPC]
    public void RPC_PlayShotEffects(Vector3 start, Vector3 end)
    {
        // Muzzle flash
        if (muzzleFlash != null)
            muzzleFlash.Play();

        // Impact effect (only if we hit something)
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, end, Quaternion.LookRotation(end - start));
        }

        // Tracer
        if (bulletTracerPrefab != null)
        {
            GameObject tracer = Instantiate(bulletTracerPrefab);
            tracer.GetComponent<BulletTracer>().Init(start, end);
        }
    }
}
