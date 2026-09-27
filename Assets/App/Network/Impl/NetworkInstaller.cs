using Zenject;

namespace App.Network.Impl.App.Network.Impl {
    public class NetworkInstaller : MonoInstaller {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<NetworkMessageClientService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<NetworkMessageServerService>()
                .AsSingle()
                .NonLazy();
        }
    }
}
