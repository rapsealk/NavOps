using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NavOps.Systems
{
    public class UserInterfaceSystem : MonoBehaviour
    {
        public Text EngineLevelText;
        public Text CurrentVelocityText;
        public Text[] RudderTexts;

        public NavOps.Entities.Warship Warship;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            EngineLevelText.text = $"Engine Level: {Warship.Velocity}";
            // CurrentVelocityText.text = $"Velocity: {Warship.Rigidbody.velocity.magnitude}";
            CurrentVelocityText.text = $"Velocity: {Vector3.Dot(Warship.Rigidbody.velocity, Warship.transform.forward)}";

            for (int i = 0; i < RudderTexts.Length; i++)
            {
                RudderTexts[i].color = Color.black;

                if (Warship.Rudder == (i - 2))
                {
                    RudderTexts[i].color = Color.green;
                }
            }
        }
    }
}
