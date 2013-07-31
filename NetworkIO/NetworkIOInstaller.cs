using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Zusi_Datenausgabe.NetworkIO
{
  public class NetworkIOInstaller : IWindsorInstaller
  {
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
      container.Register(
        Classes.FromThisAssembly()
          .InSameNamespaceAs<NetworkIOInstaller>()
          .LifestyleTransient()
          .WithServiceAllInterfaces(),
        Component.For<INetworkIOHandlerFactory>().AsFactory()
        );
    }
  }
}