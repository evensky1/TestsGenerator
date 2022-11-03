using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks.Dataflow;
using TestsGenerator.Core.impl;

namespace TestsGenerator;

public static class Program
{
    public static void Main(string[] args)
    {
        var paths = new List<string> { "C:\\Users\\fromt\\RiderProjects\\TestsGenerator\\TestDir\\FirstClass.cs" };
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

        var buffer = new BufferBlock<string>();

        var saveTestFile = new ActionBlock<string>(testCode =>
        {
            using var writer = File.AppendText(GenerateTestFilePath(destPath));
            writer.WriteAsync(testCode);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        loadCodeFromFile.LinkTo(generateTestCode, linkOptions);
        generateTestCode.LinkTo(saveTestFile, linkOptions);

        foreach (var p in paths)
        {
            loadCodeFromFile.Post(p);
            loadCodeFromFile.Complete();
        }

        saveTestFile.Completion.Wait();
    }

    private static string GenerateTestFilePath(string destPath)
    {
        var path = $"{destPath}\\UnitTest.cs";
        var counter = 1;
        while (File.Exists(path))
        {
            counter++;
            path = $"{destPath}\\UnitTest{counter}.cs";
        }

        return path;
    }
}