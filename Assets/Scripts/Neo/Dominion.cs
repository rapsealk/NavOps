using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dominion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log($"OnTriggerEnter(collider: {collider.tag} / {collider.name})");

        if (collider.tag.Equals("Player"))
        {
            Debug.Log($"OnTriggerEnter: {collider.GetComponent<Warship>().playerId}");
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        Debug.Log($"OnTriggerExit(collider: {collider.tag})");

        if (collider.tag.Equals("Player"))
        {
            Debug.Log($"OnTriggerEnter: {collider.GetComponent<Warship>().playerId}");
        }
    }
}
