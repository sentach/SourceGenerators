using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace SourceGenerator
{
    class MySyntaxReceiver : ISyntaxReceiver
    {
        public Dictionary<string, List<TypeDeclarationSyntax>> Gets { get; private set; } = new Dictionary<string, List<TypeDeclarationSyntax>>();
        public Dictionary<string, List<TypeDeclarationSyntax>> Post { get; private set; } = new Dictionary<string, List<TypeDeclarationSyntax>>();
        
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
                                if (type.TypeArgumentList.Arguments.Count == 1 && (type.TypeArgumentList.Arguments[0] is TypeSyntax))
                                {
                                    if (type.Identifier.ValueText == "IPost")
                                    {
                                        var name = GetController(tds);
                                        if(!Post.ContainsKey(name))
                                        {
                                            Post.Add(name, new List<TypeDeclarationSyntax>());
                                        }                                        
                                        Post[name].Add(tds);
                                    }
                                    if(type.Identifier.ValueText=="IGet")
                                    {
                                        var name = GetController(tds);
                                        if (!Gets.ContainsKey(name))
                                        {
                                            Gets.Add(name, new List<TypeDeclarationSyntax>());
                                        }
                                        Gets[name].Add(tds);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private string GetController(TypeDeclarationSyntax tds)
        {
            var result = "Default";
            foreach(var att in tds.AttributeLists)
            {
                foreach(var a in att.Attributes)
                {
                    if(a.Name.ToString()=="ControllerName")
                    {
                        if(a.ArgumentList.Arguments.Count>0)
                        {
                            result = a.ArgumentList.Arguments[0].ToString();
                            result = result.Replace("\"", "");
                        }
                    }
                }
            }
            return result;
        }
    }
}
