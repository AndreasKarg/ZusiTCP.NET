using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

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
    public static CodeTypeDeclaration CreateZusiTcpSpecificClientByBaseClass(
        CommandSet commands, Type classBase)
    {
      if (classBase == null) throw new ArgumentException("classBase");
      
      /******* class declaration *******/
      // -> [public?] sealed class ZusiTcpSpecificClient : [classBase]
      var classDeclaration = new CodeTypeDeclaration("ZusiTcpSpecificClient");
      classDeclaration.IsClass = true;
      classDeclaration.TypeAttributes |= TypeAttributes.Sealed;
      classDeclaration.BaseTypes.Add(classBase);
      
      
      /******* CreateCommandSet *******/
      CodeMethodReferenceExpression createCommandSetMethodReference; //Referenct to the Constructor Method
      CodeMemberMethod createCommandSetMethod; //The Constructor Method itself (to be able to add Code-Lines).
      CodeMethodReferenceExpression addMethodReference; //Reference to the Add-Method of the local document.
      {
        // -> private static CommandSet CreateCommandSet() {
        createCommandSetMethod = new CodeMemberMethod(); //Constructor Method
        createCommandSetMethod.Name = "CreateCommandSet";
        createCommandSetMethod.Attributes = MemberAttributes.Private | MemberAttributes.Static;
        createCommandSetMethod.ReturnType = new CodeTypeReference(typeof(CommandSet));
        
        createCommandSetMethodReference = new CodeMethodReferenceExpression {MethodName = createCommandSetMethod.Name};

        // -> CommandSet document;
        var documentVarDeclaration = new CodeVariableDeclarationStatement();
        documentVarDeclaration.Type = new CodeTypeReference(typeof(CommandSet));
        documentVarDeclaration.Name = "document";
        createCommandSetMethod.Statements.Add(documentVarDeclaration);
        var documentVarReference = new CodeVariableReferenceExpression(documentVarDeclaration.Name);

        addMethodReference = new CodeMethodReferenceExpression(
            new CodePropertyReferenceExpression(documentVarReference, "Command"), "Add");

        // -> document = new CommandSet();
        var documentVarAssignment = new CodeAssignStatement(
          documentVarReference,
          new CodeObjectCreateExpression(
            typeof(CommandSet), 
            new CodeExpression[] {}));
        createCommandSetMethod.Statements.Add(documentVarAssignment);
        
        classDeclaration.Members.Add(createCommandSetMethod);
      }

      /******* Constructors *******/
      {
        foreach(var baseCtor in classBase.GetConstructors())
        {
          var overloadedCtor = GenerateOverloadedConstructor(baseCtor, createCommandSetMethodReference);
          classDeclaration.Members.Add(overloadedCtor);
        }
      }

      /******* DataTypes *******/
      {
        Dictionary<string, BaseClassDecoderMethodInfo> definedReadMethods =
          BaseClassDecoderMethodInfo.GetDefinedReadMethods(classBase);
        
        var handlerMethods = new Dictionary<string,
                CodeMemberMethod>();
        
        var receiverMethods = new Dictionary<string,
                CodeMemberMethod>();
        
        foreach(CommandEntry entry in commands.Command)
        {
          string[] descs = entry.Name.Split(new string[]{";"}, StringSplitOptions.None);
          string summary = descs[0];
          
          BaseClassDecoderMethodInfo readMethod = definedReadMethods[entry.Type];
          
          /******* Add Command to the CommandSet *******/
          // -> document.Command.Add(new CommandEntry([entry.ID], [summary], [entry.Type]);
          createCommandSetMethod.Statements.Add(
            new CodeMethodInvokeExpression(
              addMethodReference, new CodeExpression[]
              {
                new CodeObjectCreateExpression(
                  typeof(CommandEntry), 
                  new CodeExpression[]
                  {
                    new CodePrimitiveExpression(entry.ID),
                    new CodePrimitiveExpression(summary),
                    new CodePrimitiveExpression(entry.Type)
                  }
                )
              }
            )
          );
          
          int i = -1;
          
          
          //FIXME: += eine "HandleDATA_" Methode pro Datentyp (=> Dictionary) die die
          // Methode zum Extrahieren der Daten aus dem IBinaryReader aufruft, und anschließend
          // zum Main-Thread Syncronisiert. Die Methode im Main-Thread muss dann durch
          // if-Abfragen die Entsprechenden Werte behandeln. Die If-Blöcke müssen dabei
          // bei jedem Datentyp einzeln gemacht werden, alles andere im Dictionary.
          
          CodeConditionStatement whenMyType;
          CodeMethodReferenceExpression receiverMethodRef;
          var localData = new CodeVariableReferenceExpression("data");
          
          /******* Finds or Creates Received-Methods *******/
          {
            CodeMemberMethod receiverMethod;
            if (receiverMethods.ContainsKey(entry.Type))
            {
              receiverMethod = receiverMethods[entry.Type];
            }
            else
            {
              // -> private final void Received_[entry.Type](object state, DataSet<[readMethod.TypeProcessingMethod.ReturnType]> data)
              receiverMethod = new CodeMemberMethod
              {
                Name = "Recieved_" + entry.Type,
                Attributes = MemberAttributes.Private | MemberAttributes.Final
              };

              receiverMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                typeof(object), "state"));
              
              receiverMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                new CodeTypeReference(typeof(DataSet<>)
                .MakeGenericType(new[] {readMethod.TypeProcessingMethod.ReturnType}))
                , "data"));
              
              classDeclaration.Members.Add(receiverMethod);
              receiverMethods.Add(entry.Type, receiverMethod);
              
              //var localVar = new System.CodeDom.CodeVariableDeclarationStatement();
              //localVar.Name = "data";
              //localVar.Type = 
              //  new System.CodeDom.CodeTypeReference(typeof(Zusi_Datenausgabe.DataSet<>)
              //  .MakeGenericType(new System.Type[] {info.TypeProcessingMethod.ReturnType}));
              //localVar.InitExpression = new System.CodeDom.CodeCastExpression(localVar.Type, 
              //  new System.CodeDom.CodeVariableReferenceExpression(localVar.Name));
              //mth.Statements.Add(localVar);
            }

            receiverMethodRef = new CodeMethodReferenceExpression(
              new CodeThisReferenceExpression(), receiverMethod.Name);
            
            // -> if ([entry.ID] == data.Id)
            whenMyType = new CodeConditionStatement
            {
              Condition = new CodeBinaryOperatorExpression(
                new CodePrimitiveExpression(entry.ID),
                CodeBinaryOperatorType.IdentityEquality /*ValueEquality*/,
                new CodePropertyReferenceExpression(
                  new CodeVariableReferenceExpression("data"), "Id"
            ))};
            receiverMethod.Statements.Add(whenMyType);
          }

          /******* Finds or Creates HandleDATA-Methods *******/
          {
            CodeMemberMethod handlerMethod;
            if (handlerMethods.ContainsKey(entry.Type))
            {
              handlerMethod = handlerMethods[entry.Type];
            }
            else
            {
              // -> private final int HandleDATA_[entry.Type](IBinaryReader input, int id)
              handlerMethod = new CodeMemberMethod
              {
                Name = "HandleDATA_" + entry.Type,
                Attributes = MemberAttributes.Private | MemberAttributes.Final
              };

              handlerMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                typeof(IBinaryReader), "input"));
              handlerMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                typeof(int), "id"));
              handlerMethod.ReturnType = new CodeTypeReference(typeof(int));
              
              classDeclaration.Members.Add(handlerMethod);
              handlerMethods.Add(entry.Type, handlerMethod);
              
              //Save the result of the reader to a local Variable.
              // -> [readMethod.TypeProcessingMethod.ReturnType] data = this.[readMethod.TypeProcessingMethod.Name](input);
              var dataVar = new CodeVariableDeclarationStatement
              {
                Name = "data",
                Type = new CodeTypeReference(readMethod.TypeProcessingMethod.ReturnType),
                InitExpression = new CodeMethodInvokeExpression(
                  new CodeMethodReferenceExpression(
                    new CodeThisReferenceExpression(), readMethod.TypeProcessingMethod.Name),
                  new CodeExpression[]
                  {
                    new CodeVariableReferenceExpression("input") /*,
                    new System.CodeDom.CodeVariableReferenceExpression("id")*/
                  })
              };
              handlerMethod.Statements.Add(dataVar);

              // -> this.PostToHost<[dataVar.Type]>(new ReceiveEvent<[readMethod.TypeProcessingMethod.ReturnType]>(this.[receiverMethodRef.MethodName]), id, data);
              handlerMethod.Statements.Add(new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                      new CodeThisReferenceExpression(), "PostToHost",
                      new[] {dataVar.Type}),
                    new CodeExpression[] {
                      new CodeDelegateCreateExpression(
                        new CodeTypeReference(typeof(ReceiveEvent<>)
                        .MakeGenericType(new[] {readMethod.TypeProcessingMethod.ReturnType})),
                        new CodeThisReferenceExpression(),
                        receiverMethodRef.MethodName),
                      new CodeVariableReferenceExpression("id"),
                      new CodeVariableReferenceExpression("data")
                    }));
              
              // -> return data.[readMethod.ReturnTypeInfo.ReadLengthMethodName]
              handlerMethod.Statements.Add(new CodeMethodReturnStatement(
                new CodePropertyReferenceExpression(localData, 
                readMethod.ReturnTypeInfo.ReadLengthMethodName)));
            }
          }
          
          foreach(SmallPropertyInfo prop in readMethod.ReturnTypeInfo.ValueRepresentatives)
          {
            i++;
            string name = CreateMethodName(descs[Math.Min(descs.Length-1, i)]);

            // TODO: Reformat
            if ((Math.Max(descs.Length - 1, 1) < readMethod.ReturnTypeInfo.ValueRepresentatives.Length) // If less descriptions than needed
                && (i + 1 >= Math.Max(descs.Length - 1, 1))) // And this Item is the last that has one (or no longer has one)
              name = string.Format("{0}_{1}", name, prop.PropertyType.Name);
            
            /******* Add private Field and public getter Property *******/
            // -> private [prop.PropertyType] [name]_;
            var commandField = new CodeMemberField
            {
              Name = name + "_",
              Type = new CodeTypeReference(prop.PropertyType)
            };

            // -> public final [prop.PropertyType] [name] {
            var commandProperty = new CodeMemberProperty
            {
              Name = name,
              HasGet = true,
              HasSet = false,
              Type = new CodeTypeReference(prop.PropertyType),
              Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            
            // -> get { return this.[commandField.Name]; }
            commandProperty.GetStatements.Add(new CodeMethodReturnStatement(
              new CodeFieldReferenceExpression(
              new CodeThisReferenceExpression(), commandField.Name)));

            // -> /// <summary>Returns the current state of [entry.Name]</summary>
            commandProperty.Comments.Add(new CodeCommentStatement("<summary>", true));
            commandProperty.Comments.Add(new CodeCommentStatement(
              string.Format("Returns the current state of {0}.", entry.Name) , true));
            commandProperty.Comments.Add(new CodeCommentStatement("</summary>", true));

            // -> public final event EventHandler [name]_Changed;
            var commandChangedEvent = new CodeMemberEvent
            {
              Name = name + "_Changed",
              Attributes = MemberAttributes.Public | MemberAttributes.Final,
              Type = new CodeTypeReference(typeof (EventHandler))
            };

            classDeclaration.Members.Add(commandField);
            classDeclaration.Members.Add(commandProperty);
            classDeclaration.Members.Add(commandChangedEvent);

            var commandPropertyReference = new CodePropertyReferenceExpression(
                new CodePropertyReferenceExpression(
                localData, "Value"),
                prop.Name);
            
            //if (true) //if EXIST OPERATOR ==(prop.PropertyType, prop.PropertyType)
            {
              var whenValueChanged = new CodeConditionStatement();
              
              //Condition: Not Equals.
              whenValueChanged.Condition =
              new CodeBinaryOperatorExpression(commandPropertyReference,
                CodeBinaryOperatorType.IdentityInequality/*ValueEquality*/,
                new CodeFieldReferenceExpression(
                  new CodeThisReferenceExpression(), commandField.Name));
              
              //Asign Value
              whenValueChanged.TrueStatements.Add(
                new CodeAssignStatement(
                new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), commandField.Name),
                commandPropertyReference));
                
              //Raise Event
              whenValueChanged.TrueStatements.Add(
                new CodeDelegateInvokeExpression(
                new CodeEventReferenceExpression(
                new CodeThisReferenceExpression(), commandChangedEvent.Name),
                new CodeExpression[] {
                new CodeThisReferenceExpression(),
                new CodeObjectCreateExpression(typeof (EventArgs))}
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
      
      createCommandSetMethod.Statements.Add(new CodeMethodReturnStatement(
        new CodeVariableReferenceExpression("document")));
    
      return classDeclaration;
    }

    private static CodeConstructor GenerateOverloadedConstructor(
            ConstructorInfo baseConstructor,
            CodeMethodReferenceExpression createCommandSetMethodReference)
    {
      var codeConstructor = new CodeConstructor
      {
        Attributes = baseConstructor.IsPublic ? MemberAttributes.Public : MemberAttributes.Private
      };

      //ctorN.IsPublic = originCtor.IsPublic;

      bool hasCmdSet = false;

      foreach(ParameterInfo param in baseConstructor.GetParameters())
      {
        if (param.ParameterType == typeof(CommandSet))
        {
          if (hasCmdSet) throw new ArgumentException();

          codeConstructor.BaseConstructorArgs.Add(new CodeMethodInvokeExpression(
            createCommandSetMethodReference));
          hasCmdSet = true;
        }
        else
        {
          codeConstructor.Parameters.Add(new CodeParameterDeclarationExpression(param.ParameterType, param.Name));
          codeConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression(param.Name));
        }
      }

      if (!hasCmdSet) throw new ArgumentException();
      return codeConstructor;
    }

    private static string CreateMethodName(string desc)
    {
      TextInfo nfo = CultureInfo.InvariantCulture.TextInfo;
      
      return nfo.ToTitleCase(desc)
        .Replace(" ", "").Replace("-", "").Replace("ß","ss")
        .Replace("/", "").Replace("\\", "").Replace(",", "").Replace(".", "")
        .Replace("(", "").Replace(")", "")
        .Replace("Ä","Ae").Replace("Ö","Oe").Replace("Ü","Ue")
        .Replace("ä","ae").Replace("ö","oe").Replace("ü","ue");
    }
    
  }
}
