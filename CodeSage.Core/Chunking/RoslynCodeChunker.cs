using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace CodeSage.Core.Chunking;

public sealed class RoslynCodeChunker
{
    public IReadOnlyList<string> ChunkFile(string filePath, string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetCompilationUnitRoot();

        var chunks = new List<string>();

        // walk all method, constructor, and property declarations
        var members = root.DescendantNodes().OfType<MemberDeclarationSyntax>()
            .Where(m => m is MethodDeclarationSyntax
                     or ConstructorDeclarationSyntax
                     or PropertyDeclarationSyntax
                     or FieldDeclarationSyntax);

        foreach (var member in members)
        {
            // find the containing type name
            var containingType = member.Ancestors()
                .OfType<TypeDeclarationSyntax>()
                .FirstOrDefault();

            var className = containingType?.Identifier.Text ?? "unknown";
            var memberName = GetMemberName(member);
            var relativeFile = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);

            var sb = new StringBuilder();
            sb.AppendLine($"// File: {relativeFile}");
            sb.AppendLine($"// Class: {className}");
            sb.AppendLine($"// Member: {memberName}");
            sb.AppendLine();
            sb.Append(member.ToFullString().Trim());

            chunks.Add(sb.ToString());
        }

        // if nothing was found (e.g. a file with only top-level statements)
        // fall back to the whole file as one chunk
        if (chunks.Count == 0)
            chunks.Add(source);

        return chunks.AsReadOnly();
    }

    private static string GetMemberName(MemberDeclarationSyntax member) => member switch
    {
        MethodDeclarationSyntax m      => m.Identifier.Text,
        ConstructorDeclarationSyntax c => c.Identifier.Text,
        PropertyDeclarationSyntax p    => p.Identifier.Text,
        FieldDeclarationSyntax f       => f.Declaration.Variables.First().Identifier.Text,
        _                              => "unknown"
    };
}