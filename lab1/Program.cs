var nonTerminals = new HashSet<string> { "S", "B", "C", "D" };
var terminals = new HashSet<string> { "a", "b", "c" };
var productions = new Dictionary<string, List<string>>
        {
            { "S", new List<string> { "aB" } },
            { "B", new List<string> { "bS", "aC", "b" } },
            { "C", new List<string> { "bD" } },
            { "D", new List<string> { "a", "bC", "cS" } }
        };
string startSymbol = "S";

Grammar grammar = new Grammar(nonTerminals, terminals, productions, startSymbol);

List<string> generatedWords = new List<string>();
Console.WriteLine("Generated strings:");
for (int i = 0; i < 5; i++)
{
    string word = grammar.GenerateString();
    generatedWords.Add(word);
    Console.WriteLine(word);
}

FiniteAutomaton fa = grammar.ToFiniteAutomaton();

Console.WriteLine("\nChecking generated words:");
foreach (string word in generatedWords)
{
    bool belongs = fa.StringBelongsToLanguage(word);
    Console.WriteLine($"\"{word}\" -> {belongs}");
}

List<string> incorrectWords = new List<string> { "xyz", "abc", "aaa", "b", "c" };
Console.WriteLine("\nChecking incorrect words:");
foreach (string word in incorrectWords)
{
    bool belongs = fa.StringBelongsToLanguage(word);
    Console.WriteLine($"\"{word}\" -> {belongs}");
}

Console.ReadKey();