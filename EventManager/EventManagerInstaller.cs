using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Zusi_Datenausgabe.EventManager
{
  public class EventManagerInstaller : IWindsorInstaller
  {
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
      container.Register(
        Classes.FromThisAssembly()
          .InSameNamespaceAs<EventManagerInstaller>()
          .LifestyleTransient()
          .WithServiceDefaultInterfaces(),
        Component.For<IEventMarshalFactory>().AsFactory()
        );
    }
  }
}