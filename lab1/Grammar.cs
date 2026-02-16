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

    public string GenerateString()
    {
        return ExpandSymbol(startSymbol);
    }

    private string ExpandSymbol(string symbol)
    {
        //terminal
        if (!productions.ContainsKey(symbol))
            return symbol;

        var possible = productions[symbol];
        string chosen = possible[random.Next(possible.Count)];

        //expansion: either terminal or non-terminal
        string result = "";
        foreach (char ch in chosen)
        {
            result += ExpandSymbol(ch.ToString());
        }
        return result;
    }

    // conversion to NFA
    public FiniteAutomaton ToFiniteAutomaton()
    {
        HashSet<string> states = new HashSet<string>(nonTerminals) { "F" };
        HashSet<string> alphabet = new HashSet<string>(terminals);
        var transitions = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        string initialState = startSymbol;
        HashSet<string> finalStates = new HashSet<string> { "F" };

        foreach (var nt in nonTerminals)
        {
            transitions[nt] = new Dictionary<string, HashSet<string>>();
        }
        transitions["F"] = new Dictionary<string, HashSet<string>>();

        foreach (var kvp in productions)
        {
            string lhs = kvp.Key;
            foreach (string rhs in kvp.Value)
            {
                string firstSymbol = rhs[0].ToString();
                string rest = rhs.Length > 1 ? rhs.Substring(1) : "";

                string dest = string.IsNullOrEmpty(rest) ? "F" : rest;

                if (!transitions[lhs].ContainsKey(firstSymbol))
                    transitions[lhs][firstSymbol] = new HashSet<string>();
                transitions[lhs][firstSymbol].Add(dest);
            }
        }

        return new FiniteAutomaton(states, alphabet, transitions, initialState, finalStates);
    }
}
