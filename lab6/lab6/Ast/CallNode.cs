using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Ast
{
    public sealed record CallNode(string Name, IReadOnlyList<IAstNode> Arguments) : IAstNode
    {
        public string Format(string indent = "")
        {
            StringBuilder builder = new();
            builder.AppendLine($"{indent}Call[{Name}]");

            foreach (IAstNode argument in Arguments)
            {
                builder.Append(argument.Format(indent + "  "));
            }

            return builder.ToString();
        }
    }

}
