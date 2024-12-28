using Lockstep.Network;
using Lockstep.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lockstep.Logging;
using SGF.Network.KCP;


namespace Lockstep.FakeServer
{
    public class KcpServer : IKcpMessageDispatcher
    {
        //network
        public static IPEndPoint serverIpPoint = NetworkUtil.ToIPEndPoint("127.0.0.1", 12345);
        private KCPNetworkProxy kCPNetworkProxy = new KCPNetworkProxy();

        //update
        private const double UpdateInterval = 0.015; //frame rate = 30
        private DateTime _lastUpdateTimeStamp;
        private DateTime _startUpTimeStamp;
        private double _deltaTime;
        private double _timeSinceStartUp;

        //user mgr 
        private KcpRoom _room;
        private Dictionary<int, PlayerServerInfo> _id2Player = new Dictionary<int, PlayerServerInfo>();
        private Dictionary<int, KCPProxy> _id2KcpProxy = new Dictionary<int, KCPProxy>();
        private Dictionary<string, PlayerServerInfo> _name2Player = new Dictionary<string, PlayerServerInfo>();

        //id
        private static int _idCounter = 0;
        private int _curCount = 0;



        public void Start()
        {
            kCPNetworkProxy.MessageDispatcher = this;
            kCPNetworkProxy.MessagePacker = MessagePacker.Instance;
            kCPNetworkProxy.Awake(NetworkProtocol.KCP);
            _startUpTimeStamp = _lastUpdateTimeStamp = DateTime.Now;
        }

        public void Dispatch(KCPProxy kcpProxy, Packet packet)
        {
            ushort opcode = packet.Opcode();
            var message = MessagePacker.Instance.DeserializeFrom(opcode, packet.Bytes, Packet.Index,
                packet.Length - Packet.Index) as IMessage;
            //var msg = JsonUtil.ToJson(message);
            //Log.sLog("Server " + msg);
            var type = (EMsgType)opcode;
            Console.WriteLine($"收到消息：{type.ToString()}   ,端口：{(kcpProxy.RemotePoint as IPEndPoint).Port}");
            switch (type)
            {
                case EMsgType.JoinRoom:
                    OnPlayerConnect(kcpProxy, message);
                    break;
                case EMsgType.QuitRoom:
                    OnPlayerQuit(kcpProxy, message);
                    break;
                case EMsgType.PlayerInput:
                    OnPlayerInput(kcpProxy, message);
                    break;
                case EMsgType.HashCode:
                    OnPlayerHashCode(kcpProxy, message);
                    break;
            }
        }

        public void Update()
        {
            kCPNetworkProxy.Update();
            var now = DateTime.Now;
            _deltaTime = (now - _lastUpdateTimeStamp).TotalSeconds;
            if (_deltaTime > UpdateInterval)
            {
                _lastUpdateTimeStamp = now;
                _timeSinceStartUp = (now - _startUpTimeStamp).TotalSeconds;
                DoUpdate();
            }
        }

        public void DoUpdate()
        {
            //check frame inputs
            var fDeltaTime = (float)_deltaTime;
            var fTimeSinceStartUp = (float)_timeSinceStartUp;
            _room?.DoUpdate(fTimeSinceStartUp, fDeltaTime);
        }


        void OnPlayerConnect(KCPProxy kcpProxy, IMessage message)
        {
            //TODO load from db

            var msg = message as Msg_JoinRoom;
            msg.name = msg.name + _idCounter;
            var name = msg.name;
            if (_name2Player.TryGetValue(name, out var val))
            {
                return;
            }

            var info = new PlayerServerInfo();
            info.Id = _idCounter++;
            info.name = name;
            _name2Player[name] = info;
            _id2Player[info.Id] = info;
            _id2KcpProxy[info.Id] = kcpProxy;
            kcpProxy.BindInfo = info;
            _curCount++;
            if (_curCount >= KcpRoom.MaxPlayerCount)
            {
                _room = new KcpRoom();
                _room.Init(0);
                foreach (var player in _id2Player.Values)
                {
                    _room.OnPlayerJoin(_id2KcpProxy[player.Id], player);
                }

                OnGameStart(_room);
            }
            Debug.Log("OnPlayerConnect count:" + _curCount + " " + JsonUtil.ToJson(msg));
        }

        void OnPlayerQuit(KCPProxy kcpProxy, IMessage message)
        {
            Debug.Log("OnPlayerQuit count:" + _curCount);
            var player = kcpProxy.GetBindInfo<PlayerServerInfo>();
            if (player == null)
                return;
            _id2Player.Remove(player.Id);
            _name2Player.Remove(player.name);
            _id2KcpProxy.Remove(player.Id);
            _curCount--;
            if (_curCount == 0)
            {
                _room = null;
            }
        }

        void OnPlayerInput(KCPProxy kcpProxy, IMessage message)
        {
            var msg = message as Msg_PlayerInput;
            var player = kcpProxy.GetBindInfo<PlayerServerInfo>();
            _room?.OnPlayerInput(player.Id, msg);
        }
        void OnPlayerHashCode(KCPProxy kcpProxy, IMessage message)
        {
            var msg = message as Msg_HashCode;
            var player = kcpProxy.GetBindInfo<PlayerServerInfo>();
            _room?.OnPlayerHashCode(player.Id, msg);
        }

        void OnGameStart(KcpRoom room)
        {
            if (room.IsRunning)
            {
                return;
            }

            room.OnGameStart();
        }
    }
}