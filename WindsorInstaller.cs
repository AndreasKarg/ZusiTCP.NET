using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Zusi_Datenausgabe
{
  public class WindsorInstaller : IWindsorInstaller
  {
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
      container.Register()
    }
  }
}