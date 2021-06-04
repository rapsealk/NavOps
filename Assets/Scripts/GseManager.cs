using UnityEngine;
using Tencentcloud.Gse.Grpcsdk;
using Grpc.Core;
//using System.Diagnostics;
using System;

namespace NavOps.Grpc
{
    public static class GseManager
    {

        private static string agentAdress = "127.0.0.1:5758";

        public static GameServerGrpcSdkService.GameServerGrpcSdkServiceClient GameServerClient
        {
            get
            {
                Channel channel = new Channel(agentAdress, ChannelCredentials.Insecure);
                return new GameServerGrpcSdkService.GameServerGrpcSdkServiceClient(channel);
            }
        }

        public static GseGrpcSdkService.GseGrpcSdkServiceClient GseClient
        {
            get
            {
                Channel channel = new Channel(agentAdress, ChannelCredentials.Insecure);
                return new GseGrpcSdkService.GseGrpcSdkServiceClient(channel);
            }
        }

        private static long terminationTime;
        private static GameServerSession gameServerSession;

        /*
        private static Logs logger
        {
            get
            {
                return new Logs();
            }
        }
        */

        private static string pid
        {
            get
            {
                return System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
            }
        }

        private static Metadata meta
        {
            get
            {
                return new Metadata{
                    {"pid", pid},
                    {"requestId", Guid.NewGuid().ToString("n")},
                };
            }
        }

        private static bool healthStatus = true;
        public static bool HealthStatus
        {
            get
            {
                return healthStatus;
            }
            set
            {
                healthStatus = value;
            }
        }

        public static void SetTerminationTime(long stime)
        {
            terminationTime = stime;
        }

        public static GameServerSession GetGameServerSession()
        {
            return gameServerSession;
        }

        public static void SetGameServerSession(GameServerSession gsession)
        {
            if (gameServerSession == null)
            {
                gameServerSession = new GameServerSession(gsession);
            }
            // logger.Println($"SetGameServerSession new: {gameServerSession}");
            Debug.Log($"SetGameServerSession new: {gameServerSession}");
        }

        // 上报游戏进程已经就绪
        public static GseResponse ProcessReady(string[] logPath, int clientPort, int grpcPort)
        {
            // logger.Println($"Getting process ready, LogPath: {logPath}, ClientPort: {clientPort}, GrpcPort: {grpcPort}");
            Debug.Log($"Getting process ready, LogPath: {logPath}, ClientPort: {clientPort}, GrpcPort: {grpcPort}");
            var req = new ProcessReadyRequest
            {
                ClientPort = clientPort,
                GrpcPort = grpcPort,
            };
            req.LogPathsToUpload.Add(logPath);         //repeated类型解析pb后，是只读类型，需要Add加入           
            return GseClient.ProcessReady(req, meta);
        }

        // 激活游戏会话
        public static GseResponse ActivateGameServerSession(string gameServerSessionId, int maxPlayers)
        {
            // logger.Println($"Activating game server session, GameServerSessionId: {gameServerSessionId}, MaxPlayers: {maxPlayers}");
            Debug.Log($"Activating game server session, GameServerSessionId: {gameServerSessionId}, MaxPlayers: {maxPlayers}");
            var req = new ActivateGameServerSessionRequest
            {
                GameServerSessionId = gameServerSessionId,
                MaxPlayers = maxPlayers,
            };
            return GseClient.ActivateGameServerSession(req, meta);
        }

        // 玩家加入
        public static GseResponse AcceptPlayerSession(string playerSessionId)
        {
            // logger.Println($"Accepting player session, PlayerSessionId: {playerSessionId}");
            Debug.Log($"Accepting player session, PlayerSessionId: {playerSessionId}");
            var req = new AcceptPlayerSessionRequest
            {
                GameServerSessionId = gameServerSession.GameServerSessionId,
                PlayerSessionId = playerSessionId,
            };
            return GseClient.AcceptPlayerSession(req, meta);
        }

        // 玩家移除
        public static GseResponse RemovePlayerSession(string playerSessionId)
        {
            // logger.Println($"Removing player session, PlayerSessionId: {playerSessionId}");
            Debug.Log($"Removing player session, PlayerSessionId: {playerSessionId}");
            var req = new RemovePlayerSessionRequest
            {
                GameServerSessionId = gameServerSession.GameServerSessionId,
                PlayerSessionId = playerSessionId,
            };
            return GseClient.RemovePlayerSession(req, meta);
        }

        // 结束游戏会话
        public static GseResponse TerminateGameServerSession()
        {
            // logger.Println($"Terminating game server session, GameServerSessionId: {gameServerSession.GameServerSessionId}");
            Debug.Log($"Terminating game server session, GameServerSessionId: {gameServerSession.GameServerSessionId}");
            var req = new TerminateGameServerSessionRequest
            {
                GameServerSessionId = gameServerSession.GameServerSessionId
            };
            return GseClient.TerminateGameServerSession(req, meta);
        }

        // 结束游戏进程
        public static GseResponse ProcessEnding()
        {
            // logger.Println($"Process ending, pid: {pid}");
            Debug.Log($"Process ending, pid: {pid}");
            var req = new ProcessEndingRequest();
            return GseClient.ProcessEnding(req, meta);
        }

        // 获取玩家信息
        public static DescribePlayerSessionsResponse DescribePlayerSessions(string gameServerSessionId, string playerId, string playerSessionId, string playerSessionStatusFilter, string nextToken, int limit)
        {
            // logger.Println($"Describing player session, GameServerSessionId: {gameServerSessionId}, PlayerId: {playerId}, PlayerSessionId: {playerSessionId}, PlayerSessionStatusFilter: {playerSessionStatusFilter}, NextToken: {nextToken}, Limit: {limit}");
            Debug.Log($"Describing player session, GameServerSessionId: {gameServerSessionId}, PlayerId: {playerId}, PlayerSessionId: {playerSessionId}, PlayerSessionStatusFilter: {playerSessionStatusFilter}, NextToken: {nextToken}, Limit: {limit}");
            var req = new DescribePlayerSessionsRequest
            {
                GameServerSessionId = gameServerSessionId,
                PlayerId = playerId,
                PlayerSessionId = playerSessionId,
                PlayerSessionStatusFilter = playerSessionStatusFilter,
                NextToken = nextToken,
                Limit = limit,
            };
            return GseClient.DescribePlayerSessions(req, meta);
        }

        // 更新玩家加入策略
        public static GseResponse UpdatePlayerSessionCreationPolicy(string newPolicy)
        {
            // logger.Println($"Updating player session creation policy, newPolicy: {newPolicy}");
            Debug.Log($"Updating player session creation policy, newPolicy: {newPolicy}");
            var req = new UpdatePlayerSessionCreationPolicyRequest
            {
                GameServerSessionId = gameServerSession.GameServerSessionId,
                NewPlayerSessionCreationPolicy = newPolicy,
            };
            return GseClient.UpdatePlayerSessionCreationPolicy(req, meta);
        }

        // 上报自定义数据
        public static GseResponse ReportCustomData(int currentCustomCount, int maxCustomCount)
        {
            // logger.Println($"Reporting custom data, CurrentCustomCount: {currentCustomCount}, MaxCustomCount: {maxCustomCount}");
            Debug.Log($"Reporting custom data, CurrentCustomCount: {currentCustomCount}, MaxCustomCount: {maxCustomCount}");
            var req = new ReportCustomDataRequest
            {
                CurrentCustomCount = currentCustomCount,
                MaxCustomCount = maxCustomCount,
            };
            return GseClient.ReportCustomData(req, meta);
        }
    }
}
