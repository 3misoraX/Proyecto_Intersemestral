using UnityEngine;
using DG.Tweening;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float fadeDuration = 0.4f;

    private float currentVolume = 1f;
    private Tween volumeTween;

    private const string MusicVolumeKey = "MusicVolume";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        musicSource.loop = true;

        currentVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        musicSource.volume = currentVolume;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;

        volumeTween?.Kill();

        musicSource
            .DOFade(0f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                musicSource.clip = clip;
                musicSource.Play();
                musicSource
                    .DOFade(currentVolume, fadeDuration)
                    .SetUpdate(true);
            });
    }

    public void SetVolume(float value)
    {
        currentVolume = value;
        PlayerPrefs.SetFloat(MusicVolumeKey, value);

        volumeTween?.Kill();
        volumeTween = musicSource
            .DOFade(currentVolume, fadeDuration)
            .SetUpdate(true);
    }

    public float GetVolume() => currentVolume;
}