using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating : MonoBehaviour
{
    public float speed = 4f;
    public float offset = 0f;
    public float range = 0.4f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = transform.position;
        newPos.y += range / 2 * Mathf.Sin(speed * Time.time + offset) - range / 2 * Mathf.Sin(speed * Time.time - Time.deltaTime + offset);
        transform.position = newPos;
    }
}
