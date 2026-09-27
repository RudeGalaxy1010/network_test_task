using Mirror;

namespace App.Network.Api
{
    public interface INetworkMessageServerService
    {
        void Initialize();

        void SendToSubscribers<T>(T message)
            where T : struct, NetworkMessage;
    }
}
