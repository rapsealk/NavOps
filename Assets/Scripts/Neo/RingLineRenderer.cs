using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RingLineRenderer : MonoBehaviour
{
    public int TeamId;
    public float ThetaScale = 0.01f;
    public float Radius = 3f;
    public int Size;

    private LineRenderer m_LineRenderer;
    private float m_Theta = 0f;

    // Start is called before the first frame update
    void Start()
    {
        m_LineRenderer = GetComponent<LineRenderer>();
        
        m_LineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        Color teamColor = (TeamId == 1) ? Color.green : Color.red;
        m_LineRenderer.sharedMaterial.SetColor("_Color", teamColor);

        // m_LineRenderer.useWorldSpace = false;
    }

    // Update is called once per frame
    void Update()
    {
        m_Theta = 0f;
        Size = (int) ((1f / ThetaScale) + 1f);
        // m_LineRenderer.SetVertexCount(Size);
        m_LineRenderer.positionCount = Size;

        Vector3 currentPosition = transform.position;

        for (int i = 0; i < Size; i++)
        {
            m_Theta += (2.0f * Mathf.PI * ThetaScale);
            float x = Radius * Mathf.Cos(m_Theta);
            float y = Radius * Mathf.Sin(m_Theta);

            // m_LineRenderer.SetPosition(i, new Vector3(y, 0f, x));
            m_LineRenderer.SetPosition(i, new Vector3(y, 0f, x) + currentPosition);
        }
    }
}
