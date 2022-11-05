using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator.Core.impl;

public class Generator : IGenerator
{
    private readonly ConcurrentQueue<MethodDeclarationSyntax> _members;
    private readonly List<BaseNamespaceDeclarationSyntax> _namespaces;
    private readonly Semaphore _semaphore;

    public Generator(int maxTaskCount)
    {
        _members = new ConcurrentQueue<MethodDeclarationSyntax>(new[] { ElementFactory.CreateSetUpMethod() });
        _namespaces = new List<BaseNamespaceDeclarationSyntax>(new[] { ElementFactory.CreateNUnitFrameworkNamespace() });
        _semaphore = new Semaphore(maxTaskCount, maxTaskCount);
    }

    public async Task<string> GenerateAsync(string sourceCode)
    {
        ClassDeclarationSyntax firstClass = null;
        
        var parsedCode = CSharpSyntaxTree.ParseText(sourceCode);
        var root = parsedCode.GetCompilationUnitRoot();

        foreach (var rootMember in root.Members)
        {
            if (rootMember.GetType().BaseType != typeof(BaseNamespaceDeclarationSyntax)) continue;

            var namespaceDeclaration = (BaseNamespaceDeclarationSyntax) rootMember;
            _namespaces.Add(namespaceDeclaration);
            
            foreach (var cmMember in namespaceDeclaration.Members)
            {
                if (cmMember is not ClassDeclarationSyntax classDeclaration) continue;
                
                firstClass ??= classDeclaration;
                ProcessClass(classDeclaration);
            }
        }

        if (firstClass == null) return "";
        
        return await Task.Run(() => 
            ElementFactory.CreateCompilationUnit(firstClass.Identifier.Text, _namespaces, _members).GetText().ToString());
    }

    private void ProcessClass(ClassDeclarationSyntax classDeclaration)
    {
        var methods = new List<string>();
            
        foreach (var member in classDeclaration.Members)
        {
            switch (member)
            {
                case MethodDeclarationSyntax method:
                    var counter = 1;
                    var methodName = $"{classDeclaration.Identifier.Text}_{method.Identifier.Text}";
                    
                    while (methods.Find(m => m.Equals(methodName)) != null)
                        methodName = $"{classDeclaration.Identifier.Text}_{method.Identifier.Text}{++counter}";
                    
                    methods.Add(methodName);

                    Task.Run(() =>
                    {
                        _semaphore.WaitOne();
                        _members.Enqueue(ElementFactory.CreateTestMethod(methodName));
                        _semaphore.Release();
                    });
                    break;
                case ClassDeclarationSyntax clazz:
                    ProcessClass(clazz);
                    break;
            }
        }
    }
}