using SGF.Network.KCP;

namespace Lockstep.Network
{
    public interface IKcpMessageDispatcher
    {
        void Dispatch(KCPProxy proxy, Packet packet);
    }
}