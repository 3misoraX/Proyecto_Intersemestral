using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHeallth : MonoBehaviour
{
    //health
    public int hp = 0;
    public int maxHp = 3;
    public float iframes;
    [SerializeField] private bool canTakeDamage = true;
    //UI
    //---

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
            if (UIManager.Instance != null) UIManager.Instance.UpdateHealth(hp); // ACTUALIZA UI
            if( hp <= 0) Die();
            StartCoroutine(IFrames());
        }
    }

    public void Heal(int healHp)
    {
        hp += healHp;
        if (hp >= maxHp) hp = maxHp;
        if (UIManager.Instance != null) UIManager.Instance.UpdateHealth(hp); // ACTUALIZA UI
    }

    public void Die()
    {
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
