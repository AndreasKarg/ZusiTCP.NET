using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Zusi_Datenausgabe.TcpCommands
{
  public class TcpCommandsInstaller : IWindsorInstaller
  {
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
      container.Register(
        Classes.FromThisAssembly()
          .InSameNamespaceAs<TcpCommandsInstaller>()
          .LifestyleTransient()
          .WithServiceFirstInterface(),

          Component.For<XmlTcpCommands>().UsingFactoryMethod(GetTCPCommands).LifestyleTransient(),
          Component.For<ITcpCommandDictionaryFactory>().AsFactory()
        );
    }

    private XmlTcpCommands GetTCPCommands(IKernel kernel, CreationContext context)
    {
      return (context.HasAdditionalArguments) ? TcpCommands.XmlTcpCommands.LoadFromFile((string)context.AdditionalArguments["filePath"]) : new TcpCommands.XmlTcpCommands();
    }
  }
}