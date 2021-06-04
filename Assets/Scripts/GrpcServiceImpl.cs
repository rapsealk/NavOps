using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class GrpcServiceImpl : GameServerGrpcSdkService.GameServerGrpcSdkServiceBase
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override Task<HealthCheckResponse> OnHealthCheck(HealthCheckRequest request, ServerCallContext context)
    {
        Debug.Log($"OnHealthCheck(request={request}, context={context})");

        return Task.FromResult(new HealthCheckResponse
        {
            HealthStatus = GseManager.HealthStatus
        });
    }

    public override Task<GseResponse> OnStartGameServerSession(StartGameServerSessionRequest request, ServerCallContext context)
    {
        Debug.Log($"OnStartGameServerSession(request={request}, context={context})");

        GseManager.SetGameServerSession(request.GameServerSession);

        var response = GseManager.ActivateGameServerSession(request.GameServerSession.GameServerSessionId, request.GameServerSession.MaxPlayers);

        return Task.FromResult(response);
    }

    public override Task<GseResponse> OnProcessTerminate(ProcessTerminateRequest request, ServerCallContext context)
    {
        Debug.Log($"OnProcessTerminate(request={request}, context={context})");

        GseManager.SetTerminationTime(request.TerminationTime);
        GseManager.TerminateGameServerSession();
        GseManager.ProcessEnding();
        return Task.FromResult(new GseReponse());
    }
}
*/
