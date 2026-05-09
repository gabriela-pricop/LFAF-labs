using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Ast
{
    public sealed record IfNode(IAstNode Condition, IAstNode ThenBranch, IAstNode? ElseBranch) : IAstNode
    {
        public string Format(string indent = "")
        {
            StringBuilder builder = new();
            builder.AppendLine($"{indent}If");
            builder.AppendLine($"{indent}  [condition]");
            builder.Append(Condition.Format(indent + "    "));
            builder.AppendLine($"{indent}  [then]");
            builder.Append(ThenBranch.Format(indent + "    "));

            if (ElseBranch is not null)
            {
                builder.AppendLine($"{indent}  [else]");
                builder.Append(ElseBranch.Format(indent + "    "));
            }

            return builder.ToString();
        }
    }
}
