using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiCameraManager : MonoBehaviour
{
    public Camera MainCamera;
    public Camera SubCamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log($"GetKeyDown: KeyCode.Alpha1({KeyCode.Alpha1})");
            SubCamera.enabled = false;
            MainCamera.enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log($"GetKeyDown: KeyCode.Alpha2({KeyCode.Alpha2})");
            MainCamera.enabled = false;
            SubCamera.enabled = true;
        }
    }
}
