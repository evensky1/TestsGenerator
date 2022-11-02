using TestsGenerator.Core.impl;

namespace TestsGenerator.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var gen = new Generator();
        var code = @"using System;
                     using System.Collections;
                     using System.Linq;
                     using System.Text;
 
                     namespace HelloWorld
                     {
                         class Program
                         {
                             static void Main(string[] args)
                              {
                                 Console.WriteLine(""Hello, World!"");
                             }
                         }
                     }";
        
        gen.GenerateAsync(code);
        
        Assert.That(true);
    }
}