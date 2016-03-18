using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZusiTcpInterface.Zusi3
{
  public class ConnectionContainer
  {
    public ConnectionContainer(string cabInfoTypeDescriptorFilename = "CabInfoTypes.csv")
    {
      IEnumerable<CabInfoTypeDescriptor> cabInfoDescriptors;
      using (var commandSetFileStream = File.OpenRead(cabInfoTypeDescriptorFilename))
      {
        cabInfoDescriptors = CabInfoTypeDescriptorReader.ReadCommandsetFrom(commandSetFileStream);
      }

      var cabInfoConversionFunctions = GenerateConversionFunctions(cabInfoDescriptors);

      var handshakeConverter = new BranchingNodeConverter();
      var ackHelloConverter = new AckHelloConverter();
      var ackNeededDataConverter = new AckNeededDataConverter();
      handshakeConverter[0x02] = ackHelloConverter;

      var cabDataConverter = new CabDataConverter(cabInfoConversionFunctions);
      var userDataConverter = new BranchingNodeConverter();
      userDataConverter[0x04] = ackNeededDataConverter;
      userDataConverter[0x0A] = cabDataConverter;

      TopLevelNodeConverter topLevelNodeConverter = new TopLevelNodeConverter();
      topLevelNodeConverter[0x01] = handshakeConverter;
      topLevelNodeConverter[0x02] = userDataConverter;
    }

    private Dictionary<short, Func<short, byte[], IProtocolChunk>> GenerateConversionFunctions(IEnumerable<CabInfoTypeDescriptor> cabInfoDescriptors)
    {
      var descriptorToConversionFunctionMap = new Dictionary<string, Func<short, byte[], IProtocolChunk>>()
      {
        {"single", CabDataAttributeConverters.ConvertSingle},
        {"boolassingle", CabDataAttributeConverters.ConvertBoolAsSingle},
        {"fail", (s, bytes) => {throw new NotSupportedException("Unsupported data type received");} }
      };

      return cabInfoDescriptors.ToDictionary(descriptor => descriptor.Id,
                                             descriptor => descriptorToConversionFunctionMap[descriptor.Type.ToLowerInvariant()]);
    }
  }
}