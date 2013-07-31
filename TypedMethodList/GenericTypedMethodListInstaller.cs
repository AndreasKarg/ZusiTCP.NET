using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Zusi_Datenausgabe.TypedMethodList
{
  public class GenericTypedMethodListInstaller : IWindsorInstaller
  {
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
      container.Register(
        Classes.FromThisAssembly()
          .InSameNamespaceAs<GenericTypedMethodListInstaller>()
          .LifestyleTransient()
          .WithServiceDefaultInterfaces(),
        Component.For<ITypedMethodListFactory>().AsFactory()
        );
    }
  }
}