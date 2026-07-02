using JetBrains.Annotations;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    BoxCollider bc;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bc = GetComponent<BoxCollider>();
    }

    public void MoveBoundaries(Transform boundPos)
    {
        transform.position = boundPos.position;
    }

    public void ResizeBoundaries(float x = 10f, float z = 10f)
    {
        bc.size = new Vector3(x, bc.size.y, z);
    }
}
