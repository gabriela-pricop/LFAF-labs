using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Ast
{
    public sealed record KeywordNode(string Name) : IAstNode
    {
        public string Format(string indent = "")
        {
            return $"{indent}Keyword[{Name}]\n";
        }
    }
}
