using System;

using Mirror;

namespace App.Network.Api
{
    public interface INetworkMessageClientService
    {
        void Initialize();

        void Subscribe<T>(Action<T> handler)
            where T : struct, NetworkMessage;

        void Unsubscribe<T>(Action<T> handler)
            where T : struct, NetworkMessage;
    }
}
