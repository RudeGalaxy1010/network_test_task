using System.Collections;
using App.Network.Api.App.Network.Api;
using App.Network.Impl.App.Network.Impl;
using Mirror;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace App.Network.Tests.PlayMode.App.Network.Tests.PlayMode {
    public class NetworkMessageServerServicePlayModeTests {
        private GameObject _transportObject;
        private NetworkMessageClientService _clientService;
        private NetworkMessageServerService _serverService;
        private int _tickRate;
        private int _unreliableBaselineRate;

        [SetUp]
        public void SetUp() {
            _tickRate = NetworkServer.tickRate;
            _unreliableBaselineRate = NetworkServer.unreliableBaselineRate;
            ResetNetworkState();
            NetworkServer.tickRate = 1000000;

            _transportObject = new GameObject("NetworkMessageServerServicePlayModeTests");
            TelepathyTransport transport = _transportObject.AddComponent<TelepathyTransport>();
            Transport.active = transport;

            NetworkServer.listen = false;
            NetworkServer.Listen(1);
            NetworkClient.ConnectHost();

            NetworkClient.connection.isAuthenticated = true;
            NetworkServer.localConnection.isAuthenticated = true;
            HostMode.InvokeOnConnected();

            _serverService = new NetworkMessageServerService();
            _serverService.Initialize();

            _clientService = new NetworkMessageClientService();
            _clientService.Initialize();
        }

        [TearDown]
        public void TearDown() {
            _clientService?.Dispose();
            _serverService?.Dispose();

            ResetNetworkState();
            NetworkServer.tickRate = _tickRate;
            NetworkServer.unreliableBaselineRate = _unreliableBaselineRate;

            if (_transportObject != null) {
                Object.DestroyImmediate(_transportObject);
            }
        }

        private static void ResetNetworkState() {
            NetworkClient.Shutdown();
            NetworkServer.Shutdown();

            if (Transport.active != null) {
                Transport.active.Shutdown();
                Transport.active = null;
            }
        }

        [UnityTest]
        public IEnumerator SendToSubscribers_SubscribedClientReceivesMessage() {
            // Arrange
            int receivedMessages = 0;
            _clientService.Subscribe<HelloMessage>(_ => receivedMessages++);
            yield return null;

            // Act
            _serverService.SendToSubscribers(new HelloMessage());
            yield return null;

            // Assert
            Assert.That(receivedMessages, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator SendToSubscribers_UnsubscribedClientDoesNotReceiveMessage() {
            // Arrange
            int receivedMessages = 0;
            System.Action<HelloMessage> handler = _ => receivedMessages++;
            _clientService.Subscribe(handler);
            yield return null;
            _clientService.Unsubscribe(handler);
            yield return null;

            // Act
            _serverService.SendToSubscribers(new HelloMessage());
            yield return null;

            // Assert
            Assert.That(receivedMessages, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator SendToSubscribers_DifferentMessageTypeDoesNotDeliverMessage() {
            // Arrange
            int receivedMessages = 0;
            _clientService.Subscribe<HelloMessage>(_ => receivedMessages++);
            NetworkClient.RegisterHandler<MessageSubscriptionRequest>(_ => { });
            yield return null;

            // Act
            _serverService.SendToSubscribers(new MessageSubscriptionRequest());
            yield return null;

            // Assert
            Assert.That(receivedMessages, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator ClientDisconnected_SubscriptionIsRemoved() {
            // Arrange
            int receivedMessages = 0;
            _clientService.Subscribe<HelloMessage>(_ => receivedMessages++);
            yield return null;
            NetworkConnectionToClient connection = NetworkServer.localConnection;
            NetworkServer.OnDisconnectedEvent?.Invoke(connection);

            // Act
            _serverService.SendToSubscribers(new HelloMessage());
            yield return null;

            // Assert
            Assert.That(receivedMessages, Is.EqualTo(0));
        }
    }
}
