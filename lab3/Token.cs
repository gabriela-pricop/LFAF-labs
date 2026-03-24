using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab3
{
    public enum TokenType
    {
        // Literals
        INT,
        FLOAT,
        STRING,
        BOOL,       

        // Identifiers
        IDENT,

        // Keywords
        LET,
        FN,
        IF,
        ELSE,
        RETURN,
        DEVICE,
        SET,
        GET,
        WHEN,
        NOTIFY,
        ALERT,
        MODE,
        FRIDGE,
        TEMPERATURE,
        DOOR,
        ENERGY,

        // Operators
        ASSIGN,     
        EQ,         
        NOT_EQ,     
        LT,         
        GT,         
        PLUS,       
        MINUS,      
        SLASH,      
        ASTERISK,   
        ARROW,      

        // Delimiters
        LPAREN,     
        RPAREN,     
        LBRACE,     
        RBRACE,     
        SEMICOLON,  
        COMMA,      
        COLON,      

        // Special
        ILLEGAL,
        EOF,
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Literal { get; }
        public int Line { get; }

        public Token(TokenType type, string literal, int line)
        {
            Type = type;
            Literal = literal;
            Line = line;
        }

        public override string ToString() =>
            $"[Line {Line}] {Type,-14} | {Literal}";
    }
}
