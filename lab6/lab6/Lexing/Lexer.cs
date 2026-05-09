using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace lab6.Lexing
{
    public sealed class Lexer
    {
        private static readonly Regex WhitespacePattern = new(@"\G[ \t\r\n]+", RegexOptions.Compiled);

        private static readonly IReadOnlyList<TokenRule> TokenRules =
        [
        Keyword(TokenType.INCOME, "income"),
        Keyword(TokenType.EXPENSE, "expense"),
        Keyword(TokenType.TAX, "tax"),
        Keyword(TokenType.PROFIT, "profit"),
        Keyword(TokenType.LOSS, "loss"),
        Keyword(TokenType.SAVE, "save"),
        Keyword(TokenType.INVEST, "invest"),
        Keyword(TokenType.BUDGET, "budget"),
        Keyword(TokenType.IF, "if"),
        Keyword(TokenType.THEN, "then"),
        Keyword(TokenType.ELSE, "else"),

        new(TokenType.FLOAT, new Regex(@"\G\d+\.\d+", RegexOptions.Compiled)),
        new(TokenType.UNKNOWN, new Regex(@"\G\d+\.", RegexOptions.Compiled)),
        new(TokenType.INTEGER, new Regex(@"\G\d+", RegexOptions.Compiled)),

        new(TokenType.GREATER_EQUAL, new Regex(@"\G>=", RegexOptions.Compiled)),
        new(TokenType.LESS_EQUAL, new Regex(@"\G<=", RegexOptions.Compiled)),
        new(TokenType.EQUAL_EQUAL, new Regex(@"\G==", RegexOptions.Compiled)),
        new(TokenType.NOT_EQUAL, new Regex(@"\G!=", RegexOptions.Compiled)),
        new(TokenType.ASSIGN, new Regex(@"\G=", RegexOptions.Compiled)),
        new(TokenType.PLUS, new Regex(@"\G\+", RegexOptions.Compiled)),
        new(TokenType.MINUS, new Regex(@"\G-", RegexOptions.Compiled)),
        new(TokenType.MULTIPLY, new Regex(@"\G\*", RegexOptions.Compiled)),
        new(TokenType.DIVIDE, new Regex(@"\G/", RegexOptions.Compiled)),
        new(TokenType.GREATER, new Regex(@"\G>", RegexOptions.Compiled)),
        new(TokenType.LESS, new Regex(@"\G<", RegexOptions.Compiled)),
        new(TokenType.PERCENT, new Regex(@"\G%", RegexOptions.Compiled)),
        new(TokenType.LPAREN, new Regex(@"\G\(", RegexOptions.Compiled)),
        new(TokenType.RPAREN, new Regex(@"\G\)", RegexOptions.Compiled)),
        new(TokenType.COMMA, new Regex(@"\G,", RegexOptions.Compiled)),
        new(TokenType.SEMICOLON, new Regex(@"\G;", RegexOptions.Compiled)),
        new(TokenType.IDENTIFIER, new Regex(@"\G[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled))
        ];

        private readonly string input;
        private readonly List<Token> tokens = [];
        private int position;
        private int line = 1;
        private int column = 1;

        public Lexer(string input)
        {
            this.input = input;
        }

        public IReadOnlyList<Token> Tokenize()
        {
            tokens.Clear();
            position = 0;
            line = 1;
            column = 1;

            while (position < input.Length)
            {
                if (TryConsumeWhitespace())
                {
                    continue;
                }

                Token? token = TryConsumeToken();
                if (token is not null)
                {
                    tokens.Add(token);
                    continue;
                }

                tokens.Add(new Token(TokenType.UNKNOWN, input[position].ToString(), line, column));
                Advance(input[position].ToString());
            }

            tokens.Add(new Token(TokenType.EOF, "EOF", line, column));
            return tokens;
        }

        private static TokenRule Keyword(TokenType type, string word)
        {
            return new TokenRule(type, new Regex($@"\G{word}(?![A-Za-z0-9_])", RegexOptions.Compiled));
        }

        private bool TryConsumeWhitespace()
        {
            Match match = WhitespacePattern.Match(input, position);
            if (!match.Success)
            {
                return false;
            }

            Advance(match.Value);
            return true;
        }

        private Token? TryConsumeToken()
        {
            foreach (TokenRule rule in TokenRules)
            {
                Match match = rule.Pattern.Match(input, position);
                if (!match.Success)
                {
                    continue;
                }

                Token token = new(rule.Type, match.Value, line, column);
                Advance(match.Value);
                return token;
            }

            return null;
        }

        private void Advance(string text)
        {
            foreach (char ch in text)
            {
                position++;
                if (ch == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
            }
        }

        private sealed record TokenRule(TokenType Type, Regex Pattern);
    }
}
