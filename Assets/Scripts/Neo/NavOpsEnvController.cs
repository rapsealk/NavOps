using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;

public class NavOpsEnvController : MonoBehaviour
{
    public TaskForce TaskForceBlue;
    public TaskForce TaskForceRed;

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
        bool isBlueAllDestroyed = TaskForceBlue.Units.All(unit => unit.IsDestroyed);
        bool isRedAllDestroyed = TaskForceRed.Units.All(unit => unit.IsDestroyed);

        if (isBlueAllDestroyed && isRedAllDestroyed)
        {
            m_AgentGroupBlue.SetGroupReward(0.0f);
            m_AgentGroupRed.SetGroupReward(0.0f);
            m_AgentGroupBlue.EndGroupEpisode();
            m_AgentGroupRed.EndGroupEpisode();
        }
        else if (isBlueAllDestroyed)
        {
            m_AgentGroupBlue.SetGroupReward(-1.0f);
            m_AgentGroupRed.SetGroupReward(1.0f);
            m_AgentGroupBlue.EndGroupEpisode();
            m_AgentGroupRed.EndGroupEpisode();
        }
        else if (isRedAllDestroyed)
        {
            m_AgentGroupBlue.SetGroupReward(1.0f);
            m_AgentGroupRed.SetGroupReward(-1.0f);
            m_AgentGroupBlue.EndGroupEpisode();
            m_AgentGroupRed.EndGroupEpisode();
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
    }
}
