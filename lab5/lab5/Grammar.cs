using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CnfLab
{
    public class Grammar
    {
        public HashSet<string> VN { get; private set; }
        public HashSet<string> VT { get; private set; }
        public Dictionary<string, List<List<string>>> Productions { get; private set; }
        public string Start { get; private set; }

        public Grammar(
            HashSet<string> vn,
            HashSet<string> vt,
            Dictionary<string, List<List<string>>> productions,
            string start)
        {
            VN = new HashSet<string>(vn);
            VT = new HashSet<string>(vt);
            Productions = productions.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(rhs => new List<string>(rhs)).ToList()
            );
            Start = start;
        }

        public Grammar Copy()
        {
            return new Grammar(
                new HashSet<string>(VN),
                new HashSet<string>(VT),
                Productions.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(rhs => new List<string>(rhs)).ToList()
                ),
                Start
            );
        }

        // ── Step 1: Eliminate ε-productions ──────────────────────────────────────
        public Grammar EliminateEpsilon()
        {
            var g = Copy();

            // Find nullable nonterminals
            var nullable = new HashSet<string>();
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var (head, rhsList) in g.Productions)
                {
                    if (nullable.Contains(head)) continue;
                    foreach (var rhs in rhsList)
                    {
                        if (rhs.Count == 0 || rhs.All(s => nullable.Contains(s)))
                        {
                            nullable.Add(head);
                            changed = true;
                            break;
                        }
                    }
                }
            }

            // Build new productions: for each nullable symbol, include/exclude it
            var newProds = g.VN.ToDictionary(nt => nt, _ => new List<List<string>>());
            foreach (var (head, rhsList) in g.Productions)
            {
                foreach (var rhs in rhsList)
                {
                    if (rhs.Count == 0) continue; // drop original epsilon
                    foreach (var expansion in NullableExpansions(rhs, nullable))
                    {
                        if (expansion.Count > 0 || head == g.Start)
                            if (!newProds[head].Any(r => r.SequenceEqual(expansion)))
                                newProds[head].Add(expansion);
                    }
                }
            }

            // If start was nullable, allow S -> ε
            if (nullable.Contains(g.Start))
            {
                var eps = new List<string>();
                if (!newProds[g.Start].Any(r => r.Count == 0))
                    newProds[g.Start].Add(eps);
            }

            g.Productions = newProds;
            return g;
        }

        private static IEnumerable<List<string>> NullableExpansions(List<string> rhs, HashSet<string> nullable)
        {
            var results = new List<List<string>>();
            void Backtrack(int idx, List<string> current)
            {
                if (idx == rhs.Count) { results.Add(new List<string>(current)); return; }
                var sym = rhs[idx];
                if (nullable.Contains(sym))
                    Backtrack(idx + 1, current);
                current.Add(sym);
                Backtrack(idx + 1, current);
                current.RemoveAt(current.Count - 1);
            }
            Backtrack(0, new List<string>());
            return results;
        }

        // ── Step 2: Eliminate unit (renaming) productions ─────────────────────────
        public Grammar EliminateUnit()
        {
            var g = Copy();
            var unitClosure = g.VN.ToDictionary(nt => nt, nt => UnitClosure(nt, g));

            var newProds = g.VN.ToDictionary(nt => nt, _ => new List<List<string>>());
            foreach (var head in g.VN)
            {
                foreach (var target in unitClosure[head])
                {
                    foreach (var rhs in g.Productions.GetValueOrDefault(target, new()))
                    {
                        if (rhs.Count == 1 && g.VN.Contains(rhs[0])) continue; // skip unit
                        if (!newProds[head].Any(r => r.SequenceEqual(rhs)))
                            newProds[head].Add(new List<string>(rhs));
                    }
                }
            }

            g.Productions = newProds;
            return g;
        }

        private static HashSet<string> UnitClosure(string start, Grammar g)
        {
            var closure = new HashSet<string> { start };
            var stack = new Stack<string>();
            stack.Push(start);
            while (stack.Count > 0)
            {
                var head = stack.Pop();
                foreach (var rhs in g.Productions.GetValueOrDefault(head, new()))
                    if (rhs.Count == 1 && g.VN.Contains(rhs[0]) && !closure.Contains(rhs[0]))
                    {
                        closure.Add(rhs[0]);
                        stack.Push(rhs[0]);
                    }
            }
            return closure;
        }

        // ── Step 3: Eliminate inaccessible symbols ────────────────────────────────
        public Grammar EliminateInaccessible()
        {
            var g = Copy();
            var reachable = new HashSet<string> { g.Start };
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var head in reachable.ToList())
                    foreach (var rhs in g.Productions.GetValueOrDefault(head, new()))
                        foreach (var sym in rhs)
                            if (g.VN.Contains(sym) && reachable.Add(sym))
                                changed = true;
            }

            g.VN = reachable;
            g.Productions = g.Productions
                .Where(kvp => reachable.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return g;
        }

        // ── Step 4: Eliminate nonproductive symbols ───────────────────────────────
        public Grammar EliminateNonproductive()
        {
            var g = Copy();
            var productive = new HashSet<string>();
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var (head, rhsList) in g.Productions)
                {
                    if (productive.Contains(head)) continue;
                    foreach (var rhs in rhsList)
                        if (rhs.All(s => g.VT.Contains(s) || productive.Contains(s)))
                        {
                            productive.Add(head);
                            changed = true;
                            break;
                        }
                }
            }

            g.VN = new HashSet<string>(g.VN.Intersect(productive));
            var newProds = new Dictionary<string, List<List<string>>>();
            foreach (var head in g.VN)
            {
                var filtered = g.Productions.GetValueOrDefault(head, new())
                    .Where(rhs => rhs.All(s => g.VT.Contains(s) || g.VN.Contains(s)))
                    .Select(rhs => new List<string>(rhs))
                    .ToList();
                newProds[head] = filtered;
            }
            g.Productions = newProds;
            return g;
        }

        // ── Step 5: Convert to CNF ────────────────────────────────────────────────
        public Grammar ToCnf()
        {
            var g = Copy();
            var terminalMap = new Dictionary<string, string>(); // terminal -> T_x NT
            var finalProds = g.VN.ToDictionary(nt => nt, _ => new List<List<string>>());

            string GetTerminalNT(string term)
            {
                if (!terminalMap.TryGetValue(term, out var name))
                {
                    name = FreshNonterminal($"T_{term}", g.VN);
                    g.VN.Add(name);
                    finalProds[name] = new List<List<string>> { new List<string> { term } };
                    terminalMap[term] = name;
                }
                return name;
            }

            // Phase 1: Replace terminals in long productions
            var phase1 = new Dictionary<string, List<List<string>>>();
            foreach (var (head, rhsList) in g.Productions)
            {
                phase1[head] = new List<List<string>>();
                foreach (var rhs in rhsList)
                {
                    if (rhs.Count <= 1) { phase1[head].Add(new List<string>(rhs)); continue; }
                    var replaced = rhs.Select(s => g.VT.Contains(s) ? GetTerminalNT(s) : s).ToList();
                    phase1[head].Add(replaced);
                }
            }

            // Phase 2: Binarize
            var pairMap = new Dictionary<(string, string), string>();
            string GetPairNT((string, string) pair)
            {
                if (!pairMap.TryGetValue(pair, out var name))
                {
                    name = FreshNonterminal("X", g.VN);
                    g.VN.Add(name);
                    pairMap[pair] = name;
                    finalProds[name] = new List<List<string>> { new List<string> { pair.Item1, pair.Item2 } };
                }
                return name;
            }

            foreach (var (head, rhsList) in phase1)
            {
                foreach (var rhs in rhsList)
                {
                    if (rhs.Count <= 2)
                    {
                        if (!finalProds.GetValueOrDefault(head, new()).Any(r => r.SequenceEqual(rhs)))
                        {
                            if (!finalProds.ContainsKey(head)) finalProds[head] = new();
                            finalProds[head].Add(new List<string>(rhs));
                        }
                        continue;
                    }

                    var symbols = new List<string>(rhs);
                    var curHead = head;

                    while (symbols.Count > 2)
                    {
                        var first = symbols[0];
                        symbols.RemoveAt(0);

                        if (symbols.Count == 2)
                        {
                            var pairNT = GetPairNT((symbols[0], symbols[1]));
                            if (!finalProds.ContainsKey(curHead)) finalProds[curHead] = new();
                            finalProds[curHead].Add(new List<string> { first, pairNT });
                            symbols.Clear();
                        }
                        else
                        {
                            var newHead = FreshNonterminal("X", g.VN);
                            g.VN.Add(newHead);
                            finalProds[newHead] = new List<List<string>>();
                            if (!finalProds.ContainsKey(curHead)) finalProds[curHead] = new();
                            finalProds[curHead].Add(new List<string> { first, newHead });
                            curHead = newHead;
                        }
                    }
                    if (symbols.Count == 2)
                    {
                        if (!finalProds.ContainsKey(curHead)) finalProds[curHead] = new();
                        finalProds[curHead].Add(new List<string>(symbols));
                    }
                }
            }

            g.Productions = finalProds;
            return g;
        }

        // ── CNF validation ────────────────────────────────────────────────────────
        public (bool Valid, List<string> Issues) ValidateCnf()
        {
            var issues = new List<string>();
            foreach (var (head, rhsList) in Productions)
            {
                if (!VN.Contains(head)) issues.Add($"NT {head} not in VN");
                foreach (var rhs in rhsList)
                {
                    if (rhs.Count == 0)
                    {
                        if (head != Start) issues.Add($"Epsilon not allowed: {head} -> ε");
                        continue;
                    }
                    if (rhs.Count == 1)
                    {
                        if (!VT.Contains(rhs[0])) issues.Add($"Unit must be terminal: {head} -> {rhs[0]}");
                        continue;
                    }
                    if (rhs.Count == 2)
                    {
                        if (!VN.Contains(rhs[0]) || !VN.Contains(rhs[1]))
                            issues.Add($"Binary must be NTs: {head} -> {rhs[0]}{rhs[1]}");
                        continue;
                    }
                    issues.Add($"Too long for CNF: {head} -> {string.Join("", rhs)}");
                }
            }
            return (issues.Count == 0, issues);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private static string FreshNonterminal(string prefix, HashSet<string> vn)
        {
            if (!vn.Contains(prefix)) return prefix;
            int i = 1;
            while (vn.Contains($"{prefix}{i}")) i++;
            return $"{prefix}{i}";
        }

        private string FormatRhs(List<string> rhs) =>
            rhs.Count == 0 ? "ε" : string.Join("", rhs);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"VN = {{ {string.Join(", ", VN.OrderBy(x => x))} }}");
            sb.AppendLine($"VT = {{ {string.Join(", ", VT.OrderBy(x => x))} }}");
            sb.AppendLine("P = {");
            foreach (var head in Productions.Keys.OrderBy(x => x))
            {
                var rhs = string.Join(" | ", Productions[head].Select(FormatRhs));
                sb.AppendLine($"  {head} -> {(string.IsNullOrEmpty(rhs) ? "ε" : rhs)}");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}