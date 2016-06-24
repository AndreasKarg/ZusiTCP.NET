using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3
{
  public class CabInfoAddress : Address
  {
    private const short ApplicationId = 0x02;
    private const short CabDataId = 0x0A;

    private readonly short[] _cabInfoSpecificIds;

    public CabInfoAddress(params short[] ids) : base(new[] {ApplicationId, CabDataId}.Concat(ids).ToArray())
    {
      _cabInfoSpecificIds = ids;
    }

    public new CabInfoAddress Concat(short suffix)
    {
      return new CabInfoAddress(CabInfoSpecificIds.Concat(new[] {suffix}).ToArray());
    }

    public IReadOnlyCollection<short> CabInfoSpecificIds
    {
      get { return _cabInfoSpecificIds; }
    }
  }
}
