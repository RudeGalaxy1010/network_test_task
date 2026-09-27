using System;
using System.Collections.Generic;
using App.Network.Api.App.Network.Api;
using Mirror;
using Zenject;

namespace App.Network.Impl.App.Network.Impl {
    public class NetworkMessageServerService : INetworkMessageServerService, IInitializable, IDisposable {
        private readonly Dictionary<int, HashSet<ushort>> _subscriptions =
            new Dictionary<int, HashSet<ushort>>();

        private bool _isInitialized;

        public void Initialize() {
            if (_isInitialized) {
                return;
            }

            NetworkServer.ReplaceHandler<MessageSubscriptionRequest>(OnSubscriptionRequest);
            NetworkServer.OnDisconnectedEvent += OnClientDisconnected;
            _isInitialized = true;
        }

        public void Dispose() {
            if (_isInitialized) {
                NetworkServer.OnDisconnectedEvent -= OnClientDisconnected;
                _isInitialized = false;
            }

            _subscriptions.Clear();
        }

        public void SendToSubscribers<T>(T message)
            where T : struct, NetworkMessage {
            EnsureInitialized();

            ushort messageTypeId = NetworkMessages.GetId<T>();

            foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values) {
                if (connection != null && IsSubscribed(connection.connectionId, messageTypeId)) {
                    connection.Send(message);
                }
            }
        }

        private void OnSubscriptionRequest(
            NetworkConnectionToClient connection,
            MessageSubscriptionRequest request) {
            if (!_subscriptions.TryGetValue(connection.connectionId, out HashSet<ushort> messageTypeIds)) {
                if (!request.IsSubscribed) {
                    return;
                }

                messageTypeIds = new HashSet<ushort>();
                _subscriptions.Add(connection.connectionId, messageTypeIds);
            }

            if (request.IsSubscribed) {
                messageTypeIds.Add(request.MessageTypeId);
            }
            else {
                messageTypeIds.Remove(request.MessageTypeId);

                if (messageTypeIds.Count == 0) {
                    _subscriptions.Remove(connection.connectionId);
                }
            }
        }

        private void OnClientDisconnected(NetworkConnectionToClient connection) {
            _subscriptions.Remove(connection.connectionId);
        }

        private bool IsSubscribed(int connectionId, ushort messageTypeId) {
            return _subscriptions.TryGetValue(connectionId, out HashSet<ushort> messageTypeIds) && messageTypeIds.Contains(messageTypeId);
        }

        private void EnsureInitialized() {
            if (!_isInitialized) {
                throw new InvalidOperationException("Network message server service is not initialized.");
            }
        }
    }
}
