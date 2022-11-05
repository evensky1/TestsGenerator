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
        var gen = new Generator(12);
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

                             public void Method1() 
                             {
                                Console.WriteLine(""Hello, World!"");
                             }     
                             public void Method2() 
                             {
                                Console.WriteLine(""Hello, World!"");
                             }     
                             public void Method3() 
                             {
                                Console.WriteLine(""Hello, World!"");
                             }     

                             public void Method4() 
                             {
                                Console.WriteLine(""Hello, World!"");
                             }            
                         }
                     }
                     namespace MyHelloWorld.AnotherOne
                     {
                         class MyAnotherProgram
                         {
                             public void Method5() 
                             {
                                Console.WriteLine(""Hello, World!"");
                             }     
                             public void Method6() 
                             {
                                Console.WriteLine(""Hello, World!"");
                             }     
                             public void Method7() 
                             {
                                Console.WriteLine(""Hello, World!"");
                             }     

                             public void Method8() 
                             {
                                Console.WriteLine(""Hello, World!"");
                             }            
                         }
                         class Something
                         {
                             public void Method9() 
                             {
                                Console.WriteLine(""Hello, World!"");
                             }     
                             public void Method10() 
                             {
                                Console.WriteLine(""Hello, World!"");
                             }     
                             public void Method11() 
                             {
                                Console.WriteLine(""Hello, World!"");
                             }     

                             public void Method12() 
                             {
                                Console.WriteLine(""Hello, World!"");
                             }            
                         }
                     }";
        
        var str = gen.GenerateAsync(code);
        Console.WriteLine(str);
        
        Assert.That(true);
    }
}