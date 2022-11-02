﻿using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator.Core.impl;

public class Generator : IGenerator
{
    private readonly ConcurrentQueue<MethodDeclarationSyntax> _members = new (new[] { CreateSetUpMethod() });
    private readonly List<NamespaceDeclarationSyntax> _namespaces = new (new[] { CreateNUnitFrameworkNamespace() });
    private int _counter = 1;
    public async Task<string> GenerateAsync(string sourceCode)
    {
        ClassDeclarationSyntax firstClass = null;
        
        var parsedCode = CSharpSyntaxTree.ParseText(sourceCode);
        var root = parsedCode.GetCompilationUnitRoot();

        foreach (var namespaceDeclaration in root.Members.Cast<NamespaceDeclarationSyntax>())
        {
            _namespaces.Add(namespaceDeclaration);

            foreach (var classDeclaration in namespaceDeclaration.Members.Cast<ClassDeclarationSyntax>())
            {
                firstClass ??= classDeclaration;
                
                //TODO: Method overload support
                
                var methods = classDeclaration.Members.Cast<MethodDeclarationSyntax>();
                
                var parallelOptions = new ParallelOptions() 
                {
                    MaxDegreeOfParallelism = -1 //unlimited
                };

                Parallel.ForEach(methods, parallelOptions, (method, _) => 
                    _members.Enqueue(CreateTestMethod(method.Identifier.Text)));
            }
        }
        
        return await Task.Run(() => 
            CreateCompilationUnit(firstClass.Identifier.Text, _namespaces, _members).GetText().ToString());
    }

    private async Task<MethodDeclarationSyntax> CreateTestMethodAsync(MethodDeclarationSyntax m)
    {
        return await Task.Run(() => CreateTestMethod(m.Identifier.Text));
    }
    private MethodDeclarationSyntax CreateTestMethod(string sourceName)
    {
        return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier($"{sourceName}Test"))
            .WithAttributeLists(
                SyntaxFactory.SingletonList(
                    SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Attribute(
                                SyntaxFactory.IdentifierName("Test"))))))
            .WithModifiers(
                SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithBody(
                SyntaxFactory.Block(
                    SyntaxFactory.SingletonList<StatementSyntax>(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("Assert"),
                                        SyntaxFactory.IdentifierName("Fail")))
                                .WithArgumentList(
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    SyntaxFactory.Literal("autogenerated"))))))))));
    }

    private static MethodDeclarationSyntax CreateSetUpMethod()
    {
        return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier("Setup"))
            .WithAttributeLists(
                SyntaxFactory.SingletonList(
                    SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Attribute(
                                SyntaxFactory.IdentifierName("SetUp"))))))
            .WithModifiers(
                SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithBody(SyntaxFactory.Block());
    }

    private static NamespaceDeclarationSyntax CreateNUnitFrameworkNamespace()
    {
        return SyntaxFactory.NamespaceDeclaration(
            SyntaxFactory.QualifiedName(
                SyntaxFactory.IdentifierName("NUnit"),
                SyntaxFactory.IdentifierName("Framework")));
    }

    private static IEnumerable<UsingDirectiveSyntax> CreateUsings(IEnumerable<NamespaceDeclarationSyntax> namespaces)
    {
        return namespaces.Select(n => SyntaxFactory.UsingDirective(n.Name));
    }

    private static CompilationUnitSyntax CreateCompilationUnit(
        string sourceClassName,
        IEnumerable<NamespaceDeclarationSyntax> namespaces,
        IEnumerable<MemberDeclarationSyntax> generatedMembers)
    {
        return SyntaxFactory.CompilationUnit()
            .WithUsings(
                SyntaxFactory.List(CreateUsings(namespaces)))
            .WithMembers(
                SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                    SyntaxFactory.NamespaceDeclaration(
                            SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(sourceClassName),
                                SyntaxFactory.IdentifierName("Tests")))
                        .WithMembers(
                            SyntaxFactory.SingletonList<MemberDeclarationSyntax>(SyntaxFactory
                                .ClassDeclaration($"{sourceClassName}Tests")
                                .WithModifiers(
                                    SyntaxFactory.TokenList(
                                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                .WithMembers(
                                    SyntaxFactory.List(generatedMembers))))))
            .NormalizeWhitespace();
    }
}