# Topic: Lexer / Scanner

### Course: Formal Languages & Finite Automata
### Author: Gabriela Pricop

---

## Theory

A **lexer** (also called a *scanner* or *tokenizer*) is the first phase of a language processor. Its responsibility is to read a raw stream of characters and group them into meaningful units called **tokens**. This process is known as *lexical analysis*. The principle can be summarised as *divide et impera*: rather than trying to interpret source code all at once, we first break it into the smallest meaningful pieces, and then hand those pieces to a parser.

Formally, a **token** is a pair:

$$\text{token} = \langle \text{type},\ \text{lexeme} \rangle$$

where the *type* is a category (e.g., keyword, identifier, integer literal) and the *lexeme* is the actual substring from the source that matched. Some implementations also attach metadata such as the line number for error reporting.

A **lexeme** is the raw character sequence in the source text that corresponds to a token instance. A **pattern** is the rule (often expressed as a regular expression) that describes which character sequences are valid for a given token type.

The lexer consumes a linear stream of characters and produces a linear stream of tokens:

$$\text{source characters} \xrightarrow{\text{lexer}} \text{token stream} \xrightarrow{\text{parser}} \text{syntax tree}$$

The token stream is then consumed by the parser, which requests tokens one at a time (a *pull* model), or receives the whole list at once (an *eager* model). Both models are valid; the implementation in this laboratory work exposes both via `NextToken()` and `Tokenize()`.

Token categories that any practical lexer must handle include:

- **Keywords** — reserved words with a fixed meaning in the language (e.g., `if`, `let`, `fn`).
- **Identifiers** — user-defined names (e.g., variable names, device names).
- **Literals** — integer constants, floating-point constants, string constants, boolean values.
- **Operators** — single or multi-character symbols that express operations (`=`, `==`, `!=`, `->`, …).
- **Delimiters** — structural punctuation (`(`, `)`, `{`, `}`, `;`, `,`).
- **Whitespace** — spaces, tabs, and newlines, which are normally discarded.
- **Comments** — human-readable annotations that are skipped by the lexer.
- **ILLEGAL / EOF** — sentinel tokens signalling an unrecognised character or the end of input.

The classical implementation strategy (used here, and described in the LLVM Kaleidoscope tutorial) is a hand-written *switch-on-leading-character* loop: the lexer inspects the current character, decides which category it belongs to, consumes exactly the right number of characters, and returns the corresponding token.

---

## Objectives

1. Understand what a lexer/scanner is and what role it plays in the compiler pipeline.

2. Implement a lexer for a domain-specific language (DSL) that:

   a. Defines a clear **token specification** covering all lexical categories of the language.

   b. Implements a `TokenType` enum and a `Token` class that carry type, lexeme, and source position.

   c. Implements a `Lexer` class that tokenizes identifiers, keywords, numbers, strings, operators, delimiters, and comments, and handles whitespace and error reporting.

   d. Provides a **sample input script** that exercises all token categories.

   e. Provides a **test harness** (`Program.cs`) that runs the lexer and prints the resulting token stream.

3. Apply the lexer to a concrete domain: **Samsung SmartThings smart-home automation**, with a focus on fridge-related commands (temperature control, door events, energy modes, filter alerts).

---

## Implementation Description

The solution is implemented in C# (.NET 8) and consists of four flat files: `TokenType.cs`, `Token.cs`, `Lexer.cs`, and `Program.cs`. The design deliberately mirrors the style of the FAF LFA laboratory series: minimal classes, no unnecessary abstraction, and a single coherent scan loop.

### Token Specification — `TokenType.cs`

The `TokenType` enum enumerates every legal token category. Tokens are grouped into six logical sections:

| Section | Token types |
|---|---|
| **Literals** | `INT`, `FLOAT`, `STRING`, `BOOL` |
| **Identifiers** | `IDENT` |
| **General keywords** | `LET`, `FN`, `IF`, `ELSE`, `RETURN` |
| **Smart-home keywords** | `DEVICE`, `SET`, `GET`, `WHEN`, `NOTIFY`, `ALERT`, `MODE` |
| **Fridge-domain keywords** | `FRIDGE`, `TEMPERATURE`, `DOOR`, `ENERGY` |
| **Operators** | `ASSIGN`, `EQ`, `NOT_EQ`, `LT`, `GT`, `PLUS`, `MINUS`, `SLASH`, `ASTERISK`, `ARROW` |
| **Delimiters** | `LPAREN`, `RPAREN`, `LBRACE`, `RBRACE`, `SEMICOLON`, `COMMA`, `COLON` |
| **Special** | `ILLEGAL`, `EOF` |

