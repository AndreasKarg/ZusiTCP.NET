using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using ZusiTcpInterface.Zusi3.Converters;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterface.Zusi3
{
  public class ConnectionCreator
  {
    private DescriptorCollection _descriptors;
    private RootNodeConverter _rootNodeConverter;

    private string _clientName = String.Empty;
    private string _clientVersion = String.Empty;
    private IEnumerable<CabInfoAddress> _neededData = Enumerable.Empty<CabInfoAddress>();
    private IPEndPoint _endPoint = new IPEndPoint(IPAddress.Loopback, 1436);

    public DescriptorCollection Descriptors
    {
      get { return _descriptors; }
    }

    public ConnectionCreator(string cabInfoTypeDescriptorFilename = "Zusi3/CabInfoTypes.xml")
    {
      using (var commandSetFileStream = File.OpenRead(cabInfoTypeDescriptorFilename))
      {
        InitialiseFrom(commandSetFileStream);
      }
    }

    public ConnectionCreator(Stream commandsetFileStream)
    {
      InitialiseFrom(commandsetFileStream);
    }

    public ConnectionCreator(IEnumerable<AttributeDescriptor> descriptors)
    {
      InitialiseFrom(descriptors);
    }

    private void InitialiseFrom(Stream fileStream)
    {
      var descriptors = DescriptorReader.ReadCommandsetFrom(fileStream);
      InitialiseFrom(descriptors);
    }

    private void InitialiseFrom(IEnumerable<AttributeDescriptor> descriptors)
    {
      var descriptorCollection = new DescriptorCollection(descriptors);
      _descriptors = descriptorCollection;
      SetupNodeConverters();
    }

    private void SetupNodeConverters()
    {
      var handshakeConverter = new NodeConverter();
      var ackHelloConverter = new AckHelloConverter();
      var ackNeededDataConverter = new AckNeededDataConverter();
      handshakeConverter.SubNodeConverters[0x02] = ackHelloConverter;

      var cabDataConverter = GenerateNodeConverter(_descriptors);
      var userDataConverter = new NodeConverter();
      userDataConverter.SubNodeConverters[0x04] = ackNeededDataConverter;
      userDataConverter.SubNodeConverters[0x0A] = cabDataConverter;

      _rootNodeConverter = new RootNodeConverter();
      _rootNodeConverter[0x01] = handshakeConverter;
      _rootNodeConverter[0x02] = userDataConverter;
    }

    private INodeConverter GenerateNodeConverter(DescriptorCollection descriptors)
    {
        var attributeConverters = AttributeConverters.MapToDescriptors(descriptors);
        return new FlatteningNodeConverter { ConversionFunctions = attributeConverters };
    }

    public Connection CreateConnection()
    {
      return new Connection(ClientName, ClientVersion, NeededData, EndPoint, _rootNodeConverter);
    }

    public IPEndPoint EndPoint
    {
      get { return _endPoint; }
      set { _endPoint = value; }
    }

    public IEnumerable<CabInfoAddress> NeededData
    {
      get { return _neededData; }
      set { _neededData = value; }
    }

    public string ClientVersion
    {
      get { return _clientVersion; }
      set { _clientVersion = value; }
    }

    public string ClientName
    {
      get { return _clientName; }
      set { _clientName = value; }
    }
  }
}