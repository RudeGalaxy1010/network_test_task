using App.Network.Api.App.Network.Api;
using Mirror;
using UnityEngine;
using Zenject;

namespace App.Network.Impl.App.Network.Impl {
    public class HelloMessageClientDemo : MonoBehaviour {
        private INetworkMessageClientService _messageService;
        private bool _isSubscribed;
        private string _lastMessage = "No messages";

        [Inject]
        public void Construct(INetworkMessageClientService messageService) {
            _messageService = messageService;
        }

        private void OnGUI() {
            if (!NetworkClient.isConnected) {
                return;
            }

            GUILayout.BeginArea(new Rect(10, 180, 260, 125));

            if (GUILayout.Button("Subscribe HelloMessage")) {
                Subscribe();
            }

            if (GUILayout.Button("Unsubscribe HelloMessage")) {
                Unsubscribe();
            }

            GUILayout.Label($"Received: {_lastMessage}");

            GUILayout.EndArea();
        }

        private void Subscribe() {
            if (_isSubscribed) {
                return;
            }

            _messageService.Subscribe<HelloMessage>(OnHelloMessage);
            _isSubscribed = true;
        }

        private void Unsubscribe() {
            if (!_isSubscribed) {
                return;
            }

            _messageService.Unsubscribe<HelloMessage>(OnHelloMessage);
            _isSubscribed = false;
        }

        private void OnHelloMessage(HelloMessage message) {
            _lastMessage = message.Text;
        }
    }
}
