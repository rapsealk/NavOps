using UnityEngine;

public class Citadel : MonoBehaviour
{
    private Warship warship;

    // Start is called before the first frame update
    void Start()
    {
        warship = GetComponentInParent<Warship>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void TakeDamage()
    {
        Debug.Log($"Citadel({name}).TakeDamage()");

        warship.TakeDamage();
    }
}
