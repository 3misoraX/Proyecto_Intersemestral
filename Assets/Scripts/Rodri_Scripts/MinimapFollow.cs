using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform target;
    public float height = 25f;

    void LateUpdate()
    {
        if (!target) return;

        transform.position = new Vector3(
            target.position.x,
            height,
            target.position.z
        );
    }
}