`BOOL` is intentionally a single type; both `"true"` and `"false"` produce a `BOOL` token. This is consistent with how `INT` and `FLOAT` work — one type, different literals — and avoids having unused variants in the enum.

```csharp
public enum TokenType
{
    // Literals
    INT,
    FLOAT,
    STRING,
    BOOL,       // true | false

    // Identifiers
    IDENT,

    // Keywords
    LET,
    FN,
    IF,
    ELSE,
    RETURN,
    DEVICE,
    SET,
    GET,
    WHEN,
    NOTIFY,
    ALERT,
    MODE,
    FRIDGE,
    TEMPERATURE,
    DOOR,
    ENERGY,

    // Operators
    ASSIGN,     // =
    EQ,         // ==
    NOT_EQ,     // !=
    LT,         // <
    GT,         // >
    PLUS,       // +
    MINUS,      // -
    SLASH,      // /
    ASTERISK,   // *
    ARROW,      // ->

    // Delimiters
    LPAREN,     // (
    RPAREN,     // )
    LBRACE,     // {
    RBRACE,     // }
    SEMICOLON,  // ;
    COMMA,      // ,
    COLON,      // :

    // Special
    ILLEGAL,
    EOF,
}
```

### Token Class — `Token.cs`

`Token` is a plain class holding three fields: the type, the raw lexeme, and the source line number. A single `ToString()` override formats it for console output.

```csharp
public class Token
{
    public TokenType Type    { get; }
    public string    Literal { get; }
    public int       Line    { get; }

    public Token(TokenType type, string literal, int line)
    {
        Type    = type;
        Literal = literal;
        Line    = line;
    }

    public override string ToString() =>
        $"[Line {Line}] {Type,-14} | {Literal}";
}
```

### Lexer Class — `Lexer.cs`

The `Lexer` class contains all scanning logic. It holds three private fields: the full source string `_input`, the current read position `_pos`, the current character `_ch`, and a line counter `_line` initialised to 1. The class exposes two public methods:

- `Tokenize()` — eagerly drains the source into a `List<Token>`, ending with an `EOF` token.
- `NextToken()` — returns one token per call; suitable for a pull-based parser.

#### Keyword Table

Keywords are stored in a static `Dictionary<string, TokenType>`. After scanning an identifier, `Keywords.TryGetValue` is called to promote the raw text to a keyword type if applicable. If no match is found, the token is classified as `IDENT`. Both `"true"` and `"false"` are entries in this table pointing to `TokenType.BOOL`.

```csharp
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
```

#### `NextToken()` — Core Scan Loop

`NextToken()` begins with a `while(true)` loop that alternates between `SkipWhitespace()` and inline comment detection. The loop only breaks when the current character is neither whitespace nor the start of a `//` comment. This two-phase approach correctly handles consecutive comment lines and blank lines without missing any newline increments in `_line`.

```csharp
public Token NextToken()
{
    while (true)
    {
        SkipWhitespace();
        if (_ch == '/' && PeekNext() == '/')
        {
            while (_ch != '\n' && _ch != '\0')
                Advance();
            continue; // re-enter so SkipWhitespace counts the \n
        }
        break;
    }

    int line = _line;

    Token tok = _ch switch
    {
        '=' => PeekNext() == '=' ? AdvanceAndMake(TokenType.EQ,     "==", line)
                                 : Make(TokenType.ASSIGN,  "=",  line),
        '!' => PeekNext() == '=' ? AdvanceAndMake(TokenType.NOT_EQ, "!=", line)
                                 : Make(TokenType.ILLEGAL, "!",  line),
        '<' => Make(TokenType.LT,        "<",  line),
        '>' => Make(TokenType.GT,        ">",  line),
        '+' => Make(TokenType.PLUS,      "+",  line),
        '-' => PeekNext() == '>' ? AdvanceAndMake(TokenType.ARROW,  "->", line)
                                 : Make(TokenType.MINUS,   "-",  line),
        '*' => Make(TokenType.ASTERISK,  "*",  line),
        '/' => Make(TokenType.SLASH,     "/",  line),
        '(' => Make(TokenType.LPAREN,    "(",  line),
        ')' => Make(TokenType.RPAREN,    ")",  line),
        '{' => Make(TokenType.LBRACE,    "{",  line),
        '}' => Make(TokenType.RBRACE,    "}",  line),
        ';' => Make(TokenType.SEMICOLON, ";",  line),
        ',' => Make(TokenType.COMMA,     ",",  line),
        ':' => Make(TokenType.COLON,     ":",  line),
        '"' => ReadString(line),
        '\0'=> new Token(TokenType.EOF,  "EOF", line),
        _   => IsLetter(_ch) ? ReadIdentOrKeyword(line)
             : IsDigit(_ch)  ? ReadNumber(line)
             :                 Make(TokenType.ILLEGAL, _ch.ToString(), line),
    };

    return tok;
}
```

