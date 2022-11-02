using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks.Dataflow;
using TestsGenerator.Core;
using TestsGenerator.Core.impl;

public class PipelineContext
{
    public string Path { get; set;  }
    public string DestPath { get; set; }
    public string Code { get; set; }
    public string GeneratedTestCode { get; set; }

    public PipelineContext(string path, string destPath)
    {
        Path = path;
        DestPath = destPath;
    }
}
public class Program
{
    public static void Main(string[] args)
    {
        var paths = new ArrayList();
        var destPath = "C://tests";
        
        var loadCodeFromFile = new TransformBlock<string, string>(path =>
        {
            using var reader = File.OpenText(path);
            return reader.ReadToEnd();
        });

        var generateTestCode = new TransformBlock<string, string>(srcCode =>
        {
            var gen = new Generator();
            return gen.GenerateAsync(srcCode);
        });

        var saveTestFile = new ActionBlock<string>(async testCode =>
        {
            await using var writer = File.OpenWrite($"{destPath}/UnitTest1.cs");
            await writer.WriteAsync(Encoding.ASCII.GetBytes(testCode));
        });            
        
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        loadCodeFromFile.LinkTo(generateTestCode, linkOptions);
        generateTestCode.LinkTo(saveTestFile, linkOptions);
        
        Parallel.ForEach(new List<string> {"1", "2", "3"}, p => loadCodeFromFile.Post(p));
    }
}