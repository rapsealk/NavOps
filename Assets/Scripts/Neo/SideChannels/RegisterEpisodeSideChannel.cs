using UnityEngine;
/*
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;

public class RegisterEpisodeSideChannel : MonoBehaviour
{
    EpisodeSideChannel sideChannel;

    public void Awake()
    {
        sideChannel = new EpisodeSideChannel();
        sideChannel.NavOpsEnvController = GetComponent<NavOpsEnvController>();
        sideChannel.NavOpsEnvController.OnEpisodeStatusChanged += sideChannel.SendEpisodeDoneToPython;
        SideChannelManager.RegisterSideChannel(sideChannel);
    }

    public void OnDestroy()
    {
        sideChannel.NavOpsEnvController.OnEpisodeStatusChanged -= sideChannel.SendEpisodeDoneToPython;
        if (Academy.IsInitialized)
        {
            SideChannelManager.UnregisterSideChannel(sideChannel);
        }
    }
}
*/
