using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Zusi_Datenausgabe.DataReader
{
  public class DataReaderInstaller : IWindsorInstaller
  {
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
      container.Register(
        Component.For<IDataReceptionHandlerFactory>().AsFactory(),

        Classes.FromThisAssembly()
          .BasedOn<IDataReader>()
          .LifestyleBoundToNearest<DataReceptionHandler>()
          .WithServiceBase(),

          Component.For<IDataReceptionHandler>().ImplementedBy<DataReceptionHandler>().LifestyleTransient(),
          Component.For<IDataReaderDictionary>().ImplementedBy<DataReaderDictionary>().LifestyleTransient()
        );
    }
  }
}