using System;
using App.Network.Api.App.Network.Api;
using App.Network.Impl.App.Network.Impl;
using NUnit.Framework;

namespace App.Network.Tests.App.Network.Tests {
    public class NetworkMessageServiceEditModeTests {
        [Test]
        public void Subscribe_BeforeInitialization_ThrowsInvalidOperationException() {
            // Arrange
            NetworkMessageClientService service = new NetworkMessageClientService();

            // Act
            TestDelegate action = () => service.Subscribe<HelloMessage>(_ => { });

            // Assert
            Assert.Throws<InvalidOperationException>(action);
        }

        [Test]
        public void SendToSubscribers_BeforeInitialization_ThrowsInvalidOperationException() {
            // Arrange
            NetworkMessageServerService service = new NetworkMessageServerService();

            // Act
            TestDelegate action = () => service.SendToSubscribers(new HelloMessage());

            // Assert
            Assert.Throws<InvalidOperationException>(action);
        }
    }
}
