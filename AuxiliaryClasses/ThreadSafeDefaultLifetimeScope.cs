using System;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle.Scoped;

namespace Zusi_Datenausgabe.AuxiliaryClasses
{
  public class ThreadSafeDefaultLifetimeScope : ILifetimeScope
  {
    private static readonly Action<Burden> EmptyOnAfterCreated = delegate { };
    private readonly object _lock = new object();
    private readonly Action<Burden> _onAfterCreated;
    private IScopeCache _scopeCache;

    public ThreadSafeDefaultLifetimeScope(IScopeCache scopeCache = null, Action<Burden> onAfterCreated = null)
    {
      _scopeCache = scopeCache ?? new ScopeCache();
      _onAfterCreated = onAfterCreated ?? EmptyOnAfterCreated;
    }

    public void Dispose()
    {
      lock (_lock)
      {
        if (_scopeCache == null)
        {
          return;
        }
        var disposableCache = _scopeCache as IDisposable;
        if (disposableCache != null)
        {
          disposableCache.Dispose();
        }
        _scopeCache = null;
      }
    }

    public Burden GetCachedInstance(ComponentModel model, ScopedInstanceActivationCallback createInstance)
    {
      lock (_lock)
      {
        var burden = _scopeCache[model];
        if (burden == null)
        {
          _scopeCache[model] = burden = createInstance(_onAfterCreated);
        }
        return burden;
      }
    }
  }
}