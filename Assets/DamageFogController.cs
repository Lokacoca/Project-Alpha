using UnityEngine;
using UnityEngine.UI;

public class DamageFogController : MonoBehaviour
{
    public static DamageFogController Instance;

    public Image fogImage;
    public float fadeDuration = 0.5f;
    public float damageWindow = 2f;

    private float lastHitTime = -999f;
    private int hitCount = 0;
    private float targetAlpha = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Decay hit count after 2 secoonds without damage
        if (Time.time - lastHitTime > damageWindow)
            hitCount = 0;

        // Target intensity from 0 to 1
        targetAlpha = Mathf.Clamp(hitCount * 0.3f, 0f, 1f);

        // Smoothly fade the UI image
        Color c = fogImage.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 5f);
        fogImage.color = c;
    }

    public void RegisterDamage()
    {
        hitCount++;
        lastHitTime = Time.time;
    }
}
