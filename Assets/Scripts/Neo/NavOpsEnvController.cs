using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;

public class NavOpsEnvController : MonoBehaviour
{
    public TaskForce TaskForceBlue;
    public TaskForce TaskForceRed;
    public BooleanCallback OnEpisodeStatusChanged = EmptyCallback;
    public Dominion[] Dominations;

    ///
    /// https://github.com/Unity-Technologies/ml-agents/blob/18d51352a716f7c48f27f3341b6a2ce2a7ca0e34/com.unity.ml-agents/Runtime/SimpleMultiAgentGroup.cs
    ///
    private SimpleMultiAgentGroup m_AgentGroupBlue;
    private SimpleMultiAgentGroup m_AgentGroupRed;

    // Start is called before the first frame update
    void Start()
    {
        m_AgentGroupBlue = new SimpleMultiAgentGroup();
        m_AgentGroupRed = new SimpleMultiAgentGroup();

        foreach (var unit in TaskForceBlue.Units)
        {
            m_AgentGroupBlue.RegisterAgent(unit);
        }

        foreach (var unit in TaskForceRed.Units)
        {
            m_AgentGroupRed.RegisterAgent(unit);
        }

        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < Dominations.Length; i++)
        {
            float r = Dominations[i].transform.localScale.x * 0.75f;

            Vector3 center = Dominations[i].transform.position;

            int blueArrived = 0;
            int redArrived = 0;

            for (int j = 0; j < TaskForceBlue.Units.Length; j++)
            {
                Vector3 pos = TaskForceBlue.Units[j].transform.position;
                bool detected = (Mathf.Pow(pos.x - center.x, 2f) + Mathf.Pow(pos.z - center.z, 2f)) <= Mathf.Pow(r, 2f);
                if (detected)
                {
                    blueArrived = 1;
                    break;
                }
            }

            for (int j = 0; j < TaskForceRed.Units.Length; j++)
            {
                Vector3 pos = TaskForceRed.Units[j].transform.position;
                bool detected = (Mathf.Pow(pos.x - center.x, 2f) + Mathf.Pow(pos.z - center.z, 2f)) <= Mathf.Pow(r, 2f);
                if (detected)
                {
                    redArrived = 1;
                    break;
                }
            }

            if (blueArrived == 1 || redArrived == 1)
            {
                int dominantId = blueArrived - redArrived;
                Dominations[i].SetDominant(dominantId);
            }
        }

        // Detection
        RaycastHit hit;
        for (int i = 0; i < TaskForceBlue.Units.Length; i++)
        {
            Vector3 position = TaskForceBlue.Units[i].transform.position;
            for (int j = 0; j < TaskForceRed.Units.Length; j++)
            {
                bool raycastDetected = false;
                string targetName = "";

                Vector3 targetPosition = TaskForceRed.Units[j].transform.position;
                Vector3 direction = targetPosition - position;
                if (Physics.Raycast(position, direction, out hit, maxDistance: 200f))
                {
                    if (hit.collider.tag != "Player")
                    {
                        continue;
                    }

                    raycastDetected = true;
                    targetName = hit.collider.name;
                }

                Debug.Log($"TaskForceBlue[{i}] -> TaskForceRed[{j}]: {raycastDetected} ({targetName})");
            }
        }

        for (int i = 0; i < TaskForceRed.Units.Length; i++)
        {
            Vector3 position = TaskForceRed.Units[i].transform.position;
            for (int j = 0; j < TaskForceBlue.Units.Length; j++)
            {
                bool raycastDetected = false;
                string targetName = "";

                Vector3 targetPosition = TaskForceBlue.Units[j].transform.position;
                Vector3 direction = targetPosition - position;
                if (Physics.Raycast(position, direction, out hit, maxDistance: 200f))
                {
                    if (hit.collider.tag != "Player")
                    {
                        continue;
                    }

                    raycastDetected = true;
                    targetName = hit.collider.name;
                }

                Debug.Log($"TaskForceRed[{i}] -> TaskForceBlue[{j}]: {raycastDetected} ({targetName})");
            }
        }

        /*
        if (false)
        {
            m_AgentGroupBlue.GroupEpisodeInterrupted();
            m_AgentGroupRed.GroupEpisodeInterrupted();
            Reset();
        }
        */

        /*
        if (false)
        {
            m_AgentGroupBlue.AddGroupReward(1.0f);
            m_AgentGroupRed.AddGroupReward(-1.0f);
            // m_AgentGroupBlue.SetGroupReward(1.0f);
            // m_AgentGroupRed.SetGroupReward(-1.0f);

            m_AgentGroupBlue.EndGroupEpisode();
            m_AgentGroupRed.EndGroupEpisode();
        }
        */
    }

    public void Reset()
    {
        foreach (var unit in TaskForceBlue.Units)
        {
            unit.Reset();
        }

        foreach (var unit in TaskForceRed.Units)
        {
            unit.Reset();
        }

        for (int i = 0; i < Dominations.Length; i++)
        {
            Dominations[i].GetComponent<SpriteRenderer>().color = Color.gray;
        }
    }

    public void NotifyAgentDestroyed()
    {
        bool isBlueAllDestroyed = TaskForceBlue.Units.All(unit => unit.IsDestroyed);
        bool isRedAllDestroyed = TaskForceRed.Units.All(unit => unit.IsDestroyed);

        if (isBlueAllDestroyed && isRedAllDestroyed)
        {
            m_AgentGroupBlue.SetGroupReward(0.0f);
            m_AgentGroupRed.SetGroupReward(0.0f);
        }
        else if (isBlueAllDestroyed)
        {
            m_AgentGroupBlue.SetGroupReward(-1.0f);
            m_AgentGroupRed.SetGroupReward(1.0f);
        }
        else if (isRedAllDestroyed)
        {
            m_AgentGroupBlue.SetGroupReward(1.0f);
            m_AgentGroupRed.SetGroupReward(-1.0f);
        }

        if (isBlueAllDestroyed || isRedAllDestroyed)
        {
            Debug.Log($"NavOpsEnvController(Blue={isBlueAllDestroyed}, Red={isRedAllDestroyed})");
            m_AgentGroupBlue.EndGroupEpisode();
            m_AgentGroupRed.EndGroupEpisode();

            OnEpisodeStatusChanged(isRedAllDestroyed);
        }
    }

    public void CheckAllDominantsOccupied()
    {
        // TODO:
    }

    public delegate void BooleanCallback(bool booleanValue);

    private static void EmptyCallback(bool booleanValue)
    {
        Debug.Log($"[NavOpsEnvController] EmptyCallback: {booleanValue}");
    }
}
