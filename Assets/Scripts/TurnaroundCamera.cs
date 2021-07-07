using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnaroundCamera : MonoBehaviour
{
    public Vector3 Center;

    private float angle = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float stepAngle = Time.deltaTime * 20f;
        angle = Mathf.Min(angle + stepAngle, 360f);
        if (angle < 360f)
        {
            Debug.Log($"[Turnaround Camera] angle: {angle}");
            transform.RotateAround(Center, Vector3.up, stepAngle);
        }
    }
}
