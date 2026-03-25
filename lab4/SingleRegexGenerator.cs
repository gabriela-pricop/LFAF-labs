using System.Text;

namespace lab4
{
    public class SingleRegexGenerator
    {
        private const int MAX_REPETITIONS = 5;
        private readonly Random _random = new Random();

        public List<string> ProcessingSteps { get; } = new List<string>();
        public List<string> WordFormationSteps { get; } = new List<string>();

        private readonly string _regex;

        public SingleRegexGenerator(string regex)
        {
            _regex = regex;
        }

        public string Generate()
        {
            ProcessingSteps.Clear();
            WordFormationSteps.Clear();
            WordFormationSteps.Add(""); 

            var result = new StringBuilder();
            ProcessRegex(_regex, result, 0);
            return result.ToString();
        }

        private int ProcessRegex(string regex, StringBuilder result, int startIndex)
        {
            int i = startIndex;

            while (i < regex.Length)
            {
                char c = regex[i];

                if (c == '\\' && i + 1 < regex.Length)
                {
                    char escaped = regex[i + 1];
                    result.Append(escaped);
                    ProcessingSteps.Add($"Added escaped character: '{escaped}'");
                    RecordWord(result.ToString());
                    i += 2;
                    continue;
                }

                if (c == '(')
                {
                    int endIdx = FindMatchingParen(regex, i);
                    if (endIdx == -1)
                        throw new ArgumentException($"Unmatched '(' at position {i} in: {regex}");

                    string groupContent = regex.Substring(i + 1, endIdx - i - 1);
                    string[] options = SplitTopLevel(groupContent, '|');

                    char quantifier = endIdx + 1 < regex.Length ? regex[endIdx + 1] : '\0';
                    int reps = GetRepetitionCount(quantifier);

                    ProcessingSteps.Add(
                        $"Processing group ({groupContent})" +
                        (IsQuantifier(quantifier) ? $" with quantifier '{quantifier}'" : "") +
                        $"  → {reps} repetition(s)");

                    for (int rep = 0; rep < reps; rep++)
                    {
                        string selected = options[_random.Next(options.Length)];
                        ProcessingSteps.Add(
                            $"  Selected option: \"{selected}\"" +
                            (reps > 1 ? $" (rep {rep + 1}/{reps})" : ""));

                        var inner = new StringBuilder();
                        ProcessRegex(selected, inner, 0);
                        result.Append(inner);
                        RecordWord(result.ToString());
                    }

                    i = endIdx + (IsQuantifier(quantifier) ? 2 : 1);
                    continue;
                }

                char next = i + 1 < regex.Length ? regex[i + 1] : '\0';
                if (IsQuantifier(next))
                {
                    int reps = GetRepetitionCount(next);
                    ProcessingSteps.Add(
                        $"Processing '{c}' with quantifier '{next}' → {reps} repetition(s)");

                    for (int rep = 0; rep < reps; rep++)
                    {
                        result.Append(c);
                        RecordWord(result.ToString());
                    }
                    i += 2;
                }
                else
                {
                    if (IsQuantifier(c) || c == '|')
                    {
                        i++;
                        continue;
                    }

                    ProcessingSteps.Add($"Added literal character: '{c}'");
                    result.Append(c);
                    RecordWord(result.ToString());
                    i++;
                }
            }

            return i;
        }

        private string[] SplitTopLevel(string s, char delimiter)
        {
            var parts = new List<string>();
            int depth = 0;
            int start = 0;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '(') depth++;
                else if (s[i] == ')') depth--;
                else if (s[i] == delimiter && depth == 0)
                {
                    parts.Add(s.Substring(start, i - start));
                    start = i + 1;
                }
            }
            parts.Add(s.Substring(start));
            return parts.ToArray();
        }

        private static int FindMatchingParen(string s, int openIdx)
        {
            int depth = 1;
            for (int i = openIdx + 1; i < s.Length; i++)
            {
                if (s[i] == '(') depth++;
                else if (s[i] == ')') { depth--; if (depth == 0) return i; }
            }
            return -1;
        }

        private bool IsQuantifier(char c) =>
            c == '?' || c == '*' || c == '+' ||
            c == '²' || c == '³' || c == '⁴' || c == '⁵' || c == '⁺';

        private int GetRepetitionCount(char q)
        {
            return q switch
            {
                '?' => _random.Next(2),                    // 0 or 1
                '*' => _random.Next(MAX_REPETITIONS + 1),  // 0..5
                '+' => 1 + _random.Next(MAX_REPETITIONS),  // 1..5
                '²' => 2,
                '³' => 3,
                '⁴' => 4,
                '⁵' => 5,
                '⁺' => 1 + _random.Next(MAX_REPETITIONS),
                _ => 1
            };
        }

        private void RecordWord(string current) => WordFormationSteps.Add(current);
    }
}
