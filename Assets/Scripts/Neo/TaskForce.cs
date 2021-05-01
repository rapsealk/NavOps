using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskForce : MonoBehaviour
{
    public int TeamId;
    public Warship[] Units;// { get => m_Warships; }
    public float[] HpValues
    {
        get => (from warship in Units
                select warship.CurrentHealth).ToArray();
    }
    public TaskForce TargetTaskForce;
    public NavOpsEnvController EnvController;

    // private Warship[] m_Warships;

    // Start is called before the first frame update
    void Start()
    {
        // m_Warships = GetComponentsInChildren<Warship>();

        for (int i = 0; i < Units.Length; i++)
        {
            Debug.Log($"[TaskForce({TeamId})] m_Warships[{i}]: {Units[i]}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reset()
    {
        //
    }
}
