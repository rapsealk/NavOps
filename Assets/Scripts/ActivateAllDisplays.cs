using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAllDisplays : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Displays Connected: {Display.displays.Length}");

        for (int i = 0; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
