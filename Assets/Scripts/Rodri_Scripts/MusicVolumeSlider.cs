using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeSlider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private SliderJuice juice;

    private void Start()
    {
        slider.value = MusicManager.Instance.GetVolume();
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        MusicManager.Instance.SetVolume(value);
        juice?.PunchFill();
    }
}