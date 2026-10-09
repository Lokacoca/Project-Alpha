using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SensitivityUI : MonoBehaviour
{
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityText;

    private void Start()
    {
        // Load saved sensitivity
        float savedSens = PlayerPrefs.GetFloat("mouseSensitivity", 1f);
        sensitivitySlider.value = savedSens;

        UpdateSensitivity(savedSens);

        sensitivitySlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        UpdateSensitivity(value);

        // Save preference
        PlayerPrefs.SetFloat("mouseSensitivity", value);
    }

    private void UpdateSensitivity(float value)
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.sensitivity = value * 50f;
        }

        sensitivityText.text = "Sensitivity: " + value.ToString("0.00");
    }
}
