namespace TestsGenerator.Tests.TestFiles;

class OverloadedClass
{
    private string str = "Hello";

    public void OverloadedMethod(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }

    public void OverloadedMethod(string arg)
    {
        Console.WriteLine("Hello, World!");
    }

    public void OverloadedMethod(int count)
    {
        Console.WriteLine("Hello, World!");
    }

    public void OverloadedMethod(char ch)
    {
        Console.WriteLine("Hello, World!");
    }
}