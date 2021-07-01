using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position = transform.position + transform.forward * 100f * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.position = transform.position - transform.forward * 100f * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(transform.up, -30f * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(transform.up, 30f * Time.deltaTime);
        }
    }
}
