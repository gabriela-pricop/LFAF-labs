using System;
using System.Collections.Generic;
using System.Linq;

public class Grammar
{
    private HashSet<string> nonTerminals;
    private HashSet<string> terminals;
    private Dictionary<string, List<string>> productions;
    private string startSymbol;
    private Random random;

    public Grammar(HashSet<string> nonTerminals,
                   HashSet<string> terminals,
                   Dictionary<string, List<string>> productions,
                   string startSymbol)
    {
        this.nonTerminals = nonTerminals;
        this.terminals = terminals;
        this.productions = productions;
        this.startSymbol = startSymbol;
        this.random = new Random();
    }

    public void PrintGrammar()
    {
        Console.WriteLine("Regular Grammar:");
        Console.WriteLine("NonTerminals: { " + string.Join(", ", nonTerminals) + " }");
        Console.WriteLine("Terminals: { " + string.Join(", ", terminals) + " }");
        Console.WriteLine("Start Symbol: " + startSymbol);
        Console.WriteLine("Productions:");

        foreach (var kvp in productions)
        {
            Console.WriteLine($"  {kvp.Key} -> {string.Join(" | ", kvp.Value)}");
        }
    }

    public string GenerateString()
    {
        return ExpandSymbol(startSymbol);
    }

    private string ExpandSymbol(string symbol)
    {
        if (!productions.ContainsKey(symbol))
            return symbol;

        var possible = productions[symbol];
        string chosen = possible[random.Next(possible.Count)];

        string result = "";
        foreach (char ch in chosen)
            result += ExpandSymbol(ch.ToString());

        return result;
    }
}