using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Gstoob.Roslyn.CodeAnalyzers.Comments
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CommentsCodeFixProvider)), Shared]
    public class CommentsCodeFixProvider : CodeFixProvider
    {
        private const string title = "Get rid of the damn region";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CommentsAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span, true, true);
            var region = node as RegionDirectiveTriviaSyntax;

            if (region != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: c =>
                        {
                            var newRoot = root.RemoveNodes(region.GetRelatedDirectives(), SyntaxRemoveOptions.AddElasticMarker);
                            var newDocument = context.Document.WithSyntaxRoot(newRoot);
                            return Task.FromResult(newDocument);
                        }),
context.Diagnostics.First());
            }
        }
    }
}
