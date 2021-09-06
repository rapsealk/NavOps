using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavOps.Entities;

namespace NavOps.Systems
{
    public class WarshipControlSystem : MonoBehaviour
    {
        private Warship _warship;
        // Start is called before the first frame update
        void Start()
        {
            _warship = GetComponent<Warship>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                // _warship.Velocity += _warship.transform.forward;
                _warship.Velocity += 1f;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                // _warship.Velocity -= _warship.transform.forward;
                _warship.Velocity -= 1f;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                // _warship.AngularVelocity -= _warship.transform.right;
                _warship.Rudder -= 1;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                // _warship.AngularVelocity += _warship.transform.right;
                _warship.Rudder += 1;
            }
        }
    }
}
