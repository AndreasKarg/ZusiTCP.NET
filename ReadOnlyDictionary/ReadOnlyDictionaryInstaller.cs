using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Zusi_Datenausgabe.ReadOnlyDictionary
{
  public class ReadOnlyDictionaryInstaller : IWindsorInstaller
  {
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
      container.Register(
        Component.For(typeof (IReadOnlyDictionary<,>))
          .ImplementedBy(typeof (ReadOnlyDictionary<,>))
          .LifestyleTransient(),
        Component.For(typeof (ReadOnlyDictionary<,>)).LifestyleTransient()
        );
    }
  }
}