using System;
using System.Collections.Generic;
using System.Net;
using Lockstep.Logging;
using Lockstep.Network;
using Lockstep.Util;
using SGF.Network.KCP;

namespace Lockstep.FakeServer{
    public class Server : IMessageDispatcher {
        //network
        public static IPEndPoint serverIpPoint = NetworkUtil.ToIPEndPoint("127.0.0.1", 10083);
        private NetOuterProxy _netProxy = new NetOuterProxy();

        //update
        //这个时间与服务器收发消息无关，收发消息是一直在进行的，这个间隔表示服务器下发一次逻辑帧的间隔
        //0.015表示0.015秒钟进行一次checkInput()，检查是否要发送, 具体取决于网速 和 玩家的输入频率
        //(实际上这个框架在接收到消息的时候也会进行checkInput，因此这个间隔没什么用，只要当服务器收到全部玩家的输入时，就会立即下发逻辑帧)
        private const double UpdateInterval = 0.015; //frame rate = 66
        private DateTime _lastUpdateTimeStamp;
        private DateTime _startUpTimeStamp;
        private double _deltaTime;
        private double _timeSinceStartUp;

        //user mgr 
        private Room _room;
        private Dictionary<int, PlayerServerInfo> _id2Player = new Dictionary<int, PlayerServerInfo>();
        private Dictionary<int, Session> _id2Session = new Dictionary<int, Session>();
        private Dictionary<string, PlayerServerInfo> _name2Player = new Dictionary<string, PlayerServerInfo>();

        //id
        private static int _idCounter = 0;
        private int _curCount = 0;
        
        

        public void Start(){
            _netProxy.MessageDispatcher = this;
            _netProxy.MessagePacker = MessagePacker.Instance;
            _netProxy.Awake(NetworkProtocol.TCP, serverIpPoint);
            _startUpTimeStamp = _lastUpdateTimeStamp = DateTime.Now;
        }

        public void Dispatch(Session session, Packet packet){
            ushort opcode = packet.Opcode();
            var message = session.Network.MessagePacker.DeserializeFrom(opcode, packet.Bytes, Packet.Index,
                packet.Length - Packet.Index) as IMessage;
            //var msg = JsonUtil.ToJson(message);
            //Log.sLog("Server " + msg);
            var type = (EMsgType) opcode;
            switch (type) {
                case EMsgType.JoinRoom:
                    OnPlayerConnect(session, message);
                    break;
                case EMsgType.QuitRoom:
                    OnPlayerQuit(session, message);
                    break;
                case EMsgType.PlayerInput:
                    OnPlayerInput(session, message);
                    break;
                case EMsgType.HashCode:
                    OnPlayerHashCode(session, message);
                    break;
            }
        }

        public void Update(){
            var now = DateTime.Now;
            _deltaTime = (now - _lastUpdateTimeStamp).TotalSeconds;
            if (_deltaTime > UpdateInterval) {
                _lastUpdateTimeStamp = now;
                _timeSinceStartUp = (now - _startUpTimeStamp).TotalSeconds;
                DoUpdate();
            }
        }

        public void DoUpdate(){
            //check frame inputs
            var fDeltaTime = (float) _deltaTime;
            var fTimeSinceStartUp = (float) _timeSinceStartUp;
            _room?.DoUpdate(fTimeSinceStartUp, fDeltaTime);
        }


        void OnPlayerConnect(Session session, IMessage message){
            //TODO load from db
            var msg = message as Msg_JoinRoom;

            var info = new PlayerServerInfo();
            //根据name  分配英雄 
            info.PrefabId = msg.name[msg.name.Length - 1] - '0';

            msg.name = msg.name + _idCounter;
            var name = msg.name;
            if (_name2Player.TryGetValue(name, out var val)) {
                return;
            }


            info.Id = _idCounter++;
            info.name = name;


            _name2Player[name] = info;
            _id2Player[info.Id] = info;
            _id2Session[info.Id] = session;
            session.BindInfo = info;
            _curCount++;
            if (_curCount >= Room.MaxPlayerCount) {
                _room = new Room();
                _room.Init(0);
                foreach (var player in _id2Player.Values) {
                    _room.OnPlayerJoin(_id2Session[player.Id], player);
                }

                OnGameStart(_room);
            }
            Debug.Log("OnPlayerConnect count:" + _curCount + " Prot: " + session.RemoteAddress.Port + "  " + JsonUtil.ToJson(msg));
        }

        void OnPlayerQuit(Session session, IMessage message){
            Debug.Log("OnPlayerQuit count:" + _curCount);
            var player = session.GetBindInfo<PlayerServerInfo>();
            if (player == null)
                return;
            _id2Player.Remove(player.Id);
            _name2Player.Remove(player.name);
            _id2Session.Remove(player.Id);
            _curCount--;
            if (_curCount == 0) { 
                _room = null;
            }
        }

        void OnPlayerInput(Session session, IMessage message){
            var msg = message as Msg_PlayerInput;
            var player = session.GetBindInfo<PlayerServerInfo>();
            _room?.OnPlayerInput(player.Id, msg);
        }
        void OnPlayerHashCode(Session session, IMessage message){
            var msg = message as Msg_HashCode;
            var player = session.GetBindInfo<PlayerServerInfo>();
            _room?.OnPlayerHashCode(player.Id, msg);
        }

        void OnGameStart(Room room){
            if (room.IsRunning) {
                return;
            }

            room.OnGameStart();
        }
    }
}