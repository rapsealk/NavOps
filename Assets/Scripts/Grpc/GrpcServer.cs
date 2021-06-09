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

            Observation obs = new Observation();
            obs.Fleets.Add(new FleetObservation
            {
                TeamId = 1,
                Hp = 1.0f,
                Fuel = 1.0f,
                Destroyed = false,
                Detected = false,
                Position = new Position
                {
                    X = 1.0f,
                    Y = 1.0f,
                },
                Rotation = new Rotation
                {
                    Cos = Mathf.Cos(0f * Mathf.Deg2Rad),
                    Sin = Mathf.Sin(0f * Mathf.Deg2Rad)
                }
            });
            obs.Fleets.Add(new FleetObservation
            {
                TeamId = 2,
                Hp = 1.0f,
                Fuel = 1.0f,
                Destroyed = false,
                Detected = false,
                Position = new Position
                {
                    X = -1.0f,
                    Y = -1.0f,
                },
                Rotation = new Rotation
                {
                    Cos = Mathf.Cos(180f * Mathf.Deg2Rad),
                    Sin = Mathf.Sin(180f * Mathf.Deg2Rad)
                }
            });
            obs.TargetIndexOnehot.Add(1.0f);
            /*
            repeated float raycast_hits = 3;
            repeated Battery batteries = 4;
            AimingPoint aiming_point = 5;
            float ammo = 6;
            repeated float speed_level_onehot = 7;
            repeated float steer_level_onehot = 8;
            */

            EnvironmentStepResponse response = new EnvironmentStepResponse
            {
                Obs = obs,
                Reward = 0f,
                Done = false
            };

            return Task.FromResult(response);
        }
    }
}
