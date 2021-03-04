using UnityEngine;

public class Citadel : MonoBehaviour, DamagableObject
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
    
    public void OnDamageTaken()
    {
        Debug.Log($"Citadel({name}).OnDamageTaken()");

        warship.OnDamageTaken();
    }
}
