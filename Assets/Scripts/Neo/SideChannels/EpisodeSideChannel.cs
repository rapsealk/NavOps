using System;
using System.Text;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;

public class EpisodeSideChannel : SideChannel
{
    public NavOpsEnvController NavOpsEnvController;

    /**
     * https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Custom-SideChannels.md
     */
    public EpisodeSideChannel()
    {
        ChannelId = new Guid("0a6d3d29-0130-475c-a98c-ae665a752cbc");
    }

    protected override void OnMessageReceived(IncomingMessage msg)
    {
        var receivedString = msg.ReadString();
        Debug.Log($"From Python: {receivedString}");
    }

    public void SendEpisodeDoneToPython(bool blueWins)
    {
        Debug.Log($"[EpisodeSideChannel] SendEpisodeDoneToPython: {blueWins}");
        using (var msgOut = new OutgoingMessage())
        {
            msgOut.WriteBoolean(blueWins);
            QueueMessageToSend(msgOut);
        }
    }
}
