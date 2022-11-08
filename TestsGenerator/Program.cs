using System.Threading.Tasks.Dataflow;
using TestsGenerator.Core.impl;

namespace TestsGenerator;

public static class Program
{
    private static object _sync = new ();
    public static void Main(string[] args)
    {
        var paths = new List<string>
        {
            "C:\\Users\\fromt\\RiderProjects\\TestsGenerator\\TestDir\\FirstClass.cs",
            "C:\\Users\\fromt\\RiderProjects\\TestsGenerator\\TestDir\\SecondClass.cs",
            "C:\\Users\\fromt\\RiderProjects\\TestsGenerator\\TestDir\\BrokenClass.cs"
            //"C:\\Users\\fromt\\RiderProjects\\TestsGenerator\\TestsGenerator.Core\\impl\\ElementFactory.cs"
        };
        
        var destPath = "C:\\Users\\fromt\\RiderProjects\\TestsGenerator\\TestDir\\Tests";
        //var destPath = "C:\\Users\\fromt\\RiderProjects\\TestsGenerator\\TestsGenerator.Tests";

        const int loadFilesCountLimit = 4;
        const int processingFileCountLimit = 4;
        const int taskCountLimit = 4;
        const int saveFilesCountLimit = 4;
        
        var loadCodeFromFile = new TransformBlock<string, string>(async path =>
        {
            using var reader = File.OpenText(path);
            return await reader.ReadToEndAsync();
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = loadFilesCountLimit });

        var generateTestCode = new TransformBlock<string, string>( srcCode =>
        {
            var gen = new Generator(taskCountLimit);
            return gen.Generate(srcCode);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = processingFileCountLimit });
        
        var saveTestFile = new ActionBlock<string>(testCode =>
        {
            using var writer = GenerateTestFileWriter(destPath);
            writer.WriteAsync(testCode);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = saveFilesCountLimit });

        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        loadCodeFromFile.LinkTo(generateTestCode, linkOptions);
        generateTestCode.LinkTo(saveTestFile, linkOptions);

        foreach (var p in paths)
            loadCodeFromFile.Post(p);
        
        loadCodeFromFile.Complete();
        saveTestFile.Completion.Wait();
    }

    private static StreamWriter GenerateTestFileWriter(string destPath)
    {
        var path = $"{destPath}\\UnitTest.cs";
        var counter = 1;
        lock (_sync)
        {
            while (File.Exists(path)) path = $"{destPath}\\UnitTest{++counter}.cs";
            
            return File.AppendText(path);    
        }
    }
}