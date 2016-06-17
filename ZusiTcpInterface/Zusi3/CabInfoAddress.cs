using System.Linq;

namespace ZusiTcpInterface.Zusi3
{
  public class CabInfoAddress : Address
  {
    private const short ApplicationId = 0x02;
    private const short CabDataId = 0x0A;

    public CabInfoAddress(params short[] ids) : base(new[] {ApplicationId, CabDataId}.Concat(ids).ToArray())
    {
    }
  }
}