The switch dispatches on the current character. Two-character tokens (`==`, `!=`, `->`) are handled by peeking one character ahead with `PeekNext()` before consuming. Any unrecognised character produces an `ILLEGAL` token.

#### `ReadIdentOrKeyword()`

Reads a maximal sequence of letters and digits (the *longest-match* rule), then performs a keyword lookup:

```csharp
private Token ReadIdentOrKeyword(int line)
{
    string text = ReadWhile(c => IsLetter(c) || IsDigit(c));

    if (Keywords.TryGetValue(text, out TokenType kw))
        return new Token(kw, text, line);

    return new Token(TokenType.IDENT, text, line);
}
```

#### `ReadNumber()`

Scans an integer and optionally extends it to a float if a decimal point followed by at least one digit is found. The presence of `PeekNext()` in the condition prevents a trailing `.` (used as a delimiter in other contexts) from being absorbed:

```csharp
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
```

#### `ReadString()`

Advances past the opening `"`, accumulates characters until a closing `"` or `\0` (end of input) is found, then advances past the closing `"`. No escape sequence handling is required for the current DSL, but the loop correctly terminates on unterminated strings to avoid an infinite read:

```csharp
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
```

#### `SkipWhitespace()`

Advances over spaces, tabs, carriage returns, and newlines. The newline case increments `_line` before calling `Advance()`, ensuring that the line counter is always accurate when the next token is read:

```csharp
private void SkipWhitespace()
{
    while (_ch == ' ' || _ch == '\t' || _ch == '\r' || _ch == '\n')
    {
        if (_ch == '\n') _line++;
        Advance();
    }
}
```

#### Low-Level Helpers

`Advance()` increments `_pos` and updates `_ch`. `PeekNext()` returns the character at `_pos + 1` without consuming it. `Make()` advances once and returns a single-character token. `AdvanceAndMake()` advances twice for two-character tokens. `ReadWhile()` accumulates characters while a predicate holds.

```csharp
private void Advance()
{
    _pos++;
    _ch = _pos < _input.Length ? _input[_pos] : '\0';
}

private char PeekNext() =>
    (_pos + 1 < _input.Length) ? _input[_pos + 1] : '\0';

private Token Make(TokenType type, string literal, int line)
{
    Advance();
    return new Token(type, literal, line);
}

private Token AdvanceAndMake(TokenType type, string literal, int line)
{
    Advance();
    Advance();
    return new Token(type, literal, line);
}

private string ReadWhile(Func<char, bool> predicate)
{
    var sb = new System.Text.StringBuilder();
    while (predicate(_ch)) { sb.Append(_ch); Advance(); }
    return sb.ToString();
}
```

### Sample Input Script and Test Harness — `Program.cs`

The program defines an inline DSL script that exercises all token categories, constructs a `Lexer`, calls `Tokenize()`, and prints each token. `.TrimStart('\n', '\r')` is applied to the raw string literal to strip the leading newline that C# inserts after the opening `"""`, which would otherwise cause the first real token to report line 2 instead of line 1.

```csharp
string input = """
    // Declare fridge device
    device fridge "SamsungFamily";
    ...
    """.TrimStart('\n', '\r');

var lexer  = new Lexer(input);
var tokens = lexer.Tokenize();

foreach (var tok in tokens)
    Console.WriteLine(tok);
```

---

## Results

Running the program against the sample fridge automation script produces the following token stream (abridged to the first two logical sections for readability):

```
Smart-Home DSL Lexer — Token Output
---------------------------------------------
Line     Type             Literal
---------------------------------------------
[Line 1] DEVICE         | device
[Line 1] FRIDGE         | fridge
[Line 1] STRING         | SamsungFamily
[Line 1] SEMICOLON      | ;
[Line 4] LET            | let
[Line 4] IDENT          | fridgeTemp
[Line 4] ASSIGN         | =
[Line 4] INT            | 38
[Line 4] SEMICOLON      | ;
[Line 5] LET            | let
[Line 5] IDENT          | freezerTemp
[Line 5] ASSIGN         | =
[Line 5] INT            | 0
[Line 5] SEMICOLON      | ;
[Line 6] SET            | set
[Line 6] FRIDGE         | fridge
[Line 6] TEMPERATURE    | temperature
[Line 6] IDENT          | fridgeTemp
[Line 6] SEMICOLON      | ;
[Line 9] IF             | if
[Line 9] LPAREN         | (
[Line 9] IDENT          | fridgeTemp
[Line 9] GT             | >
[Line 9] INT            | 40
[Line 9] RPAREN         | )
[Line 9] LBRACE         | {
[Line 10] ALERT         | alert
[Line 10] STRING        | OVERHEAT
[Line 10] SEMICOLON     | ;
[Line 11] NOTIFY        | notify
[Line 11] STRING        | TempAlert
[Line 11] ARROW         | ->
[Line 11] STRING        | owner@home.net
[Line 11] SEMICOLON     | ;
[Line 12] RBRACE        | }
...
[Line 31] LET            | let
[Line 31] IDENT          | isEco
[Line 31] ASSIGN         | =
[Line 31] BOOL           | true
[Line 31] SEMICOLON      | ;
[Line 31] EOF            | EOF
---------------------------------------------
Total tokens: 95
```

Key observations from the output:

- **Comment lines are invisible** — the three `//` comment lines in the source produce no tokens; the lexer skips them entirely and the line counter advances correctly over them (hence the jump from line 1 to line 4).
- **String contents are clean** — the surrounding `"` delimiters are consumed; the token literal is the raw content (e.g., `SamsungFamily`, not `"SamsungFamily"`).
- **Two-character operators** — `->` is emitted as a single `ARROW` token, and `==` / `!=` as `EQ` / `NOT_EQ`, confirming correct `PeekNext()` lookahead.
- **Boolean literals** — `true` produces `BOOL | true` (not `TRUE`), confirming the unified `BOOL` token type.
- **Integers and identifiers distinguished** — `38` is `INT`, `fridgeTemp` is `IDENT`.
- **Domain keywords resolved** — `fridge`, `temperature`, `door`, `energy` all resolve to their respective domain keyword types rather than `IDENT`.

---

## Conclusions

This laboratory work covered the theory and practical implementation of a lexer — the first phase of any language processor. The following results were achieved:

- A **token specification** was designed for a Smart-Home DSL with 39 distinct token types spanning literals, identifiers, general-purpose keywords, smart-home keywords, fridge-domain keywords, operators, delimiters, and sentinels.

- A clean three-class implementation (`TokenType`, `Token`, `Lexer`) was produced, following the minimal flat structure of the LFA laboratory series. The total implementation is under 200 lines.

- The **`NextToken()` scan loop** correctly handles all lexical categories through a single `switch` expression on the current character, with one-character lookahead (`PeekNext()`) for two-character tokens.

- **Comment skipping and line tracking** interact correctly: the comment-skip loop in `NextToken()` stops before the newline and re-enters `SkipWhitespace()`, which is the only place that increments `_line`. This ensures line numbers are accurate for every token.

- The **keyword promotion** pattern — scan an identifier first, then look it up in a static dictionary — correctly separates identifiers from reserved words with no special-casing in the main loop.

- The sample Samsung SmartThings fridge script produced **95 tokens** with correct types, lexemes, and line numbers, covering all implemented token categories.

- A bug-fixing cycle during development reinforced understanding of two subtle issues: the off-by-one line number caused by C#'s raw string literal leading newline, and the inconsistency between a `BOOL` literal type and separate `TRUE`/`FALSE` enum variants. Both were corrected.

---

## References

1. COJUHARI, I.; DUCA, L.; FIODOROV, I. *Formal Languages and Finite Automata – Guide for practical lessons*. Technical University of Moldova, 2022.
2. NYSTROM, R. *Crafting Interpreters*. https://craftinginterpreters.com/ — Chapter 4: Scanning.
3. LLVM Project. *Kaleidoscope: Implementing a Language with LLVM — Chapter 1: The Lexer*. https://llvm.org/docs/tutorial/MyFirstLanguageFrontend/LangImpl01.html
4. Microsoft. *C# Language Reference — Raw string literals*. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/raw-string