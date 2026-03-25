# Intro to formal languages. Regular grammars. Finite Automata.

### Course: Formal Languages & Finite Automata
### Author: Gabriela Pricop

---

## Theory

According to the guide "Formal Languages and Finite Automata" (Technical University of Moldova, 2022), a **formal language** is a set of strings over an alphabet, together with a set of rules that define which strings belong to the language. The basic components are:

- **Alphabet** – a finite, nonempty set of symbols (e.g., Σ = {a, b, c}).
- **String (or word)** – a finite sequence of symbols chosen from an alphabet. The empty string is denoted by λ.
- **Language** – any subset of Σ* (the set of all possible strings over Σ).

A **grammar** is a formal device used to generate the strings of a language. It is defined as an ordered quadruple  
G = (Vₙ, Vₜ, P, S), where:

- Vₙ – finite set of non‑terminal symbols (variables).
- Vₜ – finite set of terminal symbols (the alphabet of the language), with Vₙ ∩ Vₜ = ∅.
- S ∈ Vₙ – the start symbol.
- P – finite set of production rules (rewriting rules).

The **Chomsky hierarchy** classifies grammars into four types:

- **Type 0** (recursively enumerable) – no restrictions.
- **Type 1** (context‑sensitive) – productions of the form α₁Aα₂ → α₁βα₂.
- **Type 2** (context‑free) – productions A → β, where A ∈ Vₙ, β ∈ (Vₙ ∪ Vₜ)*.
- **Type 3** (regular) – the most restricted form. Regular grammars can be **right‑linear** (A → aB or A → a) or **left‑linear** (A → Ba or A → a), with a ∈ Vₜ, A,B ∈ Vₙ.

The grammar given for **Variant 21** is a right‑linear regular grammar, because all its productions are of the form *terminal* or *terminal* followed by a single non‑terminal:

```
S → aB
B → bS | aC | b
C → bD
D → a | bC | cS
```

Such grammars generate exactly the **regular languages**, which can be recognised by finite automata.

**Finite automata** are abstract machines with a finite number of states. They read an input string symbol by symbol and change state according to a transition function. Two common variants are:

- **Deterministic Finite Automaton (DFA)** – for each state and input symbol there is exactly one next state.
- **Nondeterministic Finite Automaton (NFA)** – for a given state and symbol there may be several possible next states; λ‑transitions (moves without consuming input) are also allowed.

When converting a right‑linear grammar to a finite automaton, the natural result is an **NFA** because a non‑terminal may have multiple productions with the same terminal (e.g., B → bS and B → b), leading to multiple transitions from the same state on the same symbol. Therefore, in this laboratory we implement an NFA to check membership of generated strings.

---

## Objectives

1. Discover what a language is and what it needs to have in order to be considered a formal one.

2. Provide the initial setup for the evolving project that you will work on during this semester.  
   a. Create a GitHub repository to deal with storing and updating your project.  
   b. Choose a programming language. Pick one that will be easiest for dealing with your tasks – you need to learn how to solve the problem itself, not everything around the problem (like setting up the project, launching it correctly, etc.).  
   c. Store reports separately in a way to make verification of your work simpler.

3. According to your variant number, get the grammar definition and do the following:  
   a. Implement a type/class for your grammar.  
   b. Add one function that would generate 5 valid strings from the language expressed by your given grammar.  
   c. Implement some functionality that would convert an object of type Grammar to one of type Finite Automaton.  
   d. For the Finite Automaton, add a method that checks if an input string can be obtained via the state transition from it.

---

## Implementation Description

### `Grammar` Class

The `Grammar` class encapsulates the definition of a right‑linear grammar and provides methods for string generation and conversion to an NFA.

#### Fields

- `HashSet<string> nonTerminals` – stores the set Vₙ.
- `HashSet<string> terminals` – stores the set Vₜ.
- `Dictionary<string, List<string>> productions` – maps each non‑terminal to a list of right‑hand sides (the productions P).
- `string startSymbol` – the start symbol S.
- `Random random` – used for randomly choosing productions during string generation.

