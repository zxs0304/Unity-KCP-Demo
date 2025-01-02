
/*
 * 描述：
 * 作者：slicol , zxs
*/
//发送： KCPPlayer.SendMessage  ->  KCPSocket.SendTo  ->  KCPProxy.Dosend  ->  kcp.Send  ->  kcp.flush  ->  KCPProxy.HandleKcpSend  ->  发送UDP包
//接收： UDP包  ->  KCPSocket.DoReceive  ->  KCPProxy.DoReceiveInThread  ->  KCPProxy.HandleRecvQueue  ->  kcp.Input  ->  kcp.Recv  ->  KCPPlayer.OnReceive

//主线程中执行的函数
//KCPSocket.Update()
//KCPProxy.Update()
//KCPProxy.HandleRecvQueue()。
//KCP.Input() 和 KCP.Recv()
//KCP.Update()

//接收线程中执行的函数
//KCPSocket.Thread_Recv()
//循环调用 KCPSocket.DoReceive()：
//m_SystemSocket.ReceiveFrom() 接收 UDP 数据包。
//调用 KCPProxy.DoReceiveInThread()：将数据包推送到SwitchQueue中。

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using Lockstep.FakeServer;
using Lockstep.Logging;
using Lockstep.Network;
using Lockstep.Util;

namespace SGF.Network.KCP
{
    public class KCPSocket
    {
        #region 工具函数
        public static IPEndPoint IPEP_Any = new IPEndPoint(IPAddress.Any, 0);
        public static IPEndPoint IPEP_IPv6Any = new IPEndPoint(IPAddress.IPv6Any, 0);
        public static IPEndPoint GetIPEndPointAny(AddressFamily family, int port)
        {
            if (family == AddressFamily.InterNetwork)
            {
                if (port == 0)
                {
                    return IPEP_Any;
                }

                return new IPEndPoint(IPAddress.Any, port);
            }
            else if (family == AddressFamily.InterNetworkV6)
            {
                if (port == 0)
                {
                    return IPEP_IPv6Any;
                }

                return new IPEndPoint(IPAddress.IPv6Any, port);
            }
            return null;
        }


        private static readonly DateTime UTCTimeBegin = new DateTime(1970, 1, 1);

