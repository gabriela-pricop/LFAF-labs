using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Ast
{
    public sealed record ProgramNode(IReadOnlyList<IAstNode> Statements) : IAstNode
    {
        public string Format(string indent = "")
        {
            StringBuilder builder = new();
            builder.AppendLine($"{indent}Program");

            foreach (IAstNode statement in Statements)
            {
                builder.Append(statement.Format(indent + "  "));
            }

            return builder.ToString();
        }
    }
}
