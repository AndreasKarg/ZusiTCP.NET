namespace Zusi_Datenausgabe.Compyling
{
  ///<summary>Provieds Methods to create Code for specific CommandSets.</summary>
  public static class SpecializedCodeCompyler
  {
    ///<summary>Creates Code for a Client with the specifyed Document using the given base class to
    /// receive Datas from the IBinaryReader.</summary>
    ///<throws ex="System.ArgumentException">Thrown, whenn type is no subclass of ZusiTcpClientAbstract.</throws>
    ///<throws ex="System.ArgumentException">Thrown, whenn at least one constructor of the base class does not 
    /// require exact one CommandSet.</throws>
    public static System.CodeDom.CodeTypeDeclaration CreateZusiTcpSpecificClientByBaseClass(
        Zusi_Datenausgabe.CommandSet commands, System.Type classBase)
    {
      if (classBase == null) throw new System.ArgumentException("classBase");
      
      /******* class declaration *******/
      var ctype = new System.CodeDom.CodeTypeDeclaration("ZusiTcpSpecificClient");
      ctype.IsClass = true;
      ctype.TypeAttributes |= System.Reflection.TypeAttributes.Sealed;
      ctype.BaseTypes.Add(classBase);
      
      
      /******* CreateCommandSet *******/
      System.CodeDom.CodeMethodReferenceExpression crdocmthr; //Referenct to the Constructor Method
      System.CodeDom.CodeMemberMethod crdocmth; //The Constructor Method itself (to be able to add Code-Lines).
      System.CodeDom.CodeMethodReferenceExpression crdoccma; //Reference to the Add-Method of the local document.
      {
        crdocmth = new System.CodeDom.CodeMemberMethod(); //Constructor Method
        crdocmth.Name = "CreateCommandSet";
        crdocmth.Attributes = System.CodeDom.MemberAttributes.Private | System.CodeDom.MemberAttributes.Static;
        crdocmth.ReturnType = new System.CodeDom.CodeTypeReference(typeof(Zusi_Datenausgabe.CommandSet));
        
        crdocmthr = new System.CodeDom.CodeMethodReferenceExpression();
        crdocmthr.MethodName = crdocmth.Name;
        
        var crdocdoc = new System.CodeDom.CodeVariableDeclarationStatement();
        crdocdoc.Type = new System.CodeDom.CodeTypeReference(typeof(Zusi_Datenausgabe.CommandSet));
        crdocdoc.Name = "document";
        crdocmth.Statements.Add(crdocdoc);
        var crdocvar = new System.CodeDom.CodeVariableReferenceExpression(crdocdoc.Name);
        crdoccma = new System.CodeDom.CodeMethodReferenceExpression(
            new System.CodeDom.CodePropertyReferenceExpression(crdocvar, "Command"), "Add");
        var crdocctr = new System.CodeDom.CodeAssignStatement(
          crdocvar,
          new System.CodeDom.CodeObjectCreateExpression(
            typeof(Zusi_Datenausgabe.CommandSet), 
            new System.CodeDom.CodeExpression[] {}));
        crdocmth.Statements.Add(crdocctr);
        
        ctype.Members.Add(crdocmth);
      }
      /******* Constructors *******/
      {
        foreach(var ctor in classBase.GetConstructors())
        {
          var ctor1 = CreateZusiTcpSpecificClientByBaseClassConstructor(ctor, crdocmthr);
          ctype.Members.Add(ctor1);
        }
      }
      /******* DataTypes *******/
      {
        System.Collections.Generic.Dictionary<string, BaseClassDecoderMethodInfo> validTypes =
          BaseClassDecoderMethodInfo.GetDefinedReadMethods(classBase);
        
        var HandleMethods = new System.Collections.Generic.Dictionary<string, 
                System.CodeDom.CodeMemberMethod>();
        
        var ReceivedMethods = new System.Collections.Generic.Dictionary<string, 
                System.CodeDom.CodeMemberMethod>();
        
        foreach(Zusi_Datenausgabe.CommandEntry entry in commands.Command)
        {
          string[] descs = entry.Name.Split(new string[]{";"}, System.StringSplitOptions.None);
          string summary = descs[0];
          
          BaseClassDecoderMethodInfo info = validTypes[entry.Type];
          
          /******* Add Command to the CommandSet *******/
          crdocmth.Statements.Add(
            new System.CodeDom.CodeMethodInvokeExpression(
            crdoccma, new System.CodeDom.CodeExpression[]
            {new System.CodeDom.CodeObjectCreateExpression(
            typeof(Zusi_Datenausgabe.CommandEntry), 
            new System.CodeDom.CodeExpression[] {
            new System.CodeDom.CodePrimitiveExpression(entry.ID),
            new System.CodeDom.CodePrimitiveExpression(summary),
            new System.CodeDom.CodePrimitiveExpression(entry.Type)
            })}));
          
          int i = -1;
          
          
          //FIXME: += eine "HandleDATA_" Methode pro Datentyp (=> Dictionary) die die
          // Methode zum Extrahieren der Daten aus dem IBinaryReader aufruft, und anschließend
          // zum Main-Thread Syncronisiert. Die Methode im Main-Thread muss dann durch
          // if-Abfragen die Entsprechenden Werte behandeln. Die If-Blöcke müssen dabei
          // bei jedem Datentyp einzeln gemacht werden, alles andere im Dictionary.
          
          System.CodeDom.CodeConditionStatement whenMyType;
          System.CodeDom.CodeMethodReferenceExpression refReceived = null;
          var localData = new System.CodeDom.CodeVariableReferenceExpression("data");
          
          /******* Finds or Creates Received-Methods *******/
          {
            System.CodeDom.CodeMemberMethod mth = null;
            if (ReceivedMethods.ContainsKey(entry.Type))
            {
              mth = ReceivedMethods[entry.Type];
            }
            else
            {
              mth = new System.CodeDom.CodeMemberMethod();
              mth.Name = "Received_" + entry.Type;
              mth.Attributes = System.CodeDom.MemberAttributes.Private | System.CodeDom.MemberAttributes.Final;
              mth.Parameters.Add(new System.CodeDom.CodeParameterDeclarationExpression(
                typeof(object), "state"));
              
              mth.Parameters.Add(new System.CodeDom.CodeParameterDeclarationExpression(
                new System.CodeDom.CodeTypeReference(typeof(Zusi_Datenausgabe.DataSet<>)
                .MakeGenericType(new System.Type[] {info.TypeProcessingMethod.ReturnType}))
                , "data"));
              
              ctype.Members.Add(mth);
              ReceivedMethods.Add(entry.Type, mth);
              
              //var localVar = new System.CodeDom.CodeVariableDeclarationStatement();
              //localVar.Name = "data";
              //localVar.Type = 
              //  new System.CodeDom.CodeTypeReference(typeof(Zusi_Datenausgabe.DataSet<>)
              //  .MakeGenericType(new System.Type[] {info.TypeProcessingMethod.ReturnType}));
              //localVar.InitExpression = new System.CodeDom.CodeCastExpression(localVar.Type, 
              //  new System.CodeDom.CodeVariableReferenceExpression(localVar.Name));
              //mth.Statements.Add(localVar);
            }
            refReceived = new System.CodeDom.CodeMethodReferenceExpression(
              new System.CodeDom.CodeThisReferenceExpression(), mth.Name);
            
            whenMyType = new System.CodeDom.CodeConditionStatement();
            whenMyType.Condition = 
              new System.CodeDom.CodeBinaryOperatorExpression(
                new System.CodeDom.CodePrimitiveExpression(entry.ID),
                System.CodeDom.CodeBinaryOperatorType.IdentityEquality/*ValueEquality*/,
                new System.CodeDom.CodePropertyReferenceExpression(
                  new System.CodeDom.CodeVariableReferenceExpression("data"), "Id"));
            mth.Statements.Add(whenMyType);
          }
          /******* Finds or Creates HandleDATA-Methods *******/
          {
            System.CodeDom.CodeMemberMethod mth;
            if (HandleMethods.ContainsKey(entry.Type))
            {
              mth = HandleMethods[entry.Type];
            }
            else
            {
              mth = new System.CodeDom.CodeMemberMethod();
              mth.Name = "HandleDATA_" + entry.Type;
              mth.Attributes = System.CodeDom.MemberAttributes.Private | System.CodeDom.MemberAttributes.Final;
              mth.Parameters.Add(new System.CodeDom.CodeParameterDeclarationExpression(
                typeof(Zusi_Datenausgabe.IBinaryReader), "input"));
              mth.Parameters.Add(new System.CodeDom.CodeParameterDeclarationExpression(
                typeof(int), "id"));
              mth.ReturnType = new System.CodeDom.CodeTypeReference(typeof(int));
              
              ctype.Members.Add(mth);
              HandleMethods.Add(entry.Type, mth);
              
              //Save the result of the reader to a local Variable.
              var localVar = new System.CodeDom.CodeVariableDeclarationStatement();
              localVar.Name = "data";
              localVar.Type = 
                new System.CodeDom.CodeTypeReference(info.TypeProcessingMethod.ReturnType);
              localVar.InitExpression = 
                  new System.CodeDom.CodeMethodInvokeExpression(
                    new System.CodeDom.CodeMethodReferenceExpression(
                      new System.CodeDom.CodeThisReferenceExpression(), info.TypeProcessingMethod.Name),
                  new System.CodeDom.CodeExpression[] {
                    new System.CodeDom.CodeVariableReferenceExpression("input")/*,
                    new System.CodeDom.CodeVariableReferenceExpression("id")*/
                  });
              mth.Statements.Add(localVar);
              mth.Statements.Add(new System.CodeDom.CodeMethodInvokeExpression(
                    new System.CodeDom.CodeMethodReferenceExpression(
                      new System.CodeDom.CodeThisReferenceExpression(), "PostToHost",
                      new System.CodeDom.CodeTypeReference[] {localVar.Type}),
                    new System.CodeDom.CodeExpression[] {
                      new System.CodeDom.CodeDelegateCreateExpression(
                        new System.CodeDom.CodeTypeReference(typeof(Zusi_Datenausgabe.ReceiveEvent<>)
                        .MakeGenericType(new System.Type[] {info.TypeProcessingMethod.ReturnType})),
                        new System.CodeDom.CodeThisReferenceExpression(),
                        refReceived.MethodName),
                      new System.CodeDom.CodeVariableReferenceExpression("id"),
                      new System.CodeDom.CodeVariableReferenceExpression("data")
                    }));
              
              
              mth.Statements.Add(new System.CodeDom.CodeMethodReturnStatement(
                new System.CodeDom.CodePropertyReferenceExpression(localData, 
                info.ReturnTypeInfo.ReadLengthMethodName)));
            }
          }
          
          
          
          foreach(SmallPropertyInfo prop in info.ReturnTypeInfo.ValueRepresentatives)
          {
            i++;
            string name = CreateMethodName(descs[System.Math.Min(descs.Length-1, i)]);
            if ((System.Math.Max(descs.Length - 1, 1) < info.ReturnTypeInfo.ValueRepresentatives.Length) // If less descriptions than needed
                && (i + 1 >= System.Math.Max(descs.Length - 1, 1))) // And this Item is the last that has one (or no longer has one)
              name = string.Format("{0}_{1}", name, prop.PropertyType.Name);
            
            
            /******* Add private Field and public getter Property *******/
            var varl = new System.CodeDom.CodeMemberField();
            varl.Name = name + "_";
            varl.Type = new System.CodeDom.CodeTypeReference(prop.PropertyType);
            
            var propy = new System.CodeDom.CodeMemberProperty();
            propy.Name = name;
            propy.HasGet = true;
            propy.HasSet = false;
            propy.Type = new System.CodeDom.CodeTypeReference(prop.PropertyType);
            propy.Attributes = System.CodeDom.MemberAttributes.Public | System.CodeDom.MemberAttributes.Final;
            propy.GetStatements.Add(new System.CodeDom.CodeMethodReturnStatement(
              new System.CodeDom.CodeFieldReferenceExpression(
              new System.CodeDom.CodeThisReferenceExpression(), varl.Name)));
            propy.Comments.Add(new System.CodeDom.CodeCommentStatement("<summary>", true));
            propy.Comments.Add(new System.CodeDom.CodeCommentStatement(
              string.Format("Returns the current state of {0}.", entry.Name) , true));
            propy.Comments.Add(new System.CodeDom.CodeCommentStatement("</summary>", true));
            
            var evnt = new System.CodeDom.CodeMemberEvent();
            evnt.Name = name + "_Changed";
            evnt.Attributes = System.CodeDom.MemberAttributes.Public | System.CodeDom.MemberAttributes.Final;
            evnt.Type = new System.CodeDom.CodeTypeReference(typeof(System.EventHandler));
            
            ctype.Members.Add(varl);
            ctype.Members.Add(propy);
            ctype.Members.Add(evnt);
            
            var pref = new System.CodeDom.CodePropertyReferenceExpression(
                new System.CodeDom.CodePropertyReferenceExpression(
                localData, "Value"),
                prop.Name);
            
            //if (true) //if EXIST OPERATOR ==(prop.PropertyType, prop.PropertyType)
            {
              var whenValueChanged = new System.CodeDom.CodeConditionStatement();
              
              //Condition: Not Equals.
              whenValueChanged.Condition =
              new System.CodeDom.CodeBinaryOperatorExpression(pref,
                System.CodeDom.CodeBinaryOperatorType.IdentityInequality/*ValueEquality*/,
                new System.CodeDom.CodeFieldReferenceExpression(
                  new System.CodeDom.CodeThisReferenceExpression(), varl.Name));
              
              //Asign Value
              whenValueChanged.TrueStatements.Add(
                new System.CodeDom.CodeAssignStatement(
                new System.CodeDom.CodeFieldReferenceExpression(
                new System.CodeDom.CodeThisReferenceExpression(), varl.Name),
                pref));
                
              //Raise Event
              whenValueChanged.TrueStatements.Add(
                new System.CodeDom.CodeDelegateInvokeExpression( 
                new System.CodeDom.CodeEventReferenceExpression(
                new System.CodeDom.CodeThisReferenceExpression(), evnt.Name),
                new System.CodeDom.CodeExpression[] {
                new System.CodeDom.CodeThisReferenceExpression(),
                new System.CodeDom.CodeObjectCreateExpression(typeof (System.EventArgs))}
                ));
              
              whenMyType.TrueStatements.Add(whenValueChanged);
            }
            //else
            //{
            //  //Asign Value
            //  whenMyType.TrueStatements.Add(
            //    new System.CodeDom.CodeAssignStatement(
            //    new System.CodeDom.CodeFieldReferenceExpression(
            //    new System.CodeDom.CodeThisReferenceExpression(), varl.Name),
            //    pref));
            //  
            //  //Raise Event
            //  whenMyType.TrueStatements.Add(
            //    new System.CodeDom.CodeDelegateInvokeExpression( 
            //    new System.CodeDom.CodeEventReferenceExpression(
            //    new System.CodeDom.CodeThisReferenceExpression(), evnt.Name),
            //    new System.CodeDom.CodeExpression[] {
            //    new System.CodeDom.CodeThisReferenceExpression(),
            //    new System.CodeDom.CodeObjectCreateExpression(typeof (System.EventArgs))}
            //    ));
            //}
          }
        }
      }
      
      crdocmth.Statements.Add(new System.CodeDom.CodeMethodReturnStatement(
        new System.CodeDom.CodeVariableReferenceExpression("document")));
    
      return ctype;
    }
    private static System.CodeDom.CodeConstructor CreateZusiTcpSpecificClientByBaseClassConstructor(
            System.Reflection.ConstructorInfo originCtor,
            System.CodeDom.CodeMethodReferenceExpression createDocument)
    {
      var ctorN = new System.CodeDom.CodeConstructor();
      if (originCtor.IsPublic)
        ctorN.Attributes = System.CodeDom.MemberAttributes.Public;
      else
        ctorN.Attributes = System.CodeDom.MemberAttributes.Private;
      //ctorN.IsPublic = originCtor.IsPublic;
      bool hasCmdSet = false;
      foreach(System.Reflection.ParameterInfo param in originCtor.GetParameters())
      {
        if (param.ParameterType == typeof(Zusi_Datenausgabe.CommandSet))
        {
          if (hasCmdSet) throw new System.ArgumentException();
          ctorN.BaseConstructorArgs.Add(new System.CodeDom.CodeMethodInvokeExpression(
            createDocument));
          hasCmdSet = true;
        }
        else
        {
          ctorN.Parameters.Add(new System.CodeDom.CodeParameterDeclarationExpression(param.ParameterType, param.Name));
          ctorN.BaseConstructorArgs.Add(new System.CodeDom.CodeVariableReferenceExpression(param.Name));
        }
      }
      if (!hasCmdSet) throw new System.ArgumentException();
      return ctorN;
    }
    private static string CreateMethodName(string desc)
    {
      System.Globalization.TextInfo nfo = System.Globalization.CultureInfo.InvariantCulture.TextInfo;
      
      return nfo.ToTitleCase(desc)
        .Replace(" ", "").Replace("-", "").Replace("ß","ss")
        .Replace("/", "").Replace("\\", "").Replace(",", "").Replace(".", "")
        .Replace("(", "").Replace(")", "")
        .Replace("Ä","Ae").Replace("Ö","Oe").Replace("Ü","Ue")
        .Replace("ä","ae").Replace("ö","oe").Replace("ü","ue");
    }
    
  }
}
