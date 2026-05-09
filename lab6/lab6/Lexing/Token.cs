using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Lexing
{
    public sealed record Token(TokenType Type, string Lexeme, int Line, int Column)
    {
        public override string ToString()
        {
            return $"{Type,-15} -> {Lexeme,-15} [line {Line}, column {Column}]";
        }
    }
}
