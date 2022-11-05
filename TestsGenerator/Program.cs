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
            "C:\\Users\\fromt\\RiderProjects\\TestsGenerator\\TestDir\\SecondClass.cs"
        };
        
        var destPath = "C:\\Users\\fromt\\RiderProjects\\TestsGenerator\\TestDir\\Tests";

        //TODO: introduce separate variables for parallel tasks

        var loadCodeFromFile = new TransformBlock<string, string>(async path =>
        {
            using var reader = File.OpenText(path);
            return await reader.ReadToEndAsync();
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

        var generateTestCode = new TransformBlock<string, string>(async srcCode =>
        {
            var gen = new Generator();
            return await gen.GenerateAsync(srcCode);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });
        
        var saveTestFile = new ActionBlock<string>(testCode =>
        {
            using var writer = GenerateTestFilePath(destPath);
            writer.WriteAsync(testCode);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        loadCodeFromFile.LinkTo(generateTestCode, linkOptions);
        generateTestCode.LinkTo(saveTestFile, linkOptions);

        foreach (var p in paths)
            loadCodeFromFile.Post(p);
        
        loadCodeFromFile.Complete();
        saveTestFile.Completion.Wait();
    }

    private static StreamWriter GenerateTestFilePath(string destPath)
    {
        var path = $"{destPath}\\UnitTest.cs";
        var counter = 1;
        lock (_sync)
        {
            while (File.Exists(path))
            {
                path = $"{destPath}\\UnitTest{counter}.cs";
                counter++; 
            }
            return File.AppendText(path);    
        }
    }
}