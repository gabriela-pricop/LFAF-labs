using System;

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

    //check if string accepted
    public bool StringBelongsToLanguage(string inputString)
    {
        HashSet<string> currentStates = new HashSet<string> { initialState };

        foreach (char ch in inputString)
        {
            string symbol = ch.ToString();
            HashSet<string> nextStates = new HashSet<string>();

            foreach (string state in currentStates)
            {
                if (transitions.ContainsKey(state) &&
                    transitions[state].ContainsKey(symbol))
                {
                    nextStates.UnionWith(transitions[state][symbol]);
                }
            }

            //no transition -> out
            if (nextStates.Count == 0)
                return false;

            currentStates = nextStates;
        }

        //check if final state
        return currentStates.Overlaps(finalStates);
    }
}
