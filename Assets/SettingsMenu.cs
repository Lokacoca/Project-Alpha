using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider sensitivitySlider;

    private void Start()
    {
        if (PlayerMovement.Instance != null)
        {
            sensitivitySlider.value = PlayerMovement.Instance.sensitivity;
        }

        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    private void OnSensitivityChanged(float value)
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.sensitivity = value;
        }
    }
}
