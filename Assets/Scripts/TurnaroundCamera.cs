using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnaroundCamera : MonoBehaviour
{
    public Vector3 Center;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(Center, Vector3.up, Time.deltaTime * 20f);
    }
}
