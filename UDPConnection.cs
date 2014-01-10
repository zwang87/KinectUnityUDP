using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace KinectForUnityTest
{
    public class UDPConnection
    {
        private const String ip = "127.0.0.1";
        private const int port = 9090;
        UdpClient client;

        public UDPConnection()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            client = new UdpClient();
        }

        public void SendData(Byte[] data)
        {
            client.Send(data, data.Length, ip, port);
        }

        public void SendData(string data)
        {
            SendData(System.Text.Encoding.Default.GetBytes(data));
        }

        public void CloseConnection()
        {
            client.Close();
        }

    }

}
