using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Ast
{
    public sealed record AssignmentNode(string Name, IAstNode Value) : IAstNode
    {
        public string Format(string indent = "")
        {
            return $"{indent}Assignment[{Name}]\n{Value.Format(indent + "  ")}";
        }
    }

}
