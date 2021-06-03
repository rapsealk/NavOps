using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dominion : MonoBehaviour
{
    // [Range(-1, 1)]
    public int Dominant { get; private set; } = 0;

    private SpriteRenderer m_SpriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDominant(int dominantId)
    {
        Dominant = dominantId;

        Color spriteColor = Color.gray;
        if (dominantId == -1)
        {
            spriteColor = Color.red;
        }
        else if (dominantId == 1)
        {
            spriteColor = Color.green;
        }

        m_SpriteRenderer.color = spriteColor;
    }
}
