using System.Collections.Generic;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Zusi_Datenausgabe.DataReader;
using Zusi_Datenausgabe.EventManager;
using Zusi_Datenausgabe.NetworkIO;
using Zusi_Datenausgabe.TypedMethodList;

namespace Zusi_Datenausgabe
{
  public class WindsorInstaller : IWindsorInstaller
  {
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
      container.AddFacility<TypedFactoryFacility>();
      container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));
      container.Register(
        Component.For(typeof (EventMarshal<>)).LifestyleTransient(),

        Component.For(typeof (IDictionary<,>)).ImplementedBy(typeof (Dictionary<,>)).LifestyleTransient(),
        Classes.FromThisAssembly().Pick()
          .Unless(t => (t == typeof (XmlTcpCommands))
                       || (t == typeof (ZusiTcpClientConnectionNoWindsor)))
          .WithServiceFirstInterface()
          .LifestyleTransient(),

        Component.For<IZusiTcpConnectionFactory>().AsFactory(),
        Component.For<INetworkIOHandlerFactory>().AsFactory(),
        Component.For<ITypedMethodListFactory>().AsFactory(),
        Component.For<IDataReceptionHandlerFactory>().AsFactory(),
        Component.For<IEventMarshalFactory>().AsFactory(),

        Component.For<XmlTcpCommands>().UsingFactoryMethod(GetTCPCommands).LifestyleTransient()
        );
    }

    private XmlTcpCommands GetTCPCommands(IKernel kernel, CreationContext context)
    {
      return (context.HasAdditionalArguments) ? XmlTcpCommands.LoadFromFile((string) context.AdditionalArguments["filePath"]) : new XmlTcpCommands();
    }
  }
}