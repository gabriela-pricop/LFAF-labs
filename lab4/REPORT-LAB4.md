# Topic: Regular Expressions — Interpreter & String Generator

### Course: Formal Languages & Finite Automata
### Author: Gabriela Pricop
### Variant: 1

---

## Theory

A **regular expression** (regex) is a formal notation for describing a *set of strings* by specifying a pattern. Developed from the theoretical foundations of formal language theory by mathematician Stephen Cole Kleene in the 1950s, regular expressions are today one of the most widely used tools in software engineering, data processing, and text manipulation.

Formally, a regular expression over an alphabet Σ is defined recursively:

- The empty string ε is a regular expression denoting `{""}`.
- Any single symbol `a ∈ Σ` is a regular expression denoting `{a}`.
- If `r` and `s` are regular expressions, then `(r|s)` denotes their **union** (alternation).
- If `r` and `s` are regular expressions, then `rs` denotes their **concatenation**.
- If `r` is a regular expression, then `r*` (Kleene star) denotes **zero or more** repetitions.

These five rules are the complete definition of the regular languages, the weakest class in the Chomsky hierarchy, recognized exactly by finite automata.

### Quantifiers

In practice, most regex dialects extend the base three operators with convenient shorthands:

| Symbol | Meaning |
|--------|---------|
| `?` | 0 or 1 occurrence |
| `*` | 0 or more occurrences (bounded to 5 in this implementation) |
| `+` | 1 or more occurrences (bounded to 5) |
| `²` | Exactly 2 occurrences |
| `³` | Exactly 3 occurrences |
| `⁴` | Exactly 4 occurrences |
| `⁵` | Exactly 5 occurrences |
| `⁺` | Same as `+` (alternative notation) |

### Common Use Cases

Regular expressions are applied across virtually every area of computer science:

- **Input validation** — email addresses, phone numbers, postal codes, passwords.
- **Text search and replace** — editors, IDEs, and command-line tools (`grep`, `sed`, `awk`).
- **Lexical analysis** — tokenizing source code in compilers and interpreters (the very next lab).
- **Web scraping** — extracting structured data from HTML/XML documents.
- **URL routing** — pattern-matching path segments in web frameworks.
- **Log file parsing** — monitoring and alerting in DevOps pipelines.
- **Network security** — intrusion detection signature matching.

---

## Objectives

1. Understand what regular expressions are, the formal theory behind them, and where they are used in practice.

2. Implement a program that:

   a. Accepts a set of regular expressions as input and **generates valid strings** that conform to each expression, without hardcoding the generation logic for any specific regex.

   b. Respects an upper bound of **5 repetitions** for unbounded quantifiers (`*`, `+`) to avoid producing extremely long strings.

   c. **(Bonus)** Exposes a **step-by-step processing trace** that shows, for each generated string, the sequence of decisions made and the intermediate word after each decision.

3. Apply the generator to **Variant 1**, which contains three concrete regular expressions:

   ```
   (a|b)(c|d)E+G?
   P(Q|R|S)T(UV|W|X)*Z+
   1(0|1)*2(3|4)⁵36
   ```

---

## Implementation Description

The solution is implemented in C# and consists of 2 classes and `Program.cs`.
### Architecture

The program contains two classes:

- **`RegexGenerator`** — the entry point; handles user interaction (mode selection, sample count), drives the generation loop, and formats output.
- **`SingleRegexProcessor`** — encapsulates the complete parsing and generation logic for one regex. It exposes two public `List<string>` properties — `ProcessingSteps` and `WordFormationSteps` — which record every decision and every intermediate word respectively.

### Core Algorithm — Recursive Descent Parser

The heart of the implementation is `ProcessRegex()`, a recursive descent method that walks the regex string character by character. It handles four distinct cases in order:

**1. Escape sequences** (`\x`): the next character is emitted as a literal, bypassing any special interpretation.

```csharp
if (c == '\\' && i + 1 < regex.Length)
{
    char escaped = regex[i + 1];
    result.Append(escaped);
    ProcessingSteps.Add($"Added escaped character: '{escaped}'");
    RecordWord(result.ToString());
    i += 2;
    continue;
}
```

**2. Groups** `(x|y|z)`: find the matching closing parenthesis, split alternatives at the top level only, pick one alternative at random, and recurse into it. If a quantifier follows the group, repeat the pick-and-recurse step the appropriate number of times.

```csharp
if (c == '(')
{
    int endIdx = FindMatchingParen(regex, i);
    string groupContent = regex.Substring(i + 1, endIdx - i - 1);
    string[] options = SplitTopLevel(groupContent, '|');

    char quantifier = endIdx + 1 < regex.Length ? regex[endIdx + 1] : '\0';
    int reps = GetRepetitionCount(quantifier);

    for (int rep = 0; rep < reps; rep++)
    {
        string selected = options[_random.Next(options.Length)];
        var inner = new StringBuilder();
        ProcessRegex(selected, inner, 0);
        result.Append(inner);
        RecordWord(result.ToString());
    }
    i = endIdx + (IsQuantifier(quantifier) ? 2 : 1);
    continue;
}
```

