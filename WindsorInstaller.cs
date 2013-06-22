using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Zusi_Datenausgabe
{
  public class WindsorInstaller : IWindsorInstaller
  {
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
      container.AddFacility<TypedFactoryFacility>();
      container.Register(Classes.FromThisAssembly().Pick()
        .Unless(t => t == typeof(TCPCommands))
        .WithServiceDefaultInterfaces()
        .LifestyleTransient());
      container.Register(Component.For<IZusiTcpConnectionFactory>().AsFactory());
    }
  }
}