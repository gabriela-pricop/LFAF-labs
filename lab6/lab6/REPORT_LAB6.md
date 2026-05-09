# Laboratory Work 6: Parser & Building an Abstract Syntax Tree

### Course: Formal Languages & Finite Automata
### Author: Gabriela Pricop

----

## Theory

Parsing is the syntactic analysis stage that comes after lexical analysis. A lexer reads raw text and groups it into tokens, while a parser checks whether those tokens respect the grammar of the language.

An Abstract Syntax Tree (AST) is a hierarchical representation of the meaningful syntactic constructs from the input program. Unlike a full parse tree, an AST does not keep every punctuation token. Instead, it keeps nodes such as assignments, expressions, conditionals, function calls, identifiers, and numeric literals. This makes the structure easier to use in later compiler or interpreter stages.

A recursive-descent parser is a direct way to implement a parser. Each grammar rule is represented by a method, and precedence is handled by calling the methods in the correct order: comparison, addition, multiplication, unary expressions, and primary expressions.

## Objectives

1. Reuse the Lab 3 financial DSL as the input language.
2. Define a `TokenType` enum for all supported token categories.
3. Use regular expressions during lexical analysis to identify token types.
4. Implement AST data structures for the parsed program.
5. Implement a parser that extracts syntactic information and builds an AST.

## Implementation description

### 1. Token types

The lexer uses a `TokenType` enum to classify every lexeme:

```csharp
public enum TokenType
{
    INCOME, EXPENSE, TAX, PROFIT, LOSS, SAVE, INVEST, BUDGET,
    IF, THEN, ELSE,
    IDENTIFIER, INTEGER, FLOAT, PERCENT,
    ASSIGN, PLUS, MINUS, MULTIPLY, DIVIDE,
    GREATER, LESS, GREATER_EQUAL, LESS_EQUAL, EQUAL_EQUAL, NOT_EQUAL,
    LPAREN, RPAREN, COMMA, SEMICOLON,
    EOF, UNKNOWN
}
```

This covers the same language used in the Java Lab 3 lexer: budget-related keywords, identifiers, numbers, percentage markers, arithmetic operators, comparison operators, parentheses, commas, and semicolons.

### 2. Regex-based lexer

The `Lexer` class stores token rules as pairs of `TokenType` and `Regex`. Each pattern is anchored with `\G`, which forces the match to start exactly at the current lexer position.

```csharp
private static readonly IReadOnlyList<TokenRule> TokenRules =
[
    Keyword(TokenType.INCOME, "income"),
    Keyword(TokenType.EXPENSE, "expense"),
    new(TokenType.FLOAT, new Regex(@"\G\d+\.\d+", RegexOptions.Compiled)),
    new(TokenType.INTEGER, new Regex(@"\G\d+", RegexOptions.Compiled)),
    new(TokenType.GREATER_EQUAL, new Regex(@"\G>=", RegexOptions.Compiled)),
    new(TokenType.ASSIGN, new Regex(@"\G=", RegexOptions.Compiled)),
    new(TokenType.IDENTIFIER, new Regex(@"\G[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled))
];
```

The order of rules matters. Keywords are checked before identifiers, floats are checked before integers, and multi-character operators such as `>=`, `<=`, `==`, and `!=` are checked before their single-character prefixes.

The lexer also tracks line and column numbers, which are used in parser error messages.

### 3. Grammar

The parser recognizes this grammar:

```text
program        -> statement* EOF

statement      -> ifStatement
               | assignment SEMICOLON
               | expression SEMICOLON

assignment     -> assignable ASSIGN expression
assignable     -> keyword | IDENTIFIER

ifStatement    -> IF expression THEN expression (ELSE expression)? SEMICOLON

expression     -> comparison
comparison     -> addition ((> | < | >= | <= | == | !=) addition)*
addition       -> multiplication ((+ | -) multiplication)*
multiplication -> unary ((* | /) unary)*
unary          -> - unary | primary
primary        -> INTEGER PERCENT?
               | FLOAT PERCENT?
               | IDENTIFIER
               | keyword
               | call
               | "(" expression ")"
call           -> (save | invest | budget) "(" arguments? ")"
arguments      -> expression ("," expression)*
```

