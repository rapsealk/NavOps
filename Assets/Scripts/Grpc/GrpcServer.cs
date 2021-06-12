using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Grpc.Core;

namespace NavOps.Grpc
{
    public class GrpcServer : NavOpsGrpcService.NavOpsGrpcServiceBase
    {
        public GameManager GameManager;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void StartGrpcServer(int grpcPort)
        {
            Debug.Log($"[GrpcServer] GameManager: {GameManager}");
            Debug.Log($"[GrpcServer] GameManager.TaskForceBlue: {GameManager.TaskForceBlue.Units[0]}");
            Debug.Log($"[GrpcServer] GameManager.TaskForceRed: {GameManager.TaskForceRed.Units[0]}");

            try
            {
                Server server = new Server
                {
                    Services = { NavOpsGrpcService.BindService(this/*new GrpcServer()*/) },
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

        public override Task<HeartbeatResponse> RequestHeartbeat(HeartbeatRequest request, ServerCallContext context)
        {
            HeartbeatResponse response = new HeartbeatResponse
            {
                Succeed = (GameManager != null)
            };

            return Task.FromResult(response);
        }

        public override Task<EnvironmentStepResponse> CallEnvironmentStep(EnvironmentStepRequest request, ServerCallContext context)
        {
            // 1. OnActionReceived
            float[][] actions = new float[request.Actions.Count][];
            for (int i = 0; i < request.Actions.Count; i++)
            {
                actions[i] = new float[2];
                actions[i][0] = request.Actions[i].ManeuverActionId;
                actions[i][1] = request.Actions[i].AttackActionId;
            }

            Debug.Log($"[GrpcServer] CallEnvironmentStep #2");
            NavOps.Grpc.Observation observation = new Observation();
            try
            {
                // TODO: Main Thread (Coroutine)
                observation = GameManager.SendActions(actions);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GrpcServer] Exception: {e}");
            }
            Debug.Log($"[GrpcServer] CallEnvironmentStep #3");

            EnvironmentStepResponse response = new EnvironmentStepResponse
            {
                Obs = observation,
                Reward = GameManager.Reward,
                Done = GameManager.EpisodeDone
            };

            return Task.FromResult(response);
        }
    }
}
