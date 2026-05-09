using lab6.Ast;
using lab6.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace lab6.Parsing
{
    public sealed class Parser
    {
        private static readonly HashSet<TokenType> AssignmentTargets =
        [
        TokenType.INCOME,
        TokenType.EXPENSE,
        TokenType.TAX,
        TokenType.PROFIT,
        TokenType.LOSS,
        TokenType.SAVE,
        TokenType.INVEST,
        TokenType.BUDGET,
        TokenType.IDENTIFIER
        ];

        private static readonly HashSet<TokenType> ValueKeywords =
        [
        TokenType.INCOME,
        TokenType.EXPENSE,
        TokenType.TAX,
        TokenType.PROFIT,
        TokenType.LOSS,
        TokenType.SAVE,
        TokenType.INVEST,
        TokenType.BUDGET
        ];

        private static readonly HashSet<TokenType> CallableKeywords =
        [
        TokenType.SAVE,
        TokenType.INVEST,
        TokenType.BUDGET
        ];

        private readonly IReadOnlyList<Token> tokens;
        private int current;

        public Parser(IReadOnlyList<Token> tokens)
        {
            this.tokens = tokens;
        }

        public ProgramNode Parse()
        {
            List<IAstNode> statements = [];

            while (!Check(TokenType.EOF))
            {
                statements.Add(ParseStatement());
            }

            return new ProgramNode(statements);
        }

        private IAstNode ParseStatement()
        {
            if (Match(TokenType.IF))
            {
                return ParseIfStatement();
            }

            if (AssignmentTargets.Contains(Peek().Type) && PeekAhead(1).Type == TokenType.ASSIGN)
            {
                return ParseAssignmentStatement();
            }

            IAstNode expression = ParseExpression();
            Consume(TokenType.SEMICOLON, "Expected ';' after expression.");
            return new ExpressionStatementNode(expression);
        }

        private IAstNode ParseIfStatement()
        {
            IAstNode condition = ParseExpression();
            Consume(TokenType.THEN, "Expected 'then' after if condition.");

            IAstNode thenBranch = ParseExpression();
            IAstNode? elseBranch = null;

            if (Match(TokenType.ELSE))
            {
                elseBranch = ParseExpression();
            }

            Consume(TokenType.SEMICOLON, "Expected ';' after if statement.");
            return new IfNode(condition, thenBranch, elseBranch);
        }

        private IAstNode ParseAssignmentStatement()
        {
            Token name = Advance();
            Consume(TokenType.ASSIGN, "Expected '=' after assignment target.");
            IAstNode value = ParseExpression();
            Consume(TokenType.SEMICOLON, "Expected ';' after assignment.");
            return new AssignmentNode(name.Lexeme, value);
        }

        private IAstNode ParseExpression()
        {
            return ParseComparison();
        }

        private IAstNode ParseComparison()
        {
            IAstNode left = ParseAddition();

            while (Match(
                TokenType.GREATER,
                TokenType.LESS,
                TokenType.GREATER_EQUAL,
                TokenType.LESS_EQUAL,
                TokenType.EQUAL_EQUAL,
                TokenType.NOT_EQUAL))
            {
                Token op = Previous();
                IAstNode right = ParseAddition();
                left = new BinaryExpressionNode(op.Lexeme, left, right);
            }

            return left;
        }

        private IAstNode ParseAddition()
        {
            IAstNode left = ParseMultiplication();

            while (Match(TokenType.PLUS, TokenType.MINUS))
            {
                Token op = Previous();
                IAstNode right = ParseMultiplication();
                left = new BinaryExpressionNode(op.Lexeme, left, right);
            }

            return left;
        }

        private IAstNode ParseMultiplication()
        {
            IAstNode left = ParseUnary();

            while (Match(TokenType.MULTIPLY, TokenType.DIVIDE))
            {
                Token op = Previous();
                IAstNode right = ParseUnary();
                left = new BinaryExpressionNode(op.Lexeme, left, right);
            }

            return left;
        }

        private IAstNode ParseUnary()
        {
            if (Match(TokenType.MINUS))
            {
                Token op = Previous();
                return new UnaryExpressionNode(op.Lexeme, ParseUnary());
            }

            return ParsePrimary();
        }

        private IAstNode ParsePrimary()
        {
            if (Match(TokenType.INTEGER, TokenType.FLOAT))
            {
                return ParseNumber(Previous());
            }

            if (Match(TokenType.IDENTIFIER))
            {
                return new IdentifierNode(Previous().Lexeme);
            }

            if (CallableKeywords.Contains(Peek().Type) && PeekAhead(1).Type == TokenType.LPAREN)
            {
                return ParseCall();
            }

            if (ValueKeywords.Contains(Peek().Type))
            {
                return new KeywordNode(Advance().Lexeme);
            }

            if (Match(TokenType.LPAREN))
            {
                IAstNode expression = ParseExpression();
                Consume(TokenType.RPAREN, "Expected ')' after expression.");
                return expression;
            }

            throw Error(Peek(), "Expected expression.");
        }

        private IAstNode ParseNumber(Token token)
        {
            bool isPercent = Match(TokenType.PERCENT);
            return new NumberNode(token.Lexeme, isPercent);
        }

        private IAstNode ParseCall()
        {
            Token name = Advance();
            Consume(TokenType.LPAREN, "Expected '(' after function name.");

            List<IAstNode> arguments = [];
            if (!Check(TokenType.RPAREN))
            {
                do
                {
                    arguments.Add(ParseExpression());
                }
                while (Match(TokenType.COMMA));
            }

            Consume(TokenType.RPAREN, "Expected ')' after function arguments.");
            return new CallNode(name.Lexeme, arguments);
        }

        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (!Check(type))
                {
                    continue;
                }

                Advance();
                return true;
            }

            return false;
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw Error(Peek(), message);
        }

        private bool Check(TokenType type)
        {
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!Check(TokenType.EOF))
            {
                current++;
            }

            return Previous();
        }

        private Token Peek()
        {
            return tokens[current];
        }

        private Token PeekAhead(int offset)
        {
            int index = current + offset;
            return index >= tokens.Count ? tokens[^1] : tokens[index];
        }

        private Token Previous()
        {
            return tokens[current - 1];
        }

        private static ParseException Error(Token token, string message)
        {
            return new ParseException($"{message} Found '{token.Lexeme}' at line {token.Line}, column {token.Column}.");
        }
    }
}

public sealed class ParseException : Exception
{
    public ParseException(string message) : base(message)
    {
    }
}
