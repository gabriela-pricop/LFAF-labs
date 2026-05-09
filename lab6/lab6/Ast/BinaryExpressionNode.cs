using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Ast
{
    public sealed record BinaryExpressionNode(string Operator, IAstNode Left, IAstNode Right) : IAstNode
    {
        public string Format(string indent = "")
        {
            return $"{indent}BinaryOp[{Operator}]\n{Left.Format(indent + "  ")}{Right.Format(indent + "  ")}";
        }
    }
}
