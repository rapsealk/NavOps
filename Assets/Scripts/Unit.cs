using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public uint TeamId = 1;
    public bool IsSelected = false;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material.color = (TeamId == 1) ? Color.blue : Color.red;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsSelected)
        {
            HandleKeyboardInputs();
        }

        Debug.Log($"Unit({name}).velocity: {rb.velocity}");
    }

    private void HandleKeyboardInputs()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rb.AddTorque(-transform.up * 1f, ForceMode.Acceleration);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            rb.AddTorque(transform.up * 1f, ForceMode.Acceleration);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            rb.AddForce(transform.forward * 10f, ForceMode.Acceleration);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            rb.AddForce(-transform.forward * 10f, ForceMode.Acceleration);
        }
    }
}
