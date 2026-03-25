namespace lab4
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("|===== Regular Expression String Generator =====|");
            Console.WriteLine();

            List<string> regexes;

            regexes = new List<string>(RegexGenerator.Variant1Regexes);
            Console.WriteLine("\nUsing Variant 1 regexes:");
            foreach (var r in regexes)
            Console.WriteLine($"  {r}");

            Console.Write("\nHow many sample strings to generate for each regex? ");
            if (!int.TryParse(Console.ReadLine(), out int sampleCount) || sampleCount < 1)
                sampleCount = 3;

            Console.WriteLine();
            Console.WriteLine("|===== GENERATION RESULTS =====");

            foreach (string regex in regexes)
            {
                Console.WriteLine($"\n  Regex: {regex}");
                Console.WriteLine(new string('-', 55));

                for (int i = 0; i < sampleCount; i++)
                {
                    var generator = new SingleRegexGenerator(regex);
                    string generated = generator.Generate();

                    Console.WriteLine($"\n  Sample {i + 1}: \"{generated}\"");

                    Console.WriteLine("\n  |===== Word Formation =====");
                    var steps = generator.WordFormationSteps;
                    for (int j = 0; j < steps.Count; j++)
                        Console.WriteLine($"  |  {j + 1,2}. \"{steps[j]}\"");
                    Console.WriteLine("  -----------------------------------------");

                    Console.WriteLine("\n  |===== Processing Sequence =====");
                    var proc = generator.ProcessingSteps;
                    for (int j = 0; j < proc.Count; j++)
                        Console.WriteLine($"  |  {j + 1,2}. {proc[j]}");
                    Console.WriteLine("  -----------------------------------------");

                    Console.WriteLine();
                }
                Console.WriteLine(new string('-', 55));
            }
        }
    }
}
