using UnityEngine;

public static class SfxPlayer
{
    // Para objetos que YA tienen su propio AudioSource y viven un rato (Player, enemigos, puertas)
    public static void Play(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null) return;
        source.PlayOneShot(clip, SfxSettings.GetVolume());
    }

    // Para objetos que se destruyen justo al reproducir el sonido (pickups, cofres, objetos rompibles)
    public static void PlayAtPoint(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, SfxSettings.GetVolume());
    }
}