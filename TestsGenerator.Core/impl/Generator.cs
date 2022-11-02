﻿using System.Collections;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsGenerator.Core.impl;

public class Generator : IGenerator
{
    public async Task<string> GenerateAsync(string sourceCode)
    {
        var members = new ConcurrentQueue<MethodDeclarationSyntax>(new[] { CreateSetUpMethod() });
        var namespaces = new List<NamespaceDeclarationSyntax>(new[] { CreateNUnitFrameworkNamespace() });

        var parsedCode = CSharpSyntaxTree.ParseText(sourceCode);

        ClassDeclarationSyntax firstClass = null;

        var root = parsedCode.GetCompilationUnitRoot();

        foreach (var namespaceDeclaration in root.Members.Cast<NamespaceDeclarationSyntax>())
        {
            namespaces.Add(namespaceDeclaration);

            foreach (var classDeclaration in namespaceDeclaration.Members.Cast<ClassDeclarationSyntax>())
            {
                firstClass ??= classDeclaration;
                
                //TODO: Method overload support
                
                Parallel.ForEach(classDeclaration.Members.Cast<MethodDeclarationSyntax>(),
                    methodDeclaration => members.Enqueue(CreateTestMethod(methodDeclaration.Identifier.Text)));
            }
        }

        var parsedTestCode =
            CreateCompilationUnit(firstClass.Identifier.Text, namespaces, members);

        return parsedTestCode.GetText().ToString();
    }

    private static async Task<MethodDeclarationSyntax> CreateTestMethodAsync(MethodDeclarationSyntax m)
    {
        return await Task.Run(() => CreateTestMethod(m.Identifier.Text));
    }


    private static MethodDeclarationSyntax CreateTestMethod(string sourceName)
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