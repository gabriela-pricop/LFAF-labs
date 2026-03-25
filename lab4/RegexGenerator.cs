namespace lab4
{
    public class RegexGenerator
    {

        private const int MAX_REPETITIONS = 5;
        private readonly Random _random = new Random();

        public static readonly string[] Variant1Regexes = [
            "(a|b)(c|d)E+G?",
            "P(Q|R|S)T(UV|W|X)*Z+",
            "1(0|1)*2(3|4)⁵36"
        ];
    }
}
