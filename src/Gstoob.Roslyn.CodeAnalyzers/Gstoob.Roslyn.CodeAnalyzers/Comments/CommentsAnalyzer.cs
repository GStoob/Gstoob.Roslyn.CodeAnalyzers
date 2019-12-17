using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gstoob.Roslyn.CodeAnalyzers.Comments
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CommentsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RemoveNotAllowedCommentsAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        private static HashSet<string> DisallowedComments = new HashSet<string>
        {
            "arrange",
            "act",
            "assert",
            "given",
            "when",
            "then"
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(HandleSyntaxTree);
        }

        private static void HandleSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);
            var commentNodes = root.DescendantTrivia().Where(n => n.IsKind(SyntaxKind.SingleLineCommentTrivia) || n.IsKind(SyntaxKind.MultiLineCommentTrivia));

            if (!commentNodes.Any()) return;

            foreach (var node in commentNodes)
            {
                var commentText = node.Kind() switch
                {
                SyntaxKind.SingleLineCommentTrivia => node.ToString().TrimStart('/').ToLowerInvariant(),
                SyntaxKind.MultiLineCommentTrivia => RemoveMultilineCommentCharacters(node.ToString().ToLowerInvariant()),
                _ => throw new ArgumentOutOfRangeException("Could not determine the comment type")
                };

                if (DisallowedComments.Contains(commentText))
                {
                    var diagnostic = Diagnostic.Create(Rule, node.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static string RemoveMultilineCommentCharacters(string commentString)
        {
            var length = commentString.Length;
            var commentText = commentString.Substring(2, length - 4);
            return commentText;
        }
    }
}
