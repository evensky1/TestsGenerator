using System.Collections;
using System.Text;
using System.Threading.Tasks.Dataflow;
using TestsGenerator.Core.impl;

namespace TestsGenerator;

public static class Program
{
    public static void Main(string[] args)
    {
        var paths = new List<string> {"C:\\Users\\fromt\\RiderProjects\\TestsGenerator\\TestDir\\FirstClass.cs"};
        var destPath = "C:\\Users\\fromt\\RiderProjects\\TestsGenerator\\TestDir\\Tests";
        
        var loadCodeFromFile = new TransformBlock<string, string>(async path =>
        {
            using var reader = File.OpenText(path);
            return await reader.ReadToEndAsync();
        });

        var generateTestCode = new TransformBlock<string, string>(srcCode =>
        {
            var gen = new Generator();
            return gen.GenerateAsync(srcCode);
        });
        
        var saveTestFile = new ActionBlock<string>(async testCode =>
        {
            await using var writer = File.AppendText($"{destPath}\\UnitTest1.cs");
            await writer.WriteAsync(testCode);
        });            
        
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        loadCodeFromFile.LinkTo(generateTestCode, linkOptions);
        generateTestCode.LinkTo(saveTestFile, linkOptions);

        var options = new ParallelOptions()
        {
            MaxDegreeOfParallelism = -1
        };
        
        Parallel.ForEach(paths, options, p =>
        {
            loadCodeFromFile.Post(p);
            loadCodeFromFile.Complete();
            generateTestCode.Completion.Wait();
        });
    }
}