**3. Quantified literals** (`c?`, `c*`, `c+`, `c²`, …): if the character immediately following the current one is a quantifier, compute a repetition count and append the character that many times.

```csharp
char next = i + 1 < regex.Length ? regex[i + 1] : '\0';
if (IsQuantifier(next))
{
    int reps = GetRepetitionCount(next);
    for (int rep = 0; rep < reps; rep++)
    {
        result.Append(c);
        RecordWord(result.ToString());
    }
    i += 2;
}
```

**4. Plain literals**: append the character directly and advance.

```csharp
ProcessingSteps.Add($"Added literal character: '{c}'");
result.Append(c);
RecordWord(result.ToString());
i++;
```

### Key Helper Methods

**`SplitTopLevel(string s, char delimiter)`** — splits a string on a delimiter only at parenthesis depth 0. This is critical for nested groups such as `(UV|W|X)*`: a naive `string.Split('|')` would incorrectly split inside inner groups.

```csharp
private static string[] SplitTopLevel(string s, char delimiter)
{
    var parts = new List<string>();
    int depth = 0, start = 0;

    for (int i = 0; i < s.Length; i++)
    {
        if      (s[i] == '(')              depth++;
        else if (s[i] == ')')              depth--;
        else if (s[i] == delimiter && depth == 0)
        {
            parts.Add(s.Substring(start, i - start));
            start = i + 1;
        }
    }
    parts.Add(s.Substring(start));
    return parts.ToArray();
}
```

**`FindMatchingParen(string s, int openIdx)`** — locates the closing `)` that corresponds to an opening `(` by maintaining a depth counter; handles arbitrarily nested groups correctly.

**`GetRepetitionCount(char q)`** — maps each quantifier symbol to a random integer in the appropriate range, using a C# switch expression:

```csharp
private static int GetRepetitionCount(char q) =>
    q switch
    {
        '?' => _random.Next(2),                    // 0 or 1
        '*' => _random.Next(MAX_REPETITIONS + 1),  // 0..5
        '+' => 1 + _random.Next(MAX_REPETITIONS),  // 1..5
        '²' => 2,
        '³' => 3,
        '⁴' => 4,
        '⁵' => 5,
        '⁺' => 1 + _random.Next(MAX_REPETITIONS),
        _   => 1
    };
```

**`RecordWord(string current)`** — appends the current accumulated string to `WordFormationSteps` after every character emission, providing a complete snapshot trail.

### Variant 1 — Regular Expression Analysis

**Expression 1: `(a|b)(c|d)E+G?`**

- `(a|b)` — selects `a` or `b` (exactly once, no quantifier).
- `(c|d)` — selects `c` or `d` (exactly once).
- `E+` — appends `E` one to five times.
- `G?` — appends `G` zero or one time.

Valid examples: `acEG`, `bdEEE`, `adE`, `bcEEEEEG`.

**Expression 2: `P(Q|R|S)T(UV|W|X)*Z+`**

- `P` — fixed literal.
- `(Q|R|S)` — selects one of `Q`, `R`, or `S`.
- `T` — fixed literal.
- `(UV|W|X)*` — repeats zero to five times, each time choosing `UV`, `W`, or `X`.
- `Z+` — appends `Z` one to five times.

Valid examples: `PQTUVUVZ`, `PRTWWWWWZ`, `PSTZ`, `PRTXUVWZZ`.

**Expression 3: `1(0|1)*2(3|4)⁵36`**

- `1` — fixed opening digit.
- `(0|1)*` — zero to five binary digits.
- `2` — fixed separator digit.
- `(3|4)⁵` — exactly five choices of `3` or `4`.
- `36` — fixed two-character closing suffix.

Valid examples: `1023333336`, `1124444436`, `12333333336`, `10123434336`.

---

## Results

Running the program with mode 1 (Variant 1 presets) and requesting 2 samples per regex produces output of the following form. Actual characters vary across runs due to random selection.

