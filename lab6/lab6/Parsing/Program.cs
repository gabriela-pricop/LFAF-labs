using lab6.Lexing;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Parsing
{
    public class Program
    {
        private const string DefaultProgram = """
        income = 5000;
        expense = 2000;
        tax = income * 15%;
        profit = income - expense - tax;
        if profit > 1000 then save(profit) else invest(profit);
        budget = income - expense;
        loss = expense - income;
        if loss > 0 then invest(loss * 50%);
        """;

        static void Main(string[] args)
        {
            string source = ReadSource(args);

            Console.WriteLine("Source program:");
            Console.WriteLine(source);
            Console.WriteLine();

            Lexer lexer = new(source);
            IReadOnlyList<Token> tokens = lexer.Tokenize();

            Console.WriteLine("Tokens:");
            foreach (Token token in tokens)
            {
                Console.WriteLine(token);
            }
            Console.WriteLine();

            if (tokens.Any(token => token.Type == TokenType.UNKNOWN))
            {
                Console.WriteLine("Lexical analysis failed because at least one UNKNOWN token was found.");
                Environment.ExitCode = 1;
                return;
            }

            try
            {
                Parser parser = new(tokens);
                Ast.ProgramNode ast = parser.Parse();

                Console.WriteLine("Abstract Syntax Tree:");
                Console.Write(ast.Format());
                Console.WriteLine($"Parsed {ast.Statements.Count} top-level statement(s).");
            }
            catch (ParseException ex)
            {
                Console.WriteLine($"Parser error: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }

        private static string ReadSource(string[] args)
        {
            if (args.Length > 0)
            {
                return File.ReadAllText(args[0]);
            }

            if (Console.IsInputRedirected)
            {
                string redirectedInput = Console.In.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(redirectedInput))
                {
                    return redirectedInput;
                }
            }

            return DefaultProgram;
        }
    }
}
