using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavOpsEnvController : MonoBehaviour
{
    // private NavOps.Grpc.GrpcServer GrpcServer;

    // Start is called before the first frame update
    void Start()
    {
        NavOps.Grpc.GrpcServer.StartGrpcServer(grpcPort: 9090);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
