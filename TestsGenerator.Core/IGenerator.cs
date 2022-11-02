namespace TestsGenerator.Core;

public interface IGenerator
{
    Task<string> GenerateAsync(string sourceCode);
}