---

### Important Methods

### `GenerateString()`

Returns one random valid string by calling the recursive helper `ExpandSymbol` on the start symbol.

```csharp
public string GenerateString()
{
    return ExpandSymbol(startSymbol);
}
```

### `ExpandSymbol(string symbol)`

Recursively expands a symbol.  
If the symbol has no productions (i.e., it is a terminal), it returns the symbol itself.  
Otherwise, it randomly picks one production from the list for that non‑terminal, then expands each character of the chosen right‑hand side and concatenates the results.

```csharp
private string ExpandSymbol(string symbol)
{
    if (!productions.ContainsKey(symbol))
        return symbol;

    var possible = productions[symbol];
    string chosen = possible[random.Next(possible.Count)];

    string result = "";
    foreach (char ch in chosen)
    {
        result += ExpandSymbol(ch.ToString());
    }
    return result;
}
```

---

### `ToFiniteAutomaton()`

Converts the grammar to an equivalent NFA.

- **States**: all non‑terminals plus a dedicated final state `"F"`.
- **Alphabet**: the terminals.
- **Transitions**:  
  - A → aB → transition from A on a to B  
  - A → a → transition from A on a to F
- **Initial state**: startSymbol  
- **Final states**: { "F" }

```csharp
public FiniteAutomaton ToFiniteAutomaton()
{
    var states = new HashSet<string>(nonTerminals) { "F" };
    var alphabet = new HashSet<string>(terminals);
    var transitions = new Dictionary<string, Dictionary<string, HashSet<string>>>();
    string initialState = startSymbol;
    var finalStates = new HashSet<string> { "F" };

    foreach (var nt in nonTerminals)
        transitions[nt] = new Dictionary<string, HashSet<string>>();
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
```

---

## `FiniteAutomaton` Class

This class models a non‑deterministic finite automaton (NFA) and provides a method to test whether a given string is accepted.

### Fields

- `HashSet<string> states` – the set of states Q.
- `HashSet<string> alphabet` – the input alphabet Σ.
- `Dictionary<string, Dictionary<string, HashSet<string>>> transitions` – the transition function δ.
- `string initialState` – the start state q₀.
- `HashSet<string> finalStates` – the set of accepting states F.

---

### `StringBelongsToLanguage(string inputString)`

Simulates the NFA on the input string.

```csharp
public bool StringBelongsToLanguage(string inputString)
{
    var currentStates = new HashSet<string> { initialState };

    foreach (char ch in inputString)
    {
        string symbol = ch.ToString();
        var nextStates = new HashSet<string>();

        foreach (string state in currentStates)
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
```

---

## Conclusions / Screenshots / Results

The implemented classes successfully model a right‑linear grammar and its equivalent non‑deterministic finite automaton. The `GenerateString` method produces valid strings according to the given grammar, and the conversion to an NFA (with a dedicated final state) correctly preserves the language. The simulation of the NFA via a set of current states correctly accepts all generated strings and rejects clearly invalid ones.

Example output (may vary due to randomness):

```
Generated strings:
ab
ab
ab
aabbbbbbbbba
abab

Checking generated words:
"ab" -> True
"ab" -> True
"ab" -> True
"aabbbbbbbbba" -> True
"abab" -> True

Checking incorrect words:
"xyz" -> False
"abc" -> False
"aaa" -> False
"b" -> False
"c" -> False
```

---

## References

[Formal Languages and Finite Automata: Guide for practical lessons](https://else.fcim.utm.md/pluginfile.php/110458/mod_resource/content/0/LFPC_Guide.pdf)

[Introduction to Automata Theory, Languages, and Computation](https://dpvipracollege.ac.in/wp-content/uploads/2023/01/John-E.-Hopcroft-Rajeev-Motwani-Jeffrey-D.-Ullman-Introduction-to-Automata-Theory-Languages-and-Computations-Prentice-Hall-2006.pdf)

[Finite-state machine – Wikipedia](https://en.wikipedia.org/wiki/Finite-state_machine)