        public static UInt32 GetClockMS()
        {
            return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(UTCTimeBegin).TotalMilliseconds) & 0xffffffff);
        }

        public delegate void KCPReceiveListener(byte[] buff, int size, KCPProxy remotePoint);

        #endregion


        public string LOG_TAG = "KCPSocket";

        private bool m_IsRunning = false;
        private Socket m_SystemSocket;
        private IPEndPoint m_LocalEndPoint;
        private AddressFamily m_AddrFamily;
        private Thread m_ThreadRecv;
        private byte[] m_RecvBufferTemp = new byte[4096];

        //KCP参数
        private List<KCPProxy> m_ListKcp; 
        private uint m_KcpKey = 0;
        private KCPReceiveListener m_AnyEPListener;

        //=================================================================================
        #region 构造和析构

        public KCPSocket(int bindPort, uint kcpKey, AddressFamily family = AddressFamily.InterNetwork)
        {
            m_AddrFamily = family;
            m_KcpKey = kcpKey;
            m_ListKcp = new List<KCPProxy>();
            m_SystemSocket = new Socket(m_AddrFamily, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = GetIPEndPointAny(m_AddrFamily, bindPort);
            m_SystemSocket.Bind(ipep);

            bindPort = (m_SystemSocket.LocalEndPoint as IPEndPoint).Port;
            LOG_TAG = "KCPSocket[" + bindPort + "-" + kcpKey + "]";

            m_IsRunning = true;
            m_ThreadRecv = new Thread(Thread_Recv) {IsBackground = true};
            m_ThreadRecv.Start();



#if UNITY_EDITOR_WIN
            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            m_SystemSocket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
#endif


#if UNITY_EDITOR
            UnityEditor.EditorApplication.playmodeStateChanged -= OnEditorPlayModeChanged;
            UnityEditor.EditorApplication.playmodeStateChanged += OnEditorPlayModeChanged;
#endif
        }


#if UNITY_EDITOR
        private void OnEditorPlayModeChanged()
        {
            if (Application.isPlaying == false)
            {
                this.Log("OnEditorPlayModeChanged()");
                UnityEditor.EditorApplication.playmodeStateChanged -= OnEditorPlayModeChanged;
                Dispose();
            }
        }
#endif

        public void Dispose()
        {
            m_IsRunning = false;
            m_AnyEPListener = null;

            if (m_ThreadRecv != null)
            {
                m_ThreadRecv.Interrupt();
                m_ThreadRecv = null;
            }

            int cnt = m_ListKcp.Count;
            for (int i = 0; i < cnt; i++)
            {
                m_ListKcp[i].Dispose();
            }
            m_ListKcp.Clear();
            
            if (m_SystemSocket != null)
            {
                try
                {
                    m_SystemSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    Debug.Log("Close() " + e.Message + e.StackTrace);
                }

                m_SystemSocket.Close();
                m_SystemSocket = null;
            }
        }


        public int LocalPort
        {
            get{return (m_SystemSocket.LocalEndPoint as IPEndPoint).Port;}
        }

        public string LocalIP
        {
            //get { return UnityEngine.Network.player.ipAddress; }
            get { return "127.0.0.1"; }
        }

        public IPEndPoint LocalEndPoint
        {
            get
            {
                if (m_LocalEndPoint == null ||
                    //m_LocalEndPoint.Address.ToString() != UnityEngine.Network.player.ipAddress)
                    m_LocalEndPoint.Address.ToString() != "127.0.0.1")
                {
                    IPAddress ip = IPAddress.Parse(LocalIP);
                    m_LocalEndPoint = new IPEndPoint(ip, LocalPort);
                }

                return m_LocalEndPoint;
            }
        }

        public Socket SystemSocket { get { return m_SystemSocket; } }
        
        #endregion

        //=================================================================================

        public bool EnableBroadcast
        {
            get { return m_SystemSocket.EnableBroadcast; }
            set { m_SystemSocket.EnableBroadcast = value; }
        }

        //=================================================================================
        #region 管理KCP

        private KCPProxy GetKcp(IPEndPoint ipep)
        {
            if (ipep == null || ipep.Port == 0 || 
                ipep.Address.Equals(IPAddress.Any) || 
                ipep.Address.Equals(IPAddress.IPv6Any))
            {
                return null;
            }

            KCPProxy proxy;
            int cnt = m_ListKcp.Count;
            for (int i = 0; i < cnt; i++)
            {
                proxy = m_ListKcp[i];
                if (proxy.RemotePoint.Equals(ipep))
                {
                    return proxy;
                }
            }

            proxy = new KCPProxy(m_KcpKey, ipep, m_SystemSocket);
            //保证 不管收到那个ip发来的消息，都会走OnReceiveAny处理
            proxy.AddReceiveListener(OnReceiveAny);
            m_ListKcp.Add(proxy);
            return proxy;
        }

        #endregion

        //=================================================================================
        #region 发送逻辑
        // 根据发送目标的ip,找到对应的kcp代理，把要发的数据传给kcp代理来处理发送
        public bool SendTo(byte[] buffer, int size, IPEndPoint remotePoint)
        {
            if (remotePoint.Address == IPAddress.Broadcast)
            {
                int cnt = m_SystemSocket.SendTo(buffer, size, SocketFlags.None, remotePoint);
                return cnt > 0;
            }
            else
            {
                KCPProxy proxy = GetKcp(remotePoint);
                if (proxy != null)
                {
                    return proxy.DoSend(buffer, size);
                }
            }

            return false;
        }

        public bool SendTo(string message, IPEndPoint remotePoint)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            return SendTo(buffer, buffer.Length, remotePoint);
        }

        #endregion


        //=================================================================================
        #region 主线程驱动

        public void Update()
        {
            if (m_IsRunning)
            {
                //获取时钟
                uint current = GetClockMS();

                int cnt = m_ListKcp.Count;
                for (int i = 0; i < cnt; i++)
                {
                    KCPProxy proxy = m_ListKcp[i];
                    proxy.Update(current);
                }
            }
        }

        #endregion

        //=================================================================================
        #region 接收逻辑
        // 当收到remotePoint发来的消息时，使用listener进行处理
        public void AddReceiveListener(IPEndPoint remotePoint, KCPReceiveListener listener)
        {
            KCPProxy proxy = GetKcp(remotePoint);
            if (proxy != null)
            {
                proxy.AddReceiveListener(listener);
            }
            else
            {
                m_AnyEPListener += listener;
            }
        }

        public void RemoveReceiveListener(IPEndPoint remotePoint, KCPReceiveListener listener)
        {
            KCPProxy proxy = GetKcp(remotePoint);
            if (proxy != null)
            {
                proxy.RemoveReceiveListener(listener);
            }
            else
            {
                m_AnyEPListener -= listener;
            }
        }

        private void OnReceiveAny(byte[] buffer, int size,   KCPProxy remotePoint)
        {
            if (m_AnyEPListener != null)
            {
                m_AnyEPListener(buffer, size, remotePoint);
            }
        }

        #endregion

        //=================================================================================
        #region 接收线程

        private void Thread_Recv()
        {

            Debug.Log("Thread_Recv() Begin ......");
            while (m_IsRunning)
            {
                try
                {
                    DoReceive();
                }
                catch (Exception e)
                {
                    Debug.Log("Thread_Recv() " + e.Message + "\n" + e.StackTrace);
                    Thread.Sleep(10);
                }
            }
            Debug.Log("Thread_Recv() End!");
        }

        // Socket收到UDP包，根据对方的IP找到对应的KCP代理，把udp包交给kcp代理进行接收处理
        private void DoReceive()
        {
            if (m_SystemSocket.Available <= 0)
            {
                return;
            }

            EndPoint remotePoint = new IPEndPoint(IPAddress.Any, 0);
            int cnt = m_SystemSocket.ReceiveFrom(m_RecvBufferTemp, m_RecvBufferTemp.Length,
                SocketFlags.None, ref remotePoint);
            //Console.WriteLine($"收到udp包 大小{cnt}");
            if (cnt > 0)
            {
                KCPProxy proxy = GetKcp((IPEndPoint)remotePoint);
                if (proxy != null)
                {
                    proxy.DoReceiveInThread(m_RecvBufferTemp, cnt);
                }
                
            }
            
        }

        #endregion
    }




    public class KCPProxy
    {
        private KCP m_Kcp;
        private bool m_NeedKcpUpdateFlag = false;
        private uint m_NextKcpUpdateTime = 0;
        private SwitchQueue<byte[]> m_RecvQueue = new SwitchQueue<byte[]>(128);

        private IPEndPoint m_RemotePoint;
        private Socket m_Socket;
        private KCPSocket.KCPReceiveListener m_Listener;

        public IPEndPoint RemotePoint { get { return m_RemotePoint; } }
        public object BindInfo;
        

        public KCPProxy(uint key, IPEndPoint remotePoint, Socket socket)
        {
            m_Socket = socket;
            m_RemotePoint = remotePoint;

            m_Kcp = new KCP(key, HandleKcpSend);
            m_Kcp.NoDelay(1, 1, 2, 1);
            m_Kcp.WndSize(128, 128);

        }

        public T GetBindInfo<T>() where T : class
        {
            return BindInfo as T;
        }

        public void Dispose()
        {
            m_Socket = null;

            if (m_Kcp != null)
            {
                m_Kcp.Dispose();
                m_Kcp = null;
            }

            m_Listener = null;
        }

        //---------------------------------------------
        private void HandleKcpSend(byte[] buff, int size)
        {
            if (m_Socket != null)
            {
                m_Socket.SendTo(buff, 0, size, SocketFlags.None, m_RemotePoint);
            }   
        }

        private void HandleKcpSend_Hook(byte[] buff, int size)
        {
            if (m_Socket != null)
            {
                m_Socket.SendTo(buff, 0, size, SocketFlags.None, m_RemotePoint);
            }    
        }

        public bool DoSend(byte[] buff, int size)
        {
            m_NeedKcpUpdateFlag = true;
            return m_Kcp.Send(buff, size) >= 0;
        }

        // 0号字节为flag（目前flag只有0x00这一种），1~2号字节为操作码，后续为实际的消息
        public bool DoSend(ushort opcode, byte[] buff)
        {
            int size = buff.Length;
            byte[] bytes = new byte[size + 3];
            bytes[0] = 0x00;
            var opcodeBytes = BitConverter.GetBytes(opcode);
            Array.Copy(opcodeBytes, 0, bytes, 1, opcodeBytes.Length);
            Array.Copy(buff, 0, bytes, 3, buff.Length);
            return DoSend(bytes,bytes.Length);
        }

        public bool DoSend(IMessage message)
        {
            byte[] bytes = MessagePacker.Instance.SerializeToByteArray(message);
            return DoSend(message.opcode, bytes);
        }

        //---------------------------------------------

        public void AddReceiveListener(KCPSocket.KCPReceiveListener listener)
        {
            m_Listener += listener;
        }

        public void RemoveReceiveListener(KCPSocket.KCPReceiveListener listener)
        {
            m_Listener -= listener;
        }



        public void DoReceiveInThread(byte[] buffer, int size)
        {
            byte[] dst = new byte[size];
            Buffer.BlockCopy(buffer, 0, dst, 0, size);
            m_RecvQueue.Push(dst);
        }

        private void HandleRecvQueue()
        {
            m_RecvQueue.Switch();
            while (!m_RecvQueue.Empty())
            {
                var recvBufferRaw = m_RecvQueue.Pop();
                // 将socket收到的UDP包进行kcp加工后，存入接收队列
                int ret = m_Kcp.Input(recvBufferRaw);

                //收到的不是一个正确的KCP包
                if (ret < 0)
                {
                    if (m_Listener != null)
                    {
                        m_Listener(recvBufferRaw, recvBufferRaw.Length, this);
                    }
                    return;
                }

                m_NeedKcpUpdateFlag = true;

                // 检查 接收队列 中，如果有一条完整的消息，就接收出来，交给用户层的接收函数处理
                for (int size = m_Kcp.PeekSize(); size > 0; size = m_Kcp.PeekSize())
                {
                    var recvBuffer = new byte[size];
                    if (m_Kcp.Recv(recvBuffer) > 0)
                    {
                        if (m_Listener != null)
                        {
                            m_Listener(recvBuffer, size, this);
                        }
                    }
                }
            }
        }

        //---------------------------------------------
        // 进行真正的接收（HandleRecvQueue）
        // 进行真正的发送（m_Kcp.Update中进行Flush）
        public void Update(uint currentTimeMS)
        {
            HandleRecvQueue();

            if (m_NeedKcpUpdateFlag || currentTimeMS >= m_NextKcpUpdateTime)
            {
                m_Kcp.Update(currentTimeMS);
                m_NextKcpUpdateTime = m_Kcp.Check(currentTimeMS);
                m_NeedKcpUpdateFlag = false;
                //Debug.Log($"m_Socket:{(m_Socket.LocalEndPoint as IPEndPoint).Port}:  {m_Kcp.rx_srtt} ms");
            }
        }

        //---------------------------------------------

    }
}
