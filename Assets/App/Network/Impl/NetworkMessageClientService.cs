using System;
using System.Collections.Generic;
using System.Linq;
using App.Network.Api.App.Network.Api;
using Mirror;
using Zenject;

namespace App.Network.Impl.App.Network.Impl {
    public class NetworkMessageClientService : INetworkMessageClientService, IInitializable, IDisposable {
        private readonly Dictionary<ushort, Delegate> _handlers = new Dictionary<ushort, Delegate>();

        private bool _isInitialized;

        public void Initialize() {
            if (_isInitialized) {
                return;
            }

            _isInitialized = true;
        }

        public void Subscribe<T>(Action<T> handler)
            where T : struct, NetworkMessage {
            EnsureInitialized();
            EnsureConnected();

            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }

            ushort messageTypeId = NetworkMessages.GetId<T>();

            if (_handlers.TryGetValue(messageTypeId, out Delegate currentHandler)) {
                if (ContainsHandler(currentHandler, handler)) {
                    return;
                }

                _handlers[messageTypeId] = Delegate.Combine(currentHandler, handler);
                return;
            }

            NetworkClient.ReplaceHandler<T>(HandleMessage);
            _handlers.Add(messageTypeId, handler);
            SendSubscriptionRequest(messageTypeId, true);
        }

        public void Dispose() {
            _handlers.Clear();
            _isInitialized = false;
        }

        public void Unsubscribe<T>(Action<T> handler)
            where T : struct, NetworkMessage {
            EnsureInitialized();
            EnsureConnected();

            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }

            ushort messageTypeId = NetworkMessages.GetId<T>();

            if (!_handlers.TryGetValue(messageTypeId, out Delegate currentHandler)) {
                return;
            }

            Delegate updatedHandler = Delegate.Remove(currentHandler, handler);

            if (updatedHandler != null) {
                _handlers[messageTypeId] = updatedHandler;
                return;
            }

            _handlers.Remove(messageTypeId);
            SendSubscriptionRequest(messageTypeId, false);
        }

        private void HandleMessage<T>(T message)
            where T : struct, NetworkMessage {
            ushort messageTypeId = NetworkMessages.GetId<T>();

            if (_handlers.TryGetValue(messageTypeId, out Delegate handler)) {
                ((Action<T>)handler)(message);
            }
        }

        private static void SendSubscriptionRequest(ushort messageTypeId, bool isSubscribed) {
            NetworkClient.connection.Send(new MessageSubscriptionRequest {
                MessageTypeId = messageTypeId,
                IsSubscribed = isSubscribed
            });
        }

        private void EnsureInitialized() {
            if (!_isInitialized) {
                throw new InvalidOperationException("Network message client service is not initialized.");
            }
        }

        private static bool ContainsHandler(Delegate currentHandler, Delegate handler) => currentHandler
            .GetInvocationList()
            .Contains(handler);

        private static void EnsureConnected() {
            if (!NetworkClient.isConnected) {
                throw new InvalidOperationException("Network client is not connected.");
            }
        }
    }
}
