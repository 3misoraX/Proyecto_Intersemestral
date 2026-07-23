using UnityEngine;

public class MusicSceneSetter : MonoBehaviour
{
    [SerializeField] private AudioClip sceneMusic;

    private void Start()
    {
        MusicManager.Instance.PlayMusic(sceneMusic);
    }
}