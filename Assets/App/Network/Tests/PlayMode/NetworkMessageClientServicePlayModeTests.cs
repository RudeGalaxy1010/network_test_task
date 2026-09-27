using System;
using System.Collections;
using App.Network.Api.App.Network.Api;
using App.Network.Impl.App.Network.Impl;
using Mirror;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace App.Network.Tests.PlayMode.App.Network.Tests.PlayMode {
    public class NetworkMessageClientServicePlayModeTests {
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

            _transportObject = new GameObject("NetworkMessageClientServicePlayModeTests");
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
                UnityEngine.Object.DestroyImmediate(_transportObject);
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
        public IEnumerator Subscribe_MessageIsReceived() {
            // Arrange
            int receivedMessages = 0;

            // Act
            _clientService.Subscribe<HelloMessage>(_ => receivedMessages++);
            yield return null;
            _serverService.SendToSubscribers(new HelloMessage());
            yield return null;

            // Assert
            Assert.That(receivedMessages, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Subscribe_SameHandlerTwice_MessageIsReceivedOnce() {
            // Arrange
            int receivedMessages = 0;
            Action<HelloMessage> handler = _ => receivedMessages++;

            // Act
            _clientService.Subscribe(handler);
            _clientService.Subscribe(handler);
            yield return null;
            _serverService.SendToSubscribers(new HelloMessage());
            yield return null;

            // Assert
            Assert.That(receivedMessages, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Subscribe_TwoHandlers_BothReceiveMessage() {
            // Arrange
            int firstHandlerCalls = 0;
            int secondHandlerCalls = 0;

            // Act
            _clientService.Subscribe<HelloMessage>(_ => firstHandlerCalls++);
            _clientService.Subscribe<HelloMessage>(_ => secondHandlerCalls++);
            yield return null;
            _serverService.SendToSubscribers(new HelloMessage());
            yield return null;

            // Assert
            Assert.That(firstHandlerCalls, Is.EqualTo(1));
            Assert.That(secondHandlerCalls, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Unsubscribe_LastHandler_MessageIsNotReceived() {
            // Arrange
            int receivedMessages = 0;
            Action<HelloMessage> handler = _ => receivedMessages++;
            _clientService.Subscribe(handler);
            yield return null;

            // Act
            _clientService.Unsubscribe(handler);
            yield return null;
            _serverService.SendToSubscribers(new HelloMessage());
            yield return null;

            // Assert
            Assert.That(receivedMessages, Is.Zero);
        }

        [UnityTest]
        public IEnumerator UnsubscribeThenSubscribe_MessageIsReceivedAgain() {
            // Arrange
            int receivedMessages = 0;
            Action<HelloMessage> handler = _ => receivedMessages++;
            _clientService.Subscribe(handler);
            yield return null;
            _clientService.Unsubscribe(handler);
            yield return null;

            // Act
            _clientService.Subscribe(handler);
            yield return null;
            _serverService.SendToSubscribers(new HelloMessage());
            yield return null;

            // Assert
            Assert.That(receivedMessages, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Subscribe_DifferentMessageTypes_ReceiveMatchingMessages() {
            // Arrange
            int helloMessages = 0;
            int subscriptionMessages = 0;

            // Act
            _clientService.Subscribe<HelloMessage>(_ => helloMessages++);
            _clientService.Subscribe<MessageSubscriptionRequest>(_ => subscriptionMessages++);
            yield return null;
            _serverService.SendToSubscribers(new HelloMessage());
            _serverService.SendToSubscribers(new MessageSubscriptionRequest());
            yield return null;

            // Assert
            Assert.That(helloMessages, Is.EqualTo(1));
            Assert.That(subscriptionMessages, Is.EqualTo(1));
        }
    }
}
