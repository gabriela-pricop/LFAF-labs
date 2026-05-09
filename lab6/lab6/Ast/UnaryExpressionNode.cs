using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Ast
{
    public sealed record UnaryExpressionNode(string Operator, IAstNode Operand) : IAstNode
    {
        public string Format(string indent = "")
        {
            return $"{indent}UnaryOp[{Operator}]\n{Operand.Format(indent + "  ")}";
        }
    }

}
