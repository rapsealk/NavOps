using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NavOps.Entities
{
    public class Warship : MonoBehaviour
    {
        // public Vector3 Velocity;
        // public Vector3 AngularVelocity;
        public float Velocity;
        public int Rudder
        {
            get => _rudder;
            set
            {
                _rudder = (int) Mathf.Max(Mathf.Min(value, 2f), -2f);
            }
        }
        public Rigidbody Rigidbody { get => _rigidbody; }

        private int _rudder;
        private Rigidbody _rigidbody;

        // Start is called before the first frame update
        void Start()
        {
            // Velocity = Vector3.zero;
            // AngularVelocity = Vector3.zero;
            Velocity = 0f;

            _rigidbody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            float rudder = Rudder;
            if (Velocity != 0f)
            {
                Vector3 linearForce = transform.forward * Velocity * Mathf.Cos(rudder * 30f * Mathf.Deg2Rad);
                _rigidbody.AddForce(linearForce);

                // transform.Rotate(transform.up * Rudder * 30f * Mathf.Deg2Rad * Time.deltaTime);
            }
            // _rigidbody.AddForce(transform.forward * Velocity);

            if (Rigidbody.velocity.magnitude != 0f)
            {
                transform.Rotate(transform.up * rudder * 30f * Mathf.Deg2Rad * Time.deltaTime * 4f);
            }
        }
    }
}
