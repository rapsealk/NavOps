using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NavOps
{
    public class TaskForce : MonoBehaviour
    {
        public int TeamId;
        public NavOps.Grpc.Warship[] Units;

        /*
        public float[] HpValues
        {
            get => (from warship in Units
                    select warship.CurrentHealth).ToArray();
        }
        public TaskForce TargetTaskForce;
        public NavOpsEnvController EnvController;
        */

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < Units.Length; i++)
            {
                Debug.Log($"[TaskForce({TeamId})] Units[{i}]: {Units[i]}");
            }
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void Reset()
        {
            foreach (var unit in Units)
            {
                unit.Reset();
            }
        }
    }
}
