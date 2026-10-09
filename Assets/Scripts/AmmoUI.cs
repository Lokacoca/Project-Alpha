using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    public TextMeshProUGUI AmmoCount;

    public void UpdateAmmo(int current, int max)
    {
        if (AmmoCount != null)
            AmmoCount.text = $"{current} / {max}";
    }
}
