using UnityEngine;
using NavOps.Grpc;
using Tencentcloud.Gse.Grpcsdk;
using Grpc.Core;

public class GrpcServer : GameServerGrpcSdkService.GameServerGrpcSdkServiceBase
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
    private static Logs logger
    {
        get
        {
            return new Logs();
        }
    }
    */

    public static void StartGrpcServer(int clientPort, int grpcPort, string logPath)
    {
        try
        {
            Server server = new Server
            {
                Services = { GameServerGrpcSdkService.BindService(new GrpcServer()) },
                Ports = { new ServerPort("127.0.0.1", grpcPort, ServerCredentials.Insecure) },
            };
            server.Start();
            // logger.Println("GrpcServer Start On localhost:" + grpcPort);
            Debug.Log($"GrpcServer Start On localhost: {grpcPort}");
            GseManager.ProcessReady(new string[] { logPath }, clientPort, grpcPort);
        }
        catch (System.Exception e)
        {
            // logger.Println("error: " + e.Message);
            Debug.Log($"error: {e.Message}");
        }
    }
}
