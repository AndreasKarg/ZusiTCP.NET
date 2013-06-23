using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
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
      container.Register(Component.For<TCPCommands>().UsingFactoryMethod(GetTCPCommands));

    }

    private TCPCommands GetTCPCommands(IKernel kernel, CreationContext context)
    {
      return (context.HasAdditionalArguments) ? TCPCommands.LoadFromFile((string) context.AdditionalArguments["filePath"]) : new TCPCommands();
    }
  }
}