using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zusi_Datenausgabe;
using Zusi_Datenausgabe.EventManager;

namespace Zusi_Datenausgabe_Test
{
  [TestClass]
  public class EventManagerTest
  {
    private IEventManager<int> _manager;

    private static IWindsorContainer _container;

    private int _hitsOnEvent;

    #region Init / Cleanup methods

    [ClassInitialize]
    public static void InitializeClass(TestContext context)
    {
      _container = new WindsorContainer()
        .Install(new WindsorInstaller()
        );

      //_container.Register(Component.For(typeof(EventManager<>)).LifestyleTransient());
    }

    [ClassCleanup]
    public static void CleanupClass()
    {
      _container.Dispose();
    }

    [TestInitialize]
    public void InitializeTest()
    {
      _manager = _container.Resolve<IEventManager<int>>();
      _hitsOnEvent = 0;
    }

    [TestCleanup]
    public void CleanupTest()
    {
      _container.Release(_manager);
    }

    #endregion

    [TestMethod]
    public void TestInvoke()
    {
      const int key = 1234;

      _manager.Subscribe<int>(key, (sender, args) => { _hitsOnEvent++;
                                                       Assert.IsTrue(args.Id == key);
                                                       Assert.AreEqual(sender, this);
      });

      Assert.IsTrue(_hitsOnEvent == 0);

      _manager.Invoke(key, this, new DataReceivedEventArgs<int>(key, 13));

      Assert.IsTrue(_hitsOnEvent == 1);
    }
  }
}
