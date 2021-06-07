using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Grpc.Core;

namespace NavOps.Grpc
{
    public class GrpcServer : NavOpsGrpcService.NavOpsGrpcServiceBase
    {
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public static void StartGrpcServer(int grpcPort)
        {
            try
            {
                Server server = new Server
                {
                    Services = { NavOpsGrpcService.BindService(new GrpcServer()) },
                    Ports = { new ServerPort("127.0.0.1", grpcPort, ServerCredentials.Insecure) }
                };
                server.Start();
                Debug.Log($"GrpcServer is running: {grpcPort}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GrpcServer]: {e.Message}");
            }
        }

        public override Task<EnvironmentStepResponse> CallEnvironmentStep(EnvironmentStepRequest request, ServerCallContext context)
        {
            Debug.Log($"[GrpcServer] CallEnvironmentStep(request={request}, context={context})");

            return Task.FromResult(new EnvironmentStepResponse());
        }
    }
}
