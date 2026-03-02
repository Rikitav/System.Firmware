using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace System.Firmware.BootService.Generators;

[Generator(LanguageNames.CSharp)]
public class DevicePathProtocolTypeListGenerator : IIncrementalGenerator
{
    private class DevicePathProtocolInfo(ClassDeclarationSyntax classDeclaration, ITypeSymbol typeSymbol)
    {
        public ClassDeclarationSyntax ClassDeclaration => classDeclaration;
        public ITypeSymbol TypeSymbol => typeSymbol;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<DevicePathProtocolInfo>> pipeline = context.SyntaxProvider
            .CreateSyntaxProvider(SyntaxPredicate, SyntaxTransform)
            .Where(declaration => declaration != null)
            .Collect();

        context.RegisterImplementationSourceOutput(pipeline, GenerateSource);
    }
    
    private static bool SyntaxPredicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return node is ClassDeclarationSyntax;
    }
    
    private static DevicePathProtocolInfo SyntaxTransform(GeneratorSyntaxContext context, CancellationToken _)
    {
        ISymbol? symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
        if (symbol is null)
            return null!;

        if (symbol is not ITypeSymbol typeSymbol)
            return null!;

        if (!IsAssignableFrom(typeSymbol, "DevicePathProtocolBase"))
            return null!;

        return new DevicePathProtocolInfo((ClassDeclarationSyntax)context.Node, typeSymbol);
    }

    private static void GenerateSource(SourceProductionContext context, ImmutableArray<DevicePathProtocolInfo> infos)
    {
        IEnumerable<FieldDeclarationSyntax> fields = infos.Select(info => SyntaxFactory.FieldDeclaration(
            SyntaxFactory.List<AttributeListSyntax>(),
            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
            SyntaxFactory.VariableDeclaration(
                SyntaxFactory.ParseTypeName("System.Type"),
                SyntaxFactory.SeparatedList([
                    SyntaxFactory.VariableDeclarator(
                        SyntaxFactory.Identifier(info.ClassDeclaration.Identifier.Text.Replace("DevicePath", string.Empty)),
                        null,
                        SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(info.TypeSymbol.ToDisplayString()))
                    )
                )
            ]))
        ));

        NamespaceDeclarationSyntax namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("System.Firmware.BootService"))
            .AddMembers(SyntaxFactory.ClassDeclaration("DevicePathProtocolTypeList")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(fields.ToArray())
        );

        CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit()
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        context.AddSource("DevicePathProtocolTypeList.g.cs", compilationUnit.ToFullString());
    }

    public static bool IsAssignableFrom(ITypeSymbol symbol, string className)
    {
        if (symbol.BaseType == null)
            return false;

        if (symbol.BaseType.Name == className)
            return true;

        return IsAssignableFrom(symbol.BaseType, className);
    }
}
