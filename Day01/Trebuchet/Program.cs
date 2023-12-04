using System.Text.RegularExpressions;
using FluentAssertions;

class Program
{
    public static void Main(string[] args)
    {
        var trebuchet = new Trebuchet();
        trebuchet.CalculateNumber("dummydata.txt", false).Should().Be(142);
        trebuchet.CalculateNumber("dummydata2.txt", true).Should().Be(281 + 98 + 22 + 21);
        Console.WriteLine($"Answer to 1 is {trebuchet.CalculateNumber("data.txt", false)}");
        Console.WriteLine($"Answer to 2 is {trebuchet.CalculateNumber("data.txt", true)}");
    }
}

class Trebuchet
{
    private List<string> possibleNumbers = new()
    {
        "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"
    };

    private Dictionary<string, string> numberConversion = new()
    {
        { "one",  "1"}, { "two",  "2"}, { "three", "3"}, { "four",  "4"}, { "five", "5"}, { "six",  "6"}, { "seven", "7"}, { "eight", "8"}, { "nine", "9"}
    };
    
    public int CalculateNumber(string fileName, bool secondExercise)
    {
        var combinedNumber = 0;
        var firstRegex = secondExercise ? new Regex("\\d|one|two|three|four|five|six|seven|eight|nine") : new Regex("\\d");
        foreach (var inputLine in File.ReadAllLines(fileName))
        {
            var matches = new List<string>();
            for (int i = 0; i < inputLine.Length; i++)
            {
                var match = firstRegex.Match(inputLine, i);
                if (match.Success)
                {
                    matches.Add(match.Value);
                }
            }
            
            var firstNumber = GetConvertedValue(matches.First());
            var lastNumber = GetConvertedValue(matches.Last());
            combinedNumber += int.Parse(string.Concat(firstNumber, lastNumber));
        }

        return combinedNumber;
    }
    
    string GetConvertedValue(string match) => possibleNumbers.Contains(match) ? numberConversion[match] : match;
}