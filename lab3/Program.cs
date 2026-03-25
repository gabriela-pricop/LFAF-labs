using System.Diagnostics.Metrics;

namespace lab3
{
    public class Program
    {
        static void Main()
        {

            string input = """
            device fridge "SamsungFamily";
            let fridgeTemp  = 38;
            let freezerTemp = 0;
            set fridge temperature fridgeTemp;
            if (fridgeTemp > 40) {
                alert "OVERHEAT";
                notify "TempAlert" -> "owner@home.net";
            }
            when fridge door {
                let elapsed = get fridge door;
                if (elapsed > 60) {
                    notify "DoorAjar" -> "family";
                }
            }
            mode fridge energy = "eco";
            fn getReport(zone) {
                return get fridge temperature;
            }
            let avg = (38 + 0) / 2;
            let isEco = true;
            """.TrimStart('\n', '\r');

            var lexer = new SmartHomeLexer(input);
            var tokens = lexer.Tokenize();

            Console.WriteLine("Smart-Home DSL Lexer — Token Output");
            Console.WriteLine(new string('-', 45));
            Console.WriteLine($"{"Line",-8} {"Type",-16} Literal");
            Console.WriteLine(new string('-', 45));

            foreach (var tok in tokens)
                Console.WriteLine(tok);

            Console.WriteLine(new string('-', 45));
            Console.WriteLine($"Total tokens: {tokens.Count}");
        }
    }
}
