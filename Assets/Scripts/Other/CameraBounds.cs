using System.Collections;
using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    BoxCollider bc;
    public GameObject cameraObj;
    public bool inRoom = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bc = GetComponent<BoxCollider>();
        cameraObj = GameObject.Find("CinemachineCamera");
    }

    void Update()
    {
        if (!inRoom)
        {
            transform.position = Vector3.MoveTowards(transform.position, cameraObj.transform.position, 20f * Time.deltaTime);
        }
    }

    public IEnumerator MoveBoundaries(Transform boundPos)
    {
        while(transform.position != boundPos.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, boundPos.position, 20f * Time.deltaTime);
            yield return null;
        }
        yield return null;
    }

    public void ResizeBoundaries(float x = 10f, float z = 10f)
    {
        bc.size = new Vector3(x, bc.size.y, z);
    }
}
