using System;
using System.Text;
using UnityEngine;
/*
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;

public class AimSideChannel : SideChannel
{
    public AimSideChannel()
    {
        ChannelId = new Guid("0a6d3d29-0130-475c-a98c-ae665a752cbc");
    }

    protected override void OnMessageReceived(IncomingMessage msg)
    {
        var receivedString = msg.ReadString();
        Debug.Log($"From Python: {receivedString}");
    }

    public void SendDebugStatementToPython(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error)
        {
            var stringToSend = type.ToString() + ": " + logString + "\n" + stackTrace;
            using (var msgOut = new OutgoingMessage())
            {
                msgOut.WriteString(stringToSend);
                QueueMessageToSend(msgOut);
            }
        }
    }
}
*/
