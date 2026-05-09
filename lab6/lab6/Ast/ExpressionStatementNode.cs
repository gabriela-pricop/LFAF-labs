using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Ast
{
    public sealed record ExpressionStatementNode(IAstNode Expression) : IAstNode
    {
        public string Format(string indent = "")
        {
            return $"{indent}ExpressionStatement\n{Expression.Format(indent + "  ")}";
        }
    }
}
