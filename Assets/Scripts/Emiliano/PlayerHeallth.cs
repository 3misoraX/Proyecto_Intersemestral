using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHeallth : MonoBehaviour
{
    //health
    public int hp = 0;
    public int maxHp = 3;
    public float iframes;
    public bool canTakeDamage = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;

    void Start()
    {
        hp = maxHp;
        if (UIManager.Instance != null) UIManager.Instance.UpdateHealth(hp);
    }

    public void LoseHealth(int dmg)
    {
        if (canTakeDamage)
        {
            hp -= dmg;
            if (UIManager.Instance != null) UIManager.Instance.UpdateHealth(hp);

            // Sonido de daño
            SfxPlayer.Play(audioSource, damageSound);

            // Ya que tenías esto listo en PlayerController, aprovechamos a dispararlo
            GetComponent<PlayerController>()?.TriggerDamageAnimation();

            if (hp <= 0) Die();
            StartCoroutine(IFrames());
        }
    }

    public void Heal(int healHp)
    {
        hp += healHp;
        if (hp >= maxHp) hp = maxHp;
        if (UIManager.Instance != null) UIManager.Instance.UpdateHealth(hp);
    }

    public void Die()
    {
        // Sonido de muerte
        SfxPlayer.Play(audioSource, deathSound);

        StartCoroutine(DieAfterSound());
    }

    private IEnumerator DieAfterSound()
    {
        // Espera a que termine el sonido antes de cambiar de escena
        float wait = deathSound != null ? deathSound.length : 0f;
        yield return new WaitForSeconds(wait);
        SceneManager.LoadScene("GameOver");
        gameObject.SetActive(false);
    }

    IEnumerator IFrames()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(iframes);
        canTakeDamage = true;
    }
}