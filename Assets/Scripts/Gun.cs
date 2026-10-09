using Photon.Pun;
using System.Collections;
using UnityEngine;
using TMPro;

public class Gun : MonoBehaviourPun
{
    [Header("General")]
    public Camera fpsCamera;          // drag FPS camera here
    public Gun_Anchor gunAnchor;      // drag the anchor here in Inspector
    public Transform cameraLerp;

    public float damage = 25f;
    public float range = 100f;
    public LayerMask hitMask = ~0;

    [Header("Fire")]
    public float fireRate = 20f;
    public bool isAutomatic = true;
    public float spreadAngle = 0.5f;

    [Header("Ammo")]
    public int magazineSize = 30;
    public float reloadTime = 1.8f;
    public bool isReloading = false;
    public int ammoInMag;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public AudioSource shotSound;
    public GameObject hitEffectPrefab;
    public GameObject bulletHolePrefab;
    public Transform muzzlePoint;
    public GameObject bulletTracerPrefab;

    [Header("UI")]
    public TextMeshProUGUI AmmoCount;

    [Header("Recoil")]
    [SerializeField] private Transform gunTransform;
    [SerializeField] private Transform PUBLICgunTransform;   // ✅ restored
    [SerializeField] private Transform CAMERAgunTransform;
    private Recoil recoil;
    private GunKickback kickBack;
    private GunKickback PUBLICkickBack;
    private Recoil CAMERArecoil;
    private Recoil PUBLICrecoil;

    private float nextTimeToFire = 0f;
    private PhotonView photonView;

    [Header("Reload smoothing")]
    public float reloadRotateSpeed = 5f;
    public float reloadPitchTarget = -40f; // target vertical angle for anchor during reload

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        kickBack = gunTransform.GetComponent<GunKickback>();
        PUBLICkickBack = PUBLICgunTransform.GetComponent<GunKickback>();

        recoil = gunTransform.GetComponent<Recoil>();
        CAMERArecoil = CAMERAgunTransform.GetComponent<Recoil>();
        PUBLICrecoil = PUBLICgunTransform.GetComponent<Recoil>();

        if (fpsCamera == null) fpsCamera = Camera.main;
        ammoInMag = magazineSize;

        if (!photonView.IsMine && AmmoCount != null)
            AmmoCount.gameObject.SetActive(false);

        UpdateAmmoUI();
    }


    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.R) && ammoInMag < magazineSize)
        {
            StartCoroutine(Reload());
            return;
        }

        bool trigger = isAutomatic ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        if (trigger && Time.time >= nextTimeToFire && ammoInMag > 0)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }

        if (ammoInMag <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        ammoInMag--;
        UpdateAmmoUI();

        // Recoil & kickback
        if (recoil != null) recoil.RecoilFire();
        if (kickBack != null) kickBack.AddKickback();
        if (PUBLICrecoil != null) PUBLICrecoil.RecoilFire();
        if(PUBLICkickBack != null) PUBLICkickBack.AddKickback();
        if (CAMERArecoil != null) CAMERArecoil.RecoilFire();

        // Visuals & sound
        if (muzzleFlash != null) muzzleFlash.Play();
        if (shotSound != null) shotSound.Play();

        // Bullet direction with random spread
        Vector3 shootDir = fpsCamera.transform.forward;
        if (spreadAngle > 0f)
        {
            float spreadRad = Mathf.Tan(spreadAngle * Mathf.Deg2Rad);
            shootDir += new Vector3(
                Random.Range(-spreadRad, spreadRad),
                Random.Range(-spreadRad, spreadRad),
                Random.Range(-spreadRad, spreadRad)
            );
        }

        RaycastHit[] hits = Physics.RaycastAll(fpsCamera.transform.position, shootDir, range, hitMask);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.root == transform.root) continue;

            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                PhotonView targetView = health.GetComponent<PhotonView>();
                if (targetView != null)
                {
                    targetView.RPC("RPC_TakeDamage", targetView.Owner,
                        Mathf.RoundToInt(damage), photonView.Owner.ActorNumber);
                }
            }

            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));

            if (bulletHolePrefab != null)
            {
                GameObject hole = Instantiate(
                    bulletHolePrefab,
                    hit.point + hit.normal * 0.001f, 
                    Quaternion.LookRotation(-hit.normal) 
                );
                hole.transform.SetParent(hit.collider.transform);
            }

            Vector3 start = muzzlePoint.position;
            Vector3 end = hit.point;
            if (bulletTracerPrefab != null)
            {
                GameObject tracer = Instantiate(bulletTracerPrefab);
                tracer.GetComponent<BulletTracer>().Init(start, end);
            }

            if (photonView != null)
                photonView.RPC(nameof(RPC_PlayShotEffects), RpcTarget.Others, start, end);

            break; 
        }
    }

    [PunRPC]
    void RPC_PlayShotEffects(Vector3 start, Vector3 end)
    {
        if (muzzleFlash != null) muzzleFlash.Play();
        if (bulletTracerPrefab != null)
        {
            GameObject tracer = Instantiate(bulletTracerPrefab);
            tracer.GetComponent<BulletTracer>().Init(start, end);
        }
    }
    IEnumerator Reload()
    {
        if (isReloading) yield break;
        isReloading = true;

        // Disable recoil/kickback
        if (kickBack != null) kickBack.enabled = false;
        if (recoil != null) recoil.enabled = false;
        if (PUBLICkickBack != null) PUBLICkickBack.enabled = false;
        if (PUBLICrecoil != null) PUBLICrecoil.enabled = false;
        if (CAMERArecoil != null) CAMERArecoil.enabled = false;

        float half = reloadTime / 2f;
        float elapsed = 0f;

        // Cache original local gun rotation
        Quaternion originalGunRot = gunTransform.localRotation;
        Quaternion reloadGunRot = originalGunRot * Quaternion.Euler(30f, 0f, 0f); // ✅ dip down 30°

        // Phase 1: animate anchor pitch offset down to -40° and gun down to 30°
        while (elapsed < half)
        {
            if (gunAnchor != null)
            {
                gunAnchor.reloadOffset = Mathf.Lerp(gunAnchor.reloadOffset, reloadPitchTarget, Time.deltaTime * reloadRotateSpeed);
            }

            if (gunTransform != null)
            {
                gunTransform.localRotation = Quaternion.Slerp(gunTransform.localRotation, reloadGunRot, Time.deltaTime * reloadRotateSpeed);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        ammoInMag = magazineSize;
        UpdateAmmoUI();

        // Phase 2: animate anchor offset back to 0 and gun back to original rotation
        elapsed = 0f;
        while (elapsed < half)
        {
            if (gunAnchor != null)
            {
                gunAnchor.reloadOffset = Mathf.Lerp(gunAnchor.reloadOffset, 0f, Time.deltaTime * reloadRotateSpeed);
            }

            if (gunTransform != null)
            {
                gunTransform.localRotation = Quaternion.Slerp(gunTransform.localRotation, originalGunRot, Time.deltaTime * reloadRotateSpeed);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Re-enable recoil/kickback
        if (kickBack != null) kickBack.enabled = true;
        if (recoil != null) recoil.enabled = true;
        if (PUBLICkickBack != null) PUBLICkickBack.enabled = true;
        if (PUBLICrecoil != null) PUBLICrecoil.enabled = true;
        if (CAMERArecoil != null) CAMERArecoil.enabled = true;

        isReloading = false;
    }

    void UpdateAmmoUI()
    {
        if (AmmoCount != null)
            AmmoCount.text = $"{ammoInMag} / {magazineSize}";
    }
}
