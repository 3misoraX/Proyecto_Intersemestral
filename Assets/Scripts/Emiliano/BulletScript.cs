using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public BulletType bulletType;

    void Start()
    {
        Destroy(this.gameObject, bulletType.duration); 
    }
}
