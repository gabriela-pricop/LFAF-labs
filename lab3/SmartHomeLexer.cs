using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab3
{
    public class SmartHomeLexer
    {
        private readonly string _input;
        private int _pos;       
        private char _ch;        
        private int _line = 1;

        private static readonly Dictionary<string, TokenType> Keywords = new()
        {
        { "let",         TokenType.LET         },
        { "fn",          TokenType.FN          },
        { "if",          TokenType.IF          },
        { "else",        TokenType.ELSE        },
        { "return",      TokenType.RETURN      },
        { "true",        TokenType.BOOL        },
        { "false",       TokenType.BOOL        },
        { "device",      TokenType.DEVICE      },
        { "set",         TokenType.SET         },
        { "get",         TokenType.GET         },
        { "when",        TokenType.WHEN        },
        { "notify",      TokenType.NOTIFY      },
        { "alert",       TokenType.ALERT       },
        { "mode",        TokenType.MODE        },
        { "fridge",      TokenType.FRIDGE      },
        { "temperature", TokenType.TEMPERATURE },
        { "door",        TokenType.DOOR        },
        { "energy",      TokenType.ENERGY      },
        };

        public SmartHomeLexer(string input)
        {
            _input = input;
            _pos = 0;
            _ch = input.Length > 0 ? input[0] : '\0';
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            Token tok;
            do
            {
                tok = NextToken();
                tokens.Add(tok);
            }
            while (tok.Type != TokenType.EOF);
            return tokens;
        }

        public Token NextToken()
        {
            // Keep looping until we land on a real character
            while (true)
            {
                SkipWhitespace();
                if (_ch == '/' && PeekNext() == '/')
                {
                    while (_ch != '\n' && _ch != '\0')
                        Advance();
                    continue; // re-enter loop so SkipWhitespace counts the \n
                }
                break;
            }

            int line = _line;

            Token tok = _ch switch
            {
                '=' => PeekNext() == '=' ? AdvanceAndMake(TokenType.EQ, "==", line)
                                         : Make(TokenType.ASSIGN, "=", line),
                '!' => PeekNext() == '=' ? AdvanceAndMake(TokenType.NOT_EQ, "!=", line)
                                         : Make(TokenType.ILLEGAL, "!", line),
                '<' => Make(TokenType.LT, "<", line),
                '>' => Make(TokenType.GT, ">", line),
                '+' => Make(TokenType.PLUS, "+", line),
                '-' => PeekNext() == '>' ? AdvanceAndMake(TokenType.ARROW, "->", line)
                                         : Make(TokenType.MINUS, "-", line),
                '*' => Make(TokenType.ASTERISK, "*", line),
                '/' => Make(TokenType.SLASH, "/", line),
                '(' => Make(TokenType.LPAREN, "(", line),
                ')' => Make(TokenType.RPAREN, ")", line),
                '{' => Make(TokenType.LBRACE, "{", line),
                '}' => Make(TokenType.RBRACE, "}", line),
                ';' => Make(TokenType.SEMICOLON, ";", line),
                ',' => Make(TokenType.COMMA, ",", line),
                ':' => Make(TokenType.COLON, ":", line),
                '"' => ReadString(line),
                '\0' => new Token(TokenType.EOF, "EOF", line),
                _ => IsLetter(_ch) ? ReadIdentOrKeyword(line)
                     : IsDigit(_ch) ? ReadNumber(line)
                     : Make(TokenType.ILLEGAL, _ch.ToString(), line),
            };

            return tok;
        }

        private Token ReadIdentOrKeyword(int line)
        {
            string text = ReadWhile(c => IsLetter(c) || IsDigit(c));

            if (Keywords.TryGetValue(text, out TokenType kw))
                return new Token(kw, text, line);

            return new Token(TokenType.IDENT, text, line);
        }

        private Token ReadNumber(int line)
        {
            string whole = ReadWhile(IsDigit);

            if (_ch == '.' && IsDigit(PeekNext()))
            {
                Advance(); // consume '.'
                string frac = ReadWhile(IsDigit);
                return new Token(TokenType.FLOAT, whole + "." + frac, line);
            }

            return new Token(TokenType.INT, whole, line);
        }

        private Token ReadString(int line)
        {
            Advance(); // skip opening "
            var sb = new System.Text.StringBuilder();

            while (_ch != '"' && _ch != '\0')
            {
                sb.Append(_ch);
                Advance();
            }

            if (_ch == '"') Advance(); // skip closing "
            return new Token(TokenType.STRING, sb.ToString(), line);
        }

        private void SkipWhitespace()
        {
            while (_ch == ' ' || _ch == '\t' || _ch == '\r' || _ch == '\n')
            {
                if (_ch == '\n') _line++;
                Advance();
            }
        }


        // Consume current char, advance, and return a single-char token
        private Token Make(TokenType type, string literal, int line)
        {
            Advance();
            return new Token(type, literal, line);
        }

        // Consume one extra character (for two-char tokens like == -> !=)
        private Token AdvanceAndMake(TokenType type, string literal, int line)
        {
            Advance(); // skip first char 
            Advance(); // skip second char
            return new Token(type, literal, line);
        }

        private string ReadWhile(Func<char, bool> predicate)
        {
            var sb = new System.Text.StringBuilder();
            while (predicate(_ch))
            {
                sb.Append(_ch);
                Advance();
            }
            return sb.ToString();
        }

        private void Advance()
        {
            _pos++;
            _ch = _pos < _input.Length ? _input[_pos] : '\0';
        }

        private char PeekNext() =>
            (_pos + 1 < _input.Length) ? _input[_pos + 1] : '\0';

        private static bool IsLetter(char c) => char.IsLetter(c) || c == '_';
        private static bool IsDigit(char c) => char.IsDigit(c);
    }
}
