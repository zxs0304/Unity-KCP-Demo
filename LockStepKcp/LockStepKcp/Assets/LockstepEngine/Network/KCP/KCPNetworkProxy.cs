using SGF.Network.KCP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Lockstep.Network
{
    public class KCPNetworkProxy : NetBase
    {
        public static IPEndPoint serverIpPoint = NetworkUtil.ToIPEndPoint("172.16.11.189", 12345);
        public IMessagePacker MessagePacker { get; set; }
        public IKcpMessageDispatcher MessageDispatcher { get; set; }
        public KCPSocket m_Socket;

        public void Awake(NetworkProtocol protocol)
        {
            try
            {
                switch (protocol)
                {
                    case NetworkProtocol.KCP:
                        m_Socket = new KCPSocket(0, 1, AddressFamily.InterNetwork);
                        m_Socket.AddReceiveListener(KCPSocket.IPEP_Any, OnReceiveAny);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        public void Send(IMessage msg)
        {
            m_Socket.SendTo(msg, serverIpPoint);
        }

        private void OnReceiveAny(byte[] buffer, int size, KCPProxy proxy)
        {
            Console.WriteLine($"Receive Any From : {proxy.RemotePoint.Address} {proxy.RemotePoint.Port}");
            Packet packet = new Packet(buffer);
            MessageDispatcher.Dispatch(proxy, packet);
        }

        public void Update()
        {
            m_Socket.Update();
        }


    }
}
