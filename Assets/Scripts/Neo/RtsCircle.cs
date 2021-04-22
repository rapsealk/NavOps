using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RtsCircle : MonoBehaviour
{
    [Range(0, 50)]
    public int Segments = 50;

    //[Range(0, 300)]
    //public float xRadius = 300f;

    //[Range(0, 300)]
    //public float yRadius = 300f;

    LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        // lineRenderer.SetColors(Color.green, Color.green);
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        // lineRenderer.SetVertexCount(Segments+1);
        lineRenderer.positionCount = Segments + 1;
        lineRenderer.useWorldSpace = false;

        RenderRtsCircleLine();
    }

    void RenderRtsCircleLine()
    {
        float x, y;
        float angle = 0f;

        for (int i = 0; i < Segments+1; i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * Turret.AttackRange;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * Turret.AttackRange;

            lineRenderer.SetPosition(i, new Vector3(y, 0f, x));

            angle += (360f / Segments);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
