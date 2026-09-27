using Mirror;

namespace App.Network.Impl
{
    public struct MessageSubscriptionRequest : NetworkMessage
    {
        public ushort MessageTypeId;
        public bool IsSubscribed;
    }
}
