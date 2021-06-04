using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NavOps.Grpc.Models
{
    public static class PortServer
    {

        /*
        private static Logs logger
        {
            get
            {
                return new Logs();
            }
        }
        */

        public static int GenerateRandomPort(int minPort, int maxPort)
        {
            while (true)
            {
                int count = 0;
                int seed = Convert.ToInt32(Regex.Match(Guid.NewGuid().ToString(), @"\d+").Value);
                System.Random ran = new System.Random(seed);
                int port = ran.Next(minPort, maxPort);
                if (count < 1000 && !IsPortInUsed(port))
                {
                    return port;
                }
                count++;
            }
        }

        private static bool IsPortInUsed(int port)
        {
            try
            {
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] ipsTCP = ipGlobalProperties.GetActiveTcpListeners();
                if (ipsTCP.Any(p => p.Port == port))
                {
                    return true;
                }

                IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();
                if (ipsUDP.Any(p => p.Port == port))
                {
                    return true;
                }

                TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
                if (tcpConnInfoArray.Any(conn => conn.LocalEndPoint.Port == port))
                {
                    return true;
                }
            } catch (NotImplementedException e)
            {
                // logger.Println("error: " + e.Message);
                Debug.Log($"Error: {e.Message}");
            }

            return false;
        }
    }
}