This grammar preserves arithmetic precedence and allows statements such as:

```text
tax = income * 15%;
profit = income - expense - tax;
if profit > 1000 then save(profit) else invest(profit);
```

### 4. AST data structures

All AST nodes implement the same interface:

```csharp
public interface IAstNode
{
    string Format(string indent = "");
}
```

The implementation contains these node types:

| Node | Purpose |
|------|---------|
| `ProgramNode` | Root node containing all statements |
| `AssignmentNode` | Assignment such as `income = 5000` |
| `ExpressionStatementNode` | Top-level expression followed by `;` |
| `IfNode` | Conditional expression with optional `else` branch |
| `BinaryExpressionNode` | Arithmetic or comparison operation |
| `UnaryExpressionNode` | Unary minus |
| `NumberNode` | Integer or float, optionally followed by `%` |
| `IdentifierNode` | User-defined variable name |
| `KeywordNode` | DSL keyword used as a value |
| `CallNode` | Function call such as `save(profit)` |

### 5. Parser

The `Parser` class consumes the token list and builds the AST. The top-level method reads statements until `EOF`:

```csharp
public ProgramNode Parse()
{
    List<IAstNode> statements = [];

    while (!Check(TokenType.EOF))
    {
        statements.Add(ParseStatement());
    }

    return new ProgramNode(statements);
}
```

Statement parsing uses lookahead to distinguish assignments from normal expression statements:

```csharp
if (AssignmentTargets.Contains(Peek().Type) && PeekAhead(1).Type == TokenType.ASSIGN)
{
    return ParseAssignmentStatement();
}
```

Operator precedence is implemented through the call hierarchy:

```csharp
private IAstNode ParseExpression() => ParseComparison();
private IAstNode ParseComparison() { ... }
private IAstNode ParseAddition() { ... }
private IAstNode ParseMultiplication() { ... }
private IAstNode ParseUnary() { ... }
private IAstNode ParsePrimary() { ... }
```

This ensures that `income - expense - tax` is parsed as left-associative subtraction, while `income * 15%` is grouped before surrounding addition or comparison operations.

## Conclusions / Screenshots / Results

Running demo program:

```text
income = 5000;
expense = 2000;
tax = income * 15%;
profit = income - expense - tax;
if profit > 1000 then save(profit) else invest(profit);
budget = income - expense;
loss = expense - income;
if loss > 0 then invest(loss * 50%);
```

The generated AST is:

```text
Program
  Assignment[income]
    Number[5000]
  Assignment[expense]
    Number[2000]
  Assignment[tax]
    BinaryOp[*]
      Keyword[income]
      Number[15%]
  Assignment[profit]
    BinaryOp[-]
      BinaryOp[-]
        Keyword[income]
        Keyword[expense]
      Keyword[tax]
  If
    [condition]
      BinaryOp[>]
        Keyword[profit]
        Number[1000]
    [then]
      Call[save]
        Keyword[profit]
    [else]
      Call[invest]
        Keyword[profit]
  Assignment[budget]
    BinaryOp[-]
      Keyword[income]
      Keyword[expense]
  Assignment[loss]
    BinaryOp[-]
      Keyword[expense]
      Keyword[income]
  If
    [condition]
      BinaryOp[>]
        Keyword[loss]
        Number[0]
    [then]
      Call[invest]
        BinaryOp[*]
          Keyword[loss]
          Number[50%]
```

The implementation satisfies the laboratory requirements: it defines token categories through `TokenType`, uses regular expressions to recognize tokens, builds AST structures, and implements a parser that extracts syntactic information from the Lab 3 language.

## References

- [Parsing - Wikipedia](https://en.wikipedia.org/wiki/Parsing)
- [Abstract Syntax Tree - Wikipedia](https://en.wikipedia.org/wiki/Abstract_syntax_tree)
- Aho, A. V., Lam, M. S., Sethi, R., & Ullman, J. D. (2006). Compilers: Principles, Techniques, and Tools.
