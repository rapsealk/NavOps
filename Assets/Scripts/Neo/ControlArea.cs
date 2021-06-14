using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlArea : MonoBehaviour
{
    public enum DominantForce
    {
        RED     = -1,
        NEUTRAL = 0,
        BLUE    = 1
    }

    public DominantForce InitialDominant = DominantForce.NEUTRAL;
    public int Dominant { get; private set; } = (int) DominantForce.NEUTRAL;
    [HideInInspector]
    public bool Dominated { get => Dominant != (int) DominantForce.NEUTRAL; }
    [HideInInspector]
    public Vector3 Position;

    private SpriteRenderer m_SpriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();

        Position = transform.position;

        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reset()
    {
        Dominant = (int) InitialDominant;
        SetDominant(Dominant);
    }

    public void SetDominant(int dominantId)
    {
        Dominant = dominantId;

        Color spriteColor = Color.gray;
        if (dominantId == (int) DominantForce.RED)
        {
            spriteColor = Color.red;
        }
        else if (dominantId == (int) DominantForce.BLUE)
        {
            spriteColor = Color.green;
        }

        m_SpriteRenderer.color = spriteColor;
    }
}