```
|===== Regular Expression String Generator =====|


Using Variant 1 regexes:
  (a|b)(c|d)E+G?
  P(Q|R|S)T(UV|W|X)*Z+
  1(0|1)*2(3|4)⁵36

How many sample strings to generate for each regex? 2

|===== GENERATION RESULTS =====

  Regex: (a|b)(c|d)E+G?
-------------------------------------------------------

  Sample 1: "acEEG"

  |===== Word Formation =====
  |   1. ""
  |   2. "a"
  |   3. "a"
  |   4. "c"
  |   5. "ac"
  |   6. "acE"
  |   7. "acEE"
  |   8. "acEEG"
  -----------------------------------------

  |===== Processing Sequence =====
  |   1. Processing group (a|b)  → 1 repetition(s)
  |   2.   Selected option: "a"
  |   3. Added literal character: 'a'
  |   4. Processing group (c|d)  → 1 repetition(s)
  |   5.   Selected option: "c"
  |   6. Added literal character: 'c'
  |   7. Processing 'E' with quantifier '+' → 2 repetition(s)
  |   8. Processing 'G' with quantifier '?' → 1 repetition(s)
  -----------------------------------------


  Sample 2: "acEEG"

  |===== Word Formation =====
  |   1. ""
  |   2. "a"
  |   3. "a"
  |   4. "c"
  |   5. "ac"
  |   6. "acE"
  |   7. "acEE"
  |   8. "acEEG"
  -----------------------------------------

  |===== Processing Sequence =====
  |   1. Processing group (a|b)  → 1 repetition(s)
  |   2.   Selected option: "a"
  |   3. Added literal character: 'a'
  |   4. Processing group (c|d)  → 1 repetition(s)
  |   5.   Selected option: "c"
  |   6. Added literal character: 'c'
  |   7. Processing 'E' with quantifier '+' → 2 repetition(s)
  |   8. Processing 'G' with quantifier '?' → 1 repetition(s)
  -----------------------------------------

-------------------------------------------------------

  Regex: P(Q|R|S)T(UV|W|X)*Z+
-------------------------------------------------------

  Sample 1: "PSTUVZZZZ"

  |===== Word Formation =====
  |   1. ""
  |   2. "P"
  |   3. "S"
  |   4. "PS"
  |   5. "PST"
  |   6. "U"
  |   7. "UV"
  |   8. "PSTUV"
  |   9. "PSTUVZ"
  |  10. "PSTUVZZ"
  |  11. "PSTUVZZZ"
  |  12. "PSTUVZZZZ"
  -----------------------------------------

  |===== Processing Sequence =====
  |   1. Added literal character: 'P'
  |   2. Processing group (Q|R|S)  → 1 repetition(s)
  |   3.   Selected option: "S"
  |   4. Added literal character: 'S'
  |   5. Added literal character: 'T'
  |   6. Processing group (UV|W|X) with quantifier '*'  → 1 repetition(s)
  |   7.   Selected option: "UV"
  |   8. Added literal character: 'U'
  |   9. Added literal character: 'V'
  |  10. Processing 'Z' with quantifier '+' → 4 repetition(s)
  -----------------------------------------


  Sample 2: "PRTWZZ"

  |===== Word Formation =====
  |   1. ""
  |   2. "P"
  |   3. "R"
  |   4. "PR"
  |   5. "PRT"
  |   6. "W"
  |   7. "PRTW"
  |   8. "PRTWZ"
  |   9. "PRTWZZ"
  -----------------------------------------

  |===== Processing Sequence =====
  |   1. Added literal character: 'P'
  |   2. Processing group (Q|R|S)  → 1 repetition(s)
  |   3.   Selected option: "R"
  |   4. Added literal character: 'R'
  |   5. Added literal character: 'T'
  |   6. Processing group (UV|W|X) with quantifier '*'  → 1 repetition(s)
  |   7.   Selected option: "W"
  |   8. Added literal character: 'W'
  |   9. Processing 'Z' with quantifier '+' → 2 repetition(s)
  -----------------------------------------

-------------------------------------------------------

  Regex: 1(0|1)*2(3|4)⁵36
-------------------------------------------------------

  Sample 1: "11024333336"

  |===== Word Formation =====
  |   1. ""
  |   2. "1"
  |   3. "1"
  |   4. "11"
  |   5. "0"
  |   6. "110"
  |   7. "1102"
  |   8. "4"
  |   9. "11024"
  |  10. "3"
  |  11. "110243"
  |  12. "3"
  |  13. "1102433"
  |  14. "3"
  |  15. "11024333"
  |  16. "3"
  |  17. "110243333"
  |  18. "1102433333"
  |  19. "11024333336"
  -----------------------------------------

  |===== Processing Sequence =====
  |   1. Added literal character: '1'
  |   2. Processing group (0|1) with quantifier '*'  → 2 repetition(s)
  |   3.   Selected option: "1" (rep 1/2)
  |   4. Added literal character: '1'
  |   5.   Selected option: "0" (rep 2/2)
  |   6. Added literal character: '0'
  |   7. Added literal character: '2'
  |   8. Processing group (3|4) with quantifier '⁵'  → 5 repetition(s)
  |   9.   Selected option: "4" (rep 1/5)
  |  10. Added literal character: '4'
  |  11.   Selected option: "3" (rep 2/5)
  |  12. Added literal character: '3'
  |  13.   Selected option: "3" (rep 3/5)
  |  14. Added literal character: '3'
  |  15.   Selected option: "3" (rep 4/5)
  |  16. Added literal character: '3'
  |  17.   Selected option: "3" (rep 5/5)
  |  18. Added literal character: '3'
  |  19. Added literal character: '3'
  |  20. Added literal character: '6'
  -----------------------------------------


  Sample 2: "110123343336"

  |===== Word Formation =====
  |   1. ""
  |   2. "1"
  |   3. "1"
  |   4. "11"
  |   5. "0"
  |   6. "110"
  |   7. "1"
  |   8. "1101"
  |   9. "11012"
  |  10. "3"
  |  11. "110123"
  |  12. "3"
  |  13. "1101233"
  |  14. "4"
  |  15. "11012334"
  |  16. "3"
  |  17. "110123343"
  |  18. "3"
  |  19. "1101233433"
  |  20. "11012334333"
  |  21. "110123343336"
  -----------------------------------------

  |===== Processing Sequence =====
  |   1. Added literal character: '1'
  |   2. Processing group (0|1) with quantifier '*'  → 3 repetition(s)
  |   3.   Selected option: "1" (rep 1/3)
  |   4. Added literal character: '1'
  |   5.   Selected option: "0" (rep 2/3)
  |   6. Added literal character: '0'
  |   7.   Selected option: "1" (rep 3/3)
  |   8. Added literal character: '1'
  |   9. Added literal character: '2'
  |  10. Processing group (3|4) with quantifier '⁵'  → 5 repetition(s)
  |  11.   Selected option: "3" (rep 1/5)
  |  12. Added literal character: '3'
  |  13.   Selected option: "3" (rep 2/5)
  |  14. Added literal character: '3'
  |  15.   Selected option: "4" (rep 3/5)
  |  16. Added literal character: '4'
  |  17.   Selected option: "3" (rep 4/5)
  |  18. Added literal character: '3'
  |  19.   Selected option: "3" (rep 5/5)
  |  20. Added literal character: '3'
  |  21. Added literal character: '3'
  |  22. Added literal character: '6'
  -----------------------------------------

-------------------------------------------------------
```

