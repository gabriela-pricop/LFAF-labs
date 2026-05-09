using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Ast
{
    public interface IAstNode
    {
        string Format(string indent = "");
    }
}
