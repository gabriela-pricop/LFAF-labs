using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Ast
{
    public sealed record NumberNode(string Value, bool IsPercent) : IAstNode
    {
        public string Format(string indent = "")
        {
            string suffix = IsPercent ? "%" : string.Empty;
            return $"{indent}Number[{Value}{suffix}]\n";
        }
    }
}
