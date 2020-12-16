using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace SourceGenerator
{
    [Generator]
    public class EndPointGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
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

            foreach (var command in syntaxReceiver.Commands)
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

            var finalSource = FileTemplate.Replace("###Commands###", commandSource.ToString());
            var sourceText = SourceText.From(finalSource, Encoding.UTF8);
            context.AddSource("GenerateController.cs", sourceText);
        }

        private string FileTemplate = @"using MediatR;
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
        public class CommandController : ControllerBase
        {
            private readonly IMediator _mediator;

            public CommandController(IMediator mediator)
            {
                _mediator = mediator;
            }

            //The command action methods will be inserted here by the source generator        
###Commands###
        }
    }";

        private string LookupIRequestGenericType(TypeDeclarationSyntax command)
        {
            foreach (var entry in command.BaseList.Types)
            {
                if (entry is SimpleBaseTypeSyntax basetype)
                {
                    if (basetype.Type is GenericNameSyntax type)
                    {
                        if (type.Identifier.ValueText == "IRequest" && type.TypeArgumentList.Arguments.Count == 1)
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
