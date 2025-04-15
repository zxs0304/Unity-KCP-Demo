using System.Net;
using System.Net.Sockets;
using Lockstep.Network;
using LockstepTutorial;
using SGF.Network.KCP;

namespace Lockstep.Logic
{
    public class KcpNetClient : IKcpMessageDispatcher
    {
        private KCPNetworkProxy kcpNetworkProxy = new KCPNetworkProxy();


        private int count = 0;
        public int id;

        public void Start()
        {
            kcpNetworkProxy.Awake(NetworkProtocol.KCP);
            kcpNetworkProxy.MessageDispatcher = this;
            kcpNetworkProxy.MessagePacker = MessagePacker.Instance;

        }


        public void Dispatch(KCPProxy kcpProxy, Packet packet)
        {
            ushort opcode = packet.Opcode();
            var message = MessagePacker.Instance.DeserializeFrom(opcode, packet.Bytes, Packet.Index,
                packet.Length - Packet.Index) as IMessage;
            var type = (EMsgType)opcode;
            switch (type)
            {
                case EMsgType.FrameInput:
                    OnFrameInput(kcpProxy, message);
                    break;
                case EMsgType.StartGame:
                    OnStartGame(kcpProxy, message);
                    break;
            }
        }

        public void OnFrameInput(KCPProxy kcpProxy, IMessage message)
        {
            var msg = message as Msg_FrameInput;
            GameManager.PushFrameInput(msg.input);
        }

        public void OnStartGame(KCPProxy kcpProxy, IMessage message)
        {
            var msg = message as Msg_StartGame;
            GameManager.Instance.StartGame(msg);
        }

        public void Send(IMessage msg)
        {
            kcpNetworkProxy.Send(msg);
        }

        public void Update()
        {
            kcpNetworkProxy.Update();
        }
    }
}