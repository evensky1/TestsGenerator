using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator.Core.impl;

public class Generator : IGenerator
{
    private readonly Semaphore _semaphore;

    public Generator(int maxTaskCount)
    {
        _semaphore = new Semaphore(maxTaskCount, maxTaskCount);
    }

    public string Generate(string sourceCode)
    {
        ClassDeclarationSyntax firstClass = null;
        
        var parsedCode = CSharpSyntaxTree.ParseText(sourceCode);
        var root = parsedCode.GetCompilationUnitRoot();
        var members = new List<MethodDeclarationSyntax>(new[] { ElementFactory.CreateSetUpMethod() });
        var namespaces = new List<BaseNamespaceDeclarationSyntax>(new[] { ElementFactory.CreateNUnitFrameworkNamespace() });
        
        foreach (var rootMember in root.Members)
        {
            if (rootMember.GetType().BaseType != typeof(BaseNamespaceDeclarationSyntax)) continue;

            var namespaceDeclaration = (BaseNamespaceDeclarationSyntax) rootMember;
            namespaces.Add(namespaceDeclaration);
            
            foreach (var cmMember in namespaceDeclaration.Members)
            {
                if (cmMember is not ClassDeclarationSyntax classDeclaration) continue;
                
                firstClass ??= classDeclaration;

                var methods = ProcessClass(classDeclaration);

                members.AddRange(methods);
            }
        }

        if (firstClass == null) return "";
        
        return ElementFactory.CreateCompilationUnit(firstClass.Identifier.Text, namespaces, members).GetText().ToString();
    }

    private List<MethodDeclarationSyntax> ProcessClass(ClassDeclarationSyntax classDeclaration)
    {
        var methods = new List<MethodDeclarationSyntax>();
            
        foreach (var member in classDeclaration.Members)
        {
            switch (member)
            {
                case MethodDeclarationSyntax method:
                    var counter = 1;
                    var methodName = $"{classDeclaration.Identifier.Text}_{method.Identifier.Text}";
                    
                    while (methods.Find(m => m.Identifier.Text.Equals($"{methodName}Test")) != null)
                        methodName = $"{classDeclaration.Identifier.Text}_{method.Identifier.Text}{++counter}";
                    
                    methods.Add(ElementFactory.CreateTestMethod(methodName));
                    break;
                case ClassDeclarationSyntax clazz:
                    methods.AddRange(ProcessClass(clazz));
                    break;
            }
        }
        
        return methods;
    }
}