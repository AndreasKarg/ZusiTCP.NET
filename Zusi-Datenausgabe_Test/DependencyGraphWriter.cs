using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Castle.Core;
using Castle.Core.Internal;
using Castle.Windsor;

namespace Zusi_Datenausgabe_Test
{
  public class DependencyGraphWriter
  {
    private readonly IWindsorContainer _container;
    private readonly IndentedTextWriter _writer;

    public DependencyGraphWriter(IWindsorContainer container, IndentedTextWriter writer)
    {
      _container = container;
      _writer = writer;
    }

    public void Output()
    {
      var graphNodes = _container.Kernel.GraphNodes;

      foreach (var graphNode in graphNodes)
      {
        //if (graphNode.Dependents.Length != 0) continue;
        _writer.WriteLine();

        WalkGraph(graphNode, 0);
      }
    }

    private void WalkGraph(IVertex node, int level)
    {
      _writer.Indent++;

      var componentModel = node as ComponentModel;
      if (componentModel != null)
      {
        //_writer.WriteLine("{0} -> {1} ({2})",
        //    componentModel.Services.First().FullName,
        //    componentModel.Implementation.FullName, componentModel.LifestyleType);
        OutputVertex(componentModel);
      }

      foreach (var childNode in node.Adjacencies)
      {
        WalkGraph(childNode, level + 1);
      }

      _writer.Indent--;
    }

    private void OutputVertex(ComponentModel model)
    {
      int index = 0;

      foreach (var service in model.Services)
      {
        OutputLine(service, model, decideOnCurlyType(index, model.Services.Count()));
        index++;
      }
    }

    private void OutputLine(Type service, ComponentModel model, curlyType curly)
    {
      char[] curlyLetters =
      {
        '\\',
        '/',
        '>',
        '|'
      };

      char letter = curlyLetters[(int) curly];

      if(curly != curlyType.Center)
        _writer.WriteLine("{0, -60} {1}", service.FullName, letter);
      else
      {
        _writer.WriteLine("{0, -60} {1} {2} ({3})", service.FullName, letter, model.Implementation.FullName,
          model.LifestyleType);
      }
    }

    private curlyType decideOnCurlyType(int index, int count)
    {
      if(count == 0)
        return curlyType.Center;

      if(index == count/2)
        return curlyType.Center;

      if(index == 0)
        return curlyType.First;

      if(index == count-1)
        return curlyType.Last;

      return curlyType.Other;
    }

    private enum curlyType
    {
      First = 0,
      Last = 1,
      Center = 2,
      Other = 3
    }
  }
}