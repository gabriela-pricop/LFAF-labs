using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Lexing
{
    public enum TokenType
    {
        INCOME,
        EXPENSE,
        TAX,
        PROFIT,
        LOSS,
        SAVE,
        INVEST,
        BUDGET,
        IF,
        THEN,
        ELSE,

        IDENTIFIER,
        INTEGER,
        FLOAT,
        PERCENT,

        ASSIGN,
        PLUS,
        MINUS,
        MULTIPLY,
        DIVIDE,
        GREATER,
        LESS,
        GREATER_EQUAL,
        LESS_EQUAL,
        EQUAL_EQUAL,
        NOT_EQUAL,

        LPAREN,
        RPAREN,
        COMMA,
        SEMICOLON,

        EOF,
        UNKNOWN
    }
}
