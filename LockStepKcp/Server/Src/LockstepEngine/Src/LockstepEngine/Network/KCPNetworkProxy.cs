﻿using SGF.Network.KCP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Lockstep.FakeServer;
namespace Lockstep.Network
{
    public class KCPNetworkProxy : NetBase
    {
        private readonly Dictionary<long, Session> sessions = new Dictionary<long, Session>();
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
                        m_Socket = new KCPSocket(12345, 1, AddressFamily.InterNetwork);
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

        private void OnReceiveAny(byte[] buffer, int size, KCPProxy proxy)
        {
            Console.WriteLine($"Receive Any From : {(proxy.RemotePoint as IPEndPoint).Address} {(proxy.RemotePoint as IPEndPoint).Port}");
            Packet packet = new Packet(buffer);
            (MessageDispatcher).Dispatch(proxy, packet);
        }
        public void Update()
        {
            m_Socket.Update();
        }


    }
}