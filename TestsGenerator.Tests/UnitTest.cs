using TestsGenerator.Core.impl;

namespace TestsGenerator.Tests;

public class Tests
{
    private const string PathToDataSource =
        "C:\\Users\\fromt\\RiderProjects\\TestsGenerator\\TestsGenerator.Tests\\TestFiles";
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test_Generation_Default_Scenario()
    {
        var gen = new Generator(5);
        var code = File.ReadAllText($"{PathToDataSource}\\TestFile1.cs");
        var tests = gen.Generate(code);
        var expected = File.ReadAllText($"{PathToDataSource}\\Results\\ResultFile1.cs");
        Assert.That(tests.Length, Is.EqualTo(expected.Length));
    }
    
    [Test]
    public void Test_Generation_With_Inner_Classes()
    {
        var gen = new Generator(5);
        var code = File.ReadAllText($"{PathToDataSource}\\TestFile2.cs");
        var tests = gen.Generate(code);
        var expected = File.ReadAllText($"{PathToDataSource}\\Results\\ResultFile2.cs");
        Assert.That(tests.Length, Is.EqualTo(expected.Length));
    }
    
    [Test]
    public void Test_Generation_With_Method_Overloading()
    {
        var gen = new Generator(5);
        var code = File.ReadAllText($"{PathToDataSource}\\TestFile3.cs");
        var tests = gen.Generate(code);
        var expected = File.ReadAllText($"{PathToDataSource}\\Results\\ResultFile3.cs");
        Assert.That(tests.Length, Is.EqualTo(expected.Length));
    }
}