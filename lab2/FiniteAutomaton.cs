using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;

public class FiniteAutomaton
{
    private HashSet<string> states;
    private HashSet<string> alphabet;
    private Dictionary<string, Dictionary<string, HashSet<string>>> transitions;
    private string initialState;
    private HashSet<string> finalStates;

    public FiniteAutomaton(HashSet<string> states,
                           HashSet<string> alphabet,
                           Dictionary<string, Dictionary<string, HashSet<string>>> transitions,
                           string initialState,
                           HashSet<string> finalStates)
    {
        this.states = states;
        this.alphabet = alphabet;
        this.transitions = transitions;
        this.initialState = initialState;
        this.finalStates = finalStates;
    }

    // Check determinism
    public bool IsDeterministic()
    {
        foreach (var state in transitions.Keys)
        {
            foreach (var symbol in transitions[state].Keys)
            {
                if (transitions[state][symbol].Count > 1)
                    return false;
            }
        }
        return true;
    }

    // String validation
    public bool StringBelongsToLanguage(string input)
    {
        HashSet<string> currentStates = new HashSet<string> { initialState };

        foreach (char ch in input)
        {
            string symbol = ch.ToString();
            HashSet<string> nextStates = new HashSet<string>();

            foreach (var state in currentStates)
            {
                if (transitions.ContainsKey(state) &&
                    transitions[state].ContainsKey(symbol))
                {
                    nextStates.UnionWith(transitions[state][symbol]);
                }
            }

            if (nextStates.Count == 0)
                return false;

            currentStates = nextStates;
        }

        return currentStates.Overlaps(finalStates);
    }

    // FA → Regular Grammar
    public Grammar ToRegularGrammar()
    {
        var vn = new HashSet<string>(states);
        var vt = new HashSet<string>(alphabet);
        var productions = new Dictionary<string, List<string>>();

        foreach (var state in transitions)
        {
            foreach (var symbol in state.Value)
            {
                foreach (var dest in symbol.Value)
                {
                    if (!productions.ContainsKey(state.Key))
                        productions[state.Key] = new List<string>();

                    productions[state.Key].Add(symbol.Key + dest);

                    if (finalStates.Contains(dest))
                        productions[state.Key].Add(symbol.Key);
                }
            }
        }

        return new Grammar(vn, vt, productions, initialState);
    }

    // NDFA → DFA (Subset Construction)
    public FiniteAutomaton ToDFA()
    {
        // Step 1: Initialize
        var start = new HashSet<string> { initialState };
        var dfaStates = new List<HashSet<string>> { start };
        var queue = new Queue<HashSet<string>>();
        queue.Enqueue(start);

        var dfaTransitions = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        var dfaFinalStates = new HashSet<HashSet<string>>(HashSet<string>.CreateSetComparer());

        // Step 2: Subset construction
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            string currentName = string.Join(",", current.OrderBy(s => s));

            if (!dfaTransitions.ContainsKey(currentName))
                dfaTransitions[currentName] = new Dictionary<string, HashSet<string>>();

            foreach (var symbol in alphabet)
            {
                HashSet<string> next = new HashSet<string>();

                foreach (var state in current)
                {
                    if (transitions.ContainsKey(state) &&
                        transitions[state].ContainsKey(symbol))
                    {
                        next.UnionWith(transitions[state][symbol]);
                    }
                }

                if (next.Count > 0)
                {
                    string nextName = string.Join(",", next.OrderBy(s => s));

                    dfaTransitions[currentName][symbol] = new HashSet<string> { nextName };

                    // Enqueue unseen states
                    if (!dfaStates.Any(s => s.SetEquals(next)))
                    {
                        dfaStates.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }
        }

        // Step 3: Rename DFA states 
        var stateMap = new Dictionary<string, string>();
        int index = 0;
        foreach (var state in dfaStates)
        {
            string name = string.Join(",", state.OrderBy(s => s));
            stateMap[name] = "\"{" + name + "}\"";
        }

        // Step 4: Build renamed transitions
        var renamedTransitions = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        foreach (var kvp in dfaTransitions)
        {
            string src = stateMap[kvp.Key];
            renamedTransitions[src] = new Dictionary<string, HashSet<string>>();

            foreach (var symbolKvp in kvp.Value)
            {
                string oldDest = symbolKvp.Value.First();
                string dest = stateMap[oldDest];
                renamedTransitions[src][symbolKvp.Key] = new HashSet<string> { dest };
            }
        }

        // Step 5: Determine DFA final states
        var renamedFinalStates = new HashSet<string>();
        foreach (var state in dfaStates)
        {
            if (state.Overlaps(finalStates))
            {
                string name = stateMap[string.Join(",", state.OrderBy(s => s))];
                renamedFinalStates.Add(name);
            }
        }

        // Step 6: Return new DFA
        var allDfaStateNames = new HashSet<string>(stateMap.Values);

        return new FiniteAutomaton(
            allDfaStateNames,
            alphabet,
            renamedTransitions,
            stateMap[string.Join(",", start.OrderBy(s => s))],
            renamedFinalStates
        );
    }

    // Graphical Representation
    public void GenerateGraph(string filename)
    {
        using (StreamWriter writer = new StreamWriter(filename + ".dot"))
        {
            writer.WriteLine("digraph FA {");
            writer.WriteLine("rankdir=LR;");

            writer.WriteLine("node [shape = doublecircle];");
            foreach (var f in finalStates)
                writer.WriteLine($"{f};");

            writer.WriteLine("node [shape = circle];");
            writer.WriteLine($"start [shape=none,label=\"\"];");
            writer.WriteLine($"start -> {initialState};");

            foreach (var state in transitions)
            {
                foreach (var symbol in state.Value)
                {
                    foreach (var dest in symbol.Value)
                    {
                        writer.WriteLine($"{state.Key} -> {dest} [label=\"{symbol.Key}\"];");
                    }
                }
            }

            writer.WriteLine("}");
        }

        string graphvizPath = @"C:\Program Files (x86)\Graphviz\bin\dot.exe";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = graphvizPath,
            Arguments = $"-Tpng {filename}.dot -o {filename}.png",
            CreateNoWindow = true,
            UseShellExecute = false
        };

        Process.Start(psi);
    }
}