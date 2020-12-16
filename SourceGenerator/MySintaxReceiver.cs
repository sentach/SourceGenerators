using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace SourceGenerator
{
    class MySyntaxReceiver : ISyntaxReceiver
    {
        public List<TypeDeclarationSyntax> Commands { get; private set; } = new List<TypeDeclarationSyntax>();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if(syntaxNode is ClassDeclarationSyntax || syntaxNode is RecordDeclarationSyntax)
            {
                var tds = (TypeDeclarationSyntax)syntaxNode;
                if(tds.BaseList != null)
                {
                    var baselist = tds.BaseList;
                    foreach (var entry in baselist.Types)
                    {
                        if(entry is SimpleBaseTypeSyntax basetype)
                        {
                            if(basetype.Type is GenericNameSyntax type)
                            {
                                if (type.Identifier.ValueText=="IRequest" && type.TypeArgumentList.Arguments.Count==1 && (type.TypeArgumentList.Arguments[0] is TypeSyntax))
                                {
                                    Commands.Add(tds);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
