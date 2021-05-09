using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;

public class RegisterEpisodeSideChannel : MonoBehaviour
{
    EpisodeSideChannel sideChannel;

    public void Awake()
    {
        sideChannel = new EpisodeSideChannel();
        //Application.logMessageReceived += sideChannel.SendDebugStatementToPython;
        SideChannelManager.RegisterSideChannel(sideChannel);
    }

    public void OnDestroy()
    {
        Application.logMessageReceived -= sideChannel.SendDebugStatementToPython;
        if (Academy.IsInitialized)
        {
            SideChannelManager.UnregisterSideChannel(sideChannel);
        }
    }
}
