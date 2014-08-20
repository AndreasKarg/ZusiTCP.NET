///<summary>Provides Methods to create specialized ZusiTcp-Components.</summary>
namespace Zusi_Datenausgabe.Compyling
{
  ///<summary>Marks a Property to return the length, that was neccessary to extract the data of this class.</summary>
  [System.AttributeUsageAttribute(System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
  public class ReadedLengthAttribute : System.Attribute
  {
  }
  ///<summary>Marks a Property to return data, that was extracted.</summary>
  [System.AttributeUsageAttribute(System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
  public class ReadedDataAttribute : System.Attribute
  {
  }
  
  
  ///<summary>Represents a Method that is able to read datas from a IBiaryReader and is located in a
  /// subclass of ZusiTcpClientAbstract, that will be the base class.</summary>
  public struct BaseClassDecoderMethodInfo
  {
    ///<summary>Creates a new Instance of BaseClassDecoderMethodInfo with the specifyed Arguments.</summary>
    public BaseClassDecoderMethodInfo(System.Reflection.MethodInfo typeProcessingMethod,
            ReadDataTypeInfo returnTypeInfo) : this()
    {
      TypeProcessingMethod = typeProcessingMethod;
      ReturnTypeInfo = returnTypeInfo;
    }
    ///<summary>Represents the Method that reads the datas from the reader.</summary>
    public System.Reflection.MethodInfo TypeProcessingMethod {set; get;}
    ///<summary>Gets informations about the return type.</summary>
    public ReadDataTypeInfo ReturnTypeInfo {set; get;}
    
    
    
    
    ///<summary>Tryes to extract all Methods, reading Data from a IBinaryDataReader.
    /// Methods have to start with "Read" and are sorted by Method Name (minus "Read").
    /// Methods have to take (only) an IBinaryReader and can return any type, that contains one Property
    /// Marked with ReadedLengthAttribute and at least one Property Marked with ReadedDataAttribute.</summary>
    public static System.Collections.Generic.Dictionary<string, BaseClassDecoderMethodInfo> GetDefinedReadMethods(System.Type type)
    {
      var value = new System.Collections.Generic.Dictionary<string, BaseClassDecoderMethodInfo>();
      
      System.Reflection.MethodInfo[] reflectingMethods = type.GetMethods(
        System.Reflection.BindingFlags.InvokeMethod |
        System.Reflection.BindingFlags.NonPublic |
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.Static |
        System.Reflection.BindingFlags.Instance);
      
      foreach(var mth in reflectingMethods)
      {
        if (!mth.Name.StartsWith("Read"))
          continue;
        string name = mth.Name.Substring("Read".Length);
        
        if ((mth.GetParameters().Length == 1) && (!mth.GetParameters()[0].IsOut) && 
            (mth.GetParameters()[0].ParameterType == typeof(Zusi_Datenausgabe.IBinaryReader)))
        {
          ReadDataTypeInfo returnTypeInfo;
          if (!ReadDataTypeInfo.TryParse(mth.ReturnType, out returnTypeInfo)) continue;
          if (returnTypeInfo.ValueRepresentatives.Length == 0) continue;
          
          value.Add(name, new BaseClassDecoderMethodInfo(mth, returnTypeInfo));
          
        }
      }
      
      return value;
    }
  }
  
  
  /* ///<summary>Represents a Method that is able to read datas from a IBinaryReader</summary>
  public struct BinaryDataReadMethodInfo
  {
    ///<summary>Creates a new Instance of BinaryDataReadMethodInfo with the specifyed Arguments.</summary>
    public BinaryDataReadMethodInfo(System.CodeDom.CodeMemberMethod typeProcessingMethod,
            ReadDataTypeInfo returnTypInfo) : this()
    {
      TypeProcessingMethod = typeProcessingMethod;
      ReturnTypInfo = returnTypInfo;
    }
    ///<summary>Represents the Method that reads the datas from the reader.</summary>
    public System.CodeDom.CodeMemberMethod TypeProcessingMethod {set; get;}
    ///<summary>Gets informations about the return type.</summary>
    public ReadDataTypeInfo ReturnTypInfo {set; get;}
    
    
    ///<summary>Tryes to extract all Methods, reading Data from a IBinaryDataReader.
    /// Methods have to start with "Read" and are sorted by Method Name (minus "Read").
    /// Methods have to take (only) an IBinaryReader and can return any type, that contains one Property
    /// Marked with ReadedLengthAttribute and at least one Property Marked with ReadedDataAttribute.
    /// Warning: This Methods will be Dissassebled(!).</summary>
    public static System.Collections.Generic.Dictionary<string, BinaryDataReadMethodInfo> GetDefinedReadMethods(System.Type type)
    {
      var value = new System.Collections.Generic.Dictionary<string, BinaryDataReadMethodInfo>();
      
      System.Reflection.MethodInfo[] reflectingMethods = type.GetMethods(
        System.Reflection.BindingFlags.InvokeMethod |
        System.Reflection.BindingFlags.NonPublic |
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.Static |
        System.Reflection.BindingFlags.Instance);
      
      foreach(var mth in reflectingMethods)
      {
        if (!mth.Name.StartsWith("Read"))
          continue;
        string name = mth.Name.Substring("Read".Length);
        
        if ((mth.GetParameters().Length == 1) && (!mth.GetParameters()[0].IsOut) && 
            (mth.GetParameters()[0].ParameterType == typeof(Zusi_Datenausgabe.IBinaryReader)))
        {
          ReadDataTypeInfo returnTypeInfo;
          if (ReadDataTypeInfo.TryParse(mth.ReturnType, out returnTypeInfo)) continue;
          if (returnTypeInfo.ValueRepresentatives.Length == 0) continue;
          
          //ToDo: Methode Dissassemblieren und mit Key Name dem Dictionary anfügen.
          
        }
      }
      
      return value;
    }
  } */
  ///<summary>Provides informations about a Type that encapsulates Return Types of Datas read from a IBinaryReader</summary>
  public struct ReadDataTypeInfo
  {
    public ReadDataTypeInfo(SmallPropertyInfo[] valueRepresentatives,
          string readLengthMethodName) : this()
    {
      ValueRepresentatives = valueRepresentatives;
      ReadLengthMethodName = readLengthMethodName;
    }
    
    ///<summary>Represents different representaions of the read value.</summary>
    public SmallPropertyInfo[] ValueRepresentatives {set; get;}
    ///<summary>Represents the name of a int-Property, that Represents the read datat length in Bytes.</summary>
    public string ReadLengthMethodName {set; get;}
    
    ///<summary>Analyses a data Type and tries to find all valueRepresentives and the readLengthPropertyName according to 
    /// ReadedLengthAttribute and ReadedDataAttribute.</summary>
    public static bool TryParse(System.Type origin, out ReadDataTypeInfo retVal)
    {
      retVal = new ReadDataTypeInfo();
      
      var RetProp = new System.Collections.Generic.List<SmallPropertyInfo>();
      string lngName = string.Empty;
      
      foreach(var mbr in origin.GetProperties(
        System.Reflection.BindingFlags.GetProperty |
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.Instance))
      {
        if (System.Attribute.IsDefined(mbr, typeof(ReadedLengthAttribute), true))
        {
          if (mbr.PropertyType != typeof(int))
            return false;
          if (!mbr.CanRead)
            return false;
          if (lngName != string.Empty)
            return false;
          lngName = mbr.Name;
        }
        if (System.Attribute.IsDefined(mbr, typeof(ReadedDataAttribute), true))
        {
          RetProp.Add(new SmallPropertyInfo(mbr));
        }
      }
      
      if (lngName == string.Empty) return false;
      retVal = new ReadDataTypeInfo(RetProp.ToArray(), lngName);
      return true;
    }
  }
  ///<summary>Represents small information about a Property.</summary>
  public struct SmallPropertyInfo
  {
    public SmallPropertyInfo(string name, System.Type propertyType) : this()
    {
      Name = name;
      PropertyType = propertyType;
    }
    public SmallPropertyInfo(System.Reflection.PropertyInfo info) : this()
    {
      Name = info.Name;
      PropertyType = info.PropertyType;
    }
    public string Name {set; get;}
    public System.Type PropertyType {set; get;}
  }
}
