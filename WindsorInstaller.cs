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
        .Unless(t => t == typeof(XmlTcpCommands))
        .WithServiceDefaultInterfaces()
        .LifestyleTransient());
      container.Register(Component.For<IZusiTcpConnectionFactory>().AsFactory());
      container.Register(Component.For<XmlTcpCommands>().UsingFactoryMethod(GetTCPCommands));

    }

    private XmlTcpCommands GetTCPCommands(IKernel kernel, CreationContext context)
    {
      return (context.HasAdditionalArguments) ? XmlTcpCommands.LoadFromFile((string) context.AdditionalArguments["filePath"]) : new XmlTcpCommands();
    }
  }
}