using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskForce : MonoBehaviour  //, Agent
{
    public int TeamId;
    public float[] HpValues
    {
        get => (from warship in m_Warships
                select warship.CurrentHealth).ToArray();
    }

    private Warship[] m_Warships;

    // Start is called before the first frame update
    void Start()
    {
        m_Warships = GetComponentsInChildren<Warship>();

        for (int i = 0; i < m_Warships.Length; i++)
        {
            Debug.Log($"[TaskForce({TeamId})] m_Warships[{i}]: {m_Warships[i]}");
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

    /*///
    /// ML-Agents
    ///
    public override void Initialize()
    {
        //
    }

    public override void OnEpisodeBegin()
    {
        //
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        //
    }

    public override void Heuristic(float[] actionsOut)
    {
        //
    }*/
}
