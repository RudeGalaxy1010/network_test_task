using App.Network.Api.App.Network.Api;
using Mirror;
using UnityEngine;
using Zenject;

namespace App.Network.Impl.App.Network.Impl {
    public class HelloMessageServerDemo : MonoBehaviour {
        private INetworkMessageServerService _messageService;

        [Inject]
        public void Construct(INetworkMessageServerService messageService) {
            _messageService = messageService;
        }

        private void OnGUI() {
            if (!NetworkServer.active) {
                return;
            }

            GUILayout.BeginArea(new Rect(10, 290, 260, 50));

            if (GUILayout.Button("Send Hello to subscribers")) {
                _messageService.SendToSubscribers(new HelloMessage {
                    Text = "Hello Client!"
                });
            }

            GUILayout.EndArea();
        }
    }
}
