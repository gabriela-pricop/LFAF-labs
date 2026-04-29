using System;
using System.Collections.Generic;
using System.Linq;
using CnfLab;

static List<string> Rhs(string text)
{
    var t = text.Trim();
    if (t == "" || t == "eps" || t == "ε") return new List<string>();
    if (t.Contains(' ')) return new List<string>(t.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    return new List<string>(t.Select(c => c.ToString()).ToList());
}

static void RunPipeline(Grammar grammar, string label)
{
    Console.WriteLine($"╔══════════════════════════════════════════════════════════╗");
    Console.WriteLine($"║  {label,-56}║");
    Console.WriteLine($"╚══════════════════════════════════════════════════════════╝");

    Console.WriteLine("\n── Original Grammar ──");
    Console.Write(grammar);

    var step1 = grammar.EliminateEpsilon();
    Console.WriteLine("\n── Step 1: After eliminating ε-productions ──");
    Console.Write(step1);

    var step2 = step1.EliminateUnit();
    Console.WriteLine("\n── Step 2: After eliminating unit (renaming) productions ──");
    Console.Write(step2);

    var step3 = step2.EliminateInaccessible();
    Console.WriteLine("\n── Step 3: After eliminating inaccessible symbols ──");
    Console.Write(step3);

    var step4 = step3.EliminateNonproductive();
    Console.WriteLine("\n── Step 4: After eliminating nonproductive symbols ──");
    Console.Write(step4);

    var cnf = step4.ToCnf();
    Console.WriteLine("\n── Step 5: Chomsky Normal Form ──");
    Console.Write(cnf);

    var (valid, issues) = cnf.ValidateCnf();
    Console.WriteLine("\n── CNF Validation ──");
    if (valid)
        Console.WriteLine("✔ All productions conform to CNF.");
    else
    {
        Console.WriteLine("✘ Issues found:");
        foreach (var issue in issues) Console.WriteLine($"  - {issue}");
    }
    Console.WriteLine();
}

Console.WriteLine("Lab 3 — Chomsky Normal Form");
Console.WriteLine("Student Variant: 21\n");

// Variant 21
var vn21 = new HashSet<string> { "S", "A", "B", "C", "D" };
var vt21 = new HashSet<string> { "a", "b", "d" };
var prods21 = new Dictionary<string, List<List<string>>>
{
    ["S"] = new() { Rhs("d B"), Rhs("A C") },
    ["A"] = new() { Rhs("d"), Rhs("d S"), Rhs("a B d B") },
    ["B"] = new() { Rhs("a"), Rhs("a A"), Rhs("A C") },
    ["C"] = new() { Rhs("b C"), Rhs("eps") },
    ["D"] = new() { Rhs("A B") },
};
var grammar21 = new Grammar(vn21, vt21, prods21, "S");
RunPipeline(grammar21, "Variant 21");

// Bonus: Python reference Variant 20
Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("BONUS -- Generic Grammar (Python reference: Variant 20)\n");

var vnB = new HashSet<string> { "S", "A", "B", "C", "D" };
var vtB = new HashSet<string> { "a", "b" };
var prodsB = new Dictionary<string, List<List<string>>>
{
    ["S"] = new() { Rhs("a B"), Rhs("b A"), Rhs("A") },
    ["A"] = new() { Rhs("B"), Rhs("S a"), Rhs("b B A"), Rhs("b") },
    ["B"] = new() { Rhs("b"), Rhs("b S"), Rhs("a D"), Rhs("eps") },
    ["D"] = new() { Rhs("A A") },
    ["C"] = new() { Rhs("B a") },
};
var grammarB = new Grammar(vnB, vtB, prodsB, "S");
RunPipeline(grammarB, "Bonus: Variant 20 (Python reference)");