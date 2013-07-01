using System.Collections;
using System.Collections.Generic;
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
      container.Register(
        Component.For(typeof(IDictionary<,>)).ImplementedBy(typeof(Dictionary<,>)).LifestyleTransient(),
        Classes.FromThisAssembly().Pick()
        .Unless(t => (t == typeof(XmlTcpCommands))
                   /*||(t == typeof(ReadOnlyDictionary<,>))*/)
        .WithServiceFirstInterface()
        .LifestyleTransient(),
        Component.For<IZusiTcpConnectionFactory>().AsFactory(),
        Component.For<INetworkIOHandlerFactory>().AsFactory(),
        Component.For<XmlTcpCommands>().UsingFactoryMethod(GetTCPCommands).LifestyleTransient()//,
        //Component.For<>()
        );
    }

    private XmlTcpCommands GetTCPCommands(IKernel kernel, CreationContext context)
    {
      return (context.HasAdditionalArguments) ? XmlTcpCommands.LoadFromFile((string) context.AdditionalArguments["filePath"]) : new XmlTcpCommands();
    }
  }
}