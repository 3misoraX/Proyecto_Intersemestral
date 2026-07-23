using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroy : MonoBehaviour
{
    public string[] sceneNames;
    public bool player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(string name in sceneNames)
        {
            if(SceneManager.GetActiveScene().name != name)
            {
                DontDestroyOnLoad(gameObject);
                if (player)
                {
                    gameObject.GetComponent<PlayerHeallth>().hp = gameObject.GetComponent<PlayerHeallth>().maxHp;
                    gameObject.transform.position = new Vector3(0, 1, 0);
                }
            }
        }
    }

}
