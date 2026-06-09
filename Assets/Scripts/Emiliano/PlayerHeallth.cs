using System.Collections;
using UnityEngine;

public class PlayerHeallth : MonoBehaviour
{
    //health
    public int hp = 0;
    public int maxHp = 3;
    [SerializeField] private bool canTakeDamage = true;
    //UI
    //---

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hp = maxHp;
        //actualize the UI
    }

    public void LoseHealth(int dmg)
    {
        if (canTakeDamage)
            {
            hp -= dmg;
            //actualize the UI
            if( hp <= 0)
            {
                Die();
            }
            StartCoroutine(IFrames(1.5f));
        }
    }

    public void Heal(int healHp)
    {
        hp += healHp;
        if (hp >= maxHp)
            hp = maxHp;
    }

    private void Die()
    {
        //game over screen
        this.gameObject.SetActive(false);
    }

    IEnumerator IFrames(float iframes)
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(iframes);
        canTakeDamage = true;
    }
}