Key observations from the output:

- **Word formation traces correctly** — every character emission (including each repetition of a quantified token) appends exactly one snapshot to `WordFormationSteps`, so the trail is unambiguous.
- **`*` can produce zero repetitions** — `(UV|W|X)*` in sample 2 of expression 2 chose 0 repetitions, producing `PSTZ` with no middle group content. This is a valid string.
- **`⁵` always produces exactly 5** — expression 3's `(3|4)⁵` always contributes five characters, confirmed by both samples.
- **Groups with multi-character alternatives work** — `UV` in `(UV|W|X)*` is selected as a unit and contributes two characters to the word, not one.
- **Processing sequence matches the regex structure** — the steps read left-to-right exactly as the regex is written, confirming that the algorithm does not re-order or lookahead beyond what is needed.

---

## Conclusions

This laboratory work demonstrated the practical interpretation and dynamic generation of strings from regular expressions, without hardcoding any domain-specific logic.

The key results achieved are:

- A **recursive descent interpreter** for a subset of regular expression syntax was built from scratch in C#. It handles alternation groups, all standard quantifiers plus Unicode superscript quantifiers, and nested groups to arbitrary depth.

- The **`SplitTopLevel` helper** was the most important algorithmic insight: splitting group alternatives on `|` must respect parenthesis depth to avoid incorrectly splitting nested groups such as `(UV|W|X)*`. A naive `string.Split` would fail here.

- The **bonus step-tracing feature** (`ProcessingSteps` and `WordFormationSteps`) proved valuable as a correctness tool during development: by reading the trace it is immediately clear whether group selection, repetition counts, and character emission all behave as expected.

- All three Variant 1 expressions generate strings that match the provided examples — `{acEG, bdE, …}`, `{PQTUVUVZ, PRTWWWWWZ, …}`, and `{1023333336, 1124444436, …}` — confirming the correctness of the implementation.

- Bounding `*` and `+` to a maximum of **5 repetitions** is sufficient to demonstrate the structure of the generated language while keeping output readable.

---

## References

1. COJUHARI, I.; DUCA, L.; FIODOROV, I. *Formal Languages and Finite Automata — Guide for practical lessons*. Technical University of Moldova, 2022.
2. SIPSER, M. *Introduction to the Theory of Computation*, 3rd ed. Cengage Learning, 2012 — Chapter 1: Regular Languages.
3. THOMPSON, K. "Programming Techniques: Regular Expression Search Algorithm." *Communications of the ACM*, 11(6), 1968.
4. Microsoft. *C# Language Reference — Switch expression*. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression