using UnityEngine;
using System;
using NavOps.Grpc.Models;

public class StartServer : MonoBehaviour
{
    private int grpcPort = PortServer.GenerateRandomPort(50000, 60000);
    private int chatPort = PortServer.GenerateRandomPort(6001, 10000);
    private const string logPath = "./log/log.txt";

    // Start is called before the first frame update
    //[Obsolete]
    void Start()
    {
        // Start GrpcServer By Grpc, Listen on TCP protocol
        GrpcServer.StartGrpcServer(chatPort, grpcPort, logPath);
    }

    //[Obsolete]
    void OnGUI()
    {
    }
}
