using System;
using System.Collections.Generic;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Zusi_Datenausgabe.DataReader;
using Zusi_Datenausgabe.EventManager;
using Zusi_Datenausgabe.NetworkIO;
using Zusi_Datenausgabe.TcpCommands;
using Zusi_Datenausgabe.TypedMethodList;

namespace Zusi_Datenausgabe
{
  public class WindsorInstaller : IWindsorInstaller
  {
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
      container.AddFacility<TypedFactoryFacility>();
      container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

      container.Install(
        new TcpCommandsInstaller(),
        new EventManagerInstaller(),
        new DataReaderInstaller(),
        new NetworkIOInstaller(),
        new GenericTypedMethodListInstaller()
        );

      container.Register(
        Component.For(typeof (IDictionary<,>)).ImplementedBy(typeof (Dictionary<,>)).LifestyleTransient(),

        Classes.FromThisAssembly().InSameNamespaceAs<WindsorInstaller>()
          .Unless(t => (t == typeof (ZusiTcpClientConnectionNoWindsor)))
          .WithServiceFirstInterface()
          .LifestyleTransient(),

        Component.For<IZusiTcpConnectionFactory>().AsFactory()
        );
    }
  }
}