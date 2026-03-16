using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Variant 21 FA
        var states = new HashSet<string> { "q0", "q1", "q2", "q3" };
        var alphabet = new HashSet<string> { "a", "b", "c" };
        var finalStates = new HashSet<string> { "q3" };
        string startState = "q0";

        var transitions = new Dictionary<string, Dictionary<string, HashSet<string>>>
        {
            { "q0", new Dictionary<string, HashSet<string>>
                {
                    { "a", new HashSet<string> { "q0", "q1" } }
                }
            },
            { "q1", new Dictionary<string, HashSet<string>>
                {
                    { "b", new HashSet<string> { "q2" } }
                }
            },
            { "q2", new Dictionary<string, HashSet<string>>
                {
                    { "a", new HashSet<string> { "q2" } },
                    { "c", new HashSet<string> { "q3" } }
                }
            },
            { "q3", new Dictionary<string, HashSet<string>>
                {
                    { "c", new HashSet<string> { "q3" } }
                }
            }
        };

        var fa = new FiniteAutomaton(states, alphabet, transitions, startState, finalStates);

        Console.WriteLine("Is Deterministic? " + fa.IsDeterministic());

        var grammar = fa.ToRegularGrammar();
        grammar.PrintGrammar();

        var dfa = fa.ToDFA();
        Console.WriteLine("DFA Deterministic? " + dfa.IsDeterministic());

        fa.GenerateGraph("nfa_variant21");
        dfa.GenerateGraph("dfa_variant21");

        Console.ReadKey();
    }
}