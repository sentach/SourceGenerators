using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
#if DEBUG//SOURCEGENERATOR
using System.Diagnostics;
#endif
using System.Text;

namespace SourceGenerator
{
    [Generator]
    public class EndPointGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG//SOURCEGENERATOR
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif 
            // Register a factory that can create our custom syntax receiver
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            MySyntaxReceiver syntaxReceiver = (MySyntaxReceiver)context.SyntaxReceiver;
            GenerateCommandClass(context, syntaxReceiver);
        }

        private void GenerateCommandClass(GeneratorExecutionContext context, MySyntaxReceiver syntaxReceiver)
        {
            var commandSource = new StringBuilder();
            var controllers = new List<string>();
            GetControllers(syntaxReceiver.Gets, controllers);
            GetControllers(syntaxReceiver.Post, controllers);

            foreach (var controller in controllers)
            {
                foreach (var command in syntaxReceiver.Gets[controller])
                {
                    var commandName = command.Identifier.ValueText;
                    var commandReturnType = LookupIRequestGenericType(command);
                    var commandComments = command.GetLeadingTrivia().ToString();

                    commandSource.AppendLine($@"                
                {commandComments}
                [HttpGet]
                [Produces(""application/json"")]
                [ProducesResponseType(typeof({commandReturnType}), StatusCodes.Status200OK)]
                [ProducesResponseType(StatusCodes.Status400BadRequest)]
                public async Task<{commandReturnType}> {commandName}()
                {{ 
                    var command = new {commandName}();
                    return await _mediator.Send(command);
                }}
                ");
                }
                foreach (var command in syntaxReceiver.Post[controller])
                {
                    var commandName = command.Identifier.ValueText;
                    var commandReturnType = LookupIRequestGenericType(command);
                    var commandComments = command.GetLeadingTrivia().ToString();

                    commandSource.AppendLine($@"                
                {commandComments}
                [HttpPost]
                [Produces(""application/json"")]
                [ProducesResponseType(typeof({commandReturnType}), StatusCodes.Status201Created)]
                [ProducesResponseType(StatusCodes.Status400BadRequest)]
                public async Task<{commandReturnType}> {commandName}([FromBody] {commandName} command)
                {{ 

                    return await _mediator.Send(command);
                }}
                ");
                }


                var finalSource = FileTemplate.Replace("###Commands###", commandSource.ToString()).Replace("###name###", controller);
                var sourceText = SourceText.From(finalSource, Encoding.UTF8);
                context.AddSource("GenerateController.cs", sourceText);
            }
        }

        private void GetControllers(Dictionary<string, List<TypeDeclarationSyntax>> dic, List<string> controllers)
        {
            foreach(var key in dic.Keys)
            {
                if(!controllers.Contains(key))
                {
                    controllers.Add(key);
                }
            }
        }

        private readonly string FileTemplate = @"using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CharlaSG.BussinesLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGenerated
{
    /// <summary>
    /// This is the controller class for all the commands in the system
    /// </summary>
    [Route(""api/[controller]/[Action]"")]
    [ApiController]
    public class ###name###Controller : ControllerBase
    {
        private readonly IMediator _mediator;

        public ###name###Controller(IMediator mediator)
        {
            _mediator = mediator;
        }
            
###Commands###
    }
}";

        List<string> tipos = new List<string> { "IGet", "IPost" };
        private string LookupIRequestGenericType(TypeDeclarationSyntax command)
        {
            
            foreach (var entry in command.BaseList.Types)
            {
                if (entry is SimpleBaseTypeSyntax basetype)
                {
                    if (basetype.Type is GenericNameSyntax type)
                    {
                        if (tipos.Contains(type.Identifier.ValueText)  && type.TypeArgumentList.Arguments.Count == 1)
                        {
                            return type.TypeArgumentList.Arguments[0].ToString();
                        }
                    }
                }
            }
            return "";
        }
    }
}
