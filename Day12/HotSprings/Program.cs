using System.Text.RegularExpressions;
using FluentAssertions;

namespace HotSprings;

class Program
{
    static void Main(string[] args)
    {
        var hotSprings = new HotSprings();
        hotSprings.Solve1("dummydata").Should().Be(21);
        Console.WriteLine(hotSprings.Solve1("data"));
    }
}

class HotSprings()
{
    private const string OptionalDotsStart = @"^\.*";
    private const string OptionalDotsEnd = @"\.*$";
    private const string OneOrMoreDot = @"\.+";
    private string ConstructHashRegex(int repeats) => $"#{{{repeats}}}";
    public int Solve1(string fileName)
    {
        var totalCombis = 0;
        foreach (var entry in File.ReadAllLines($"Data/{fileName}"))
        {
            var sequence = entry.Split()[0];
            var instructions = entry.Split()[1].Split(",").Select(int.Parse).ToList();
            var regex = BuildRegexPattern(instructions);
            totalCombis += CheckCombinations(sequence, 0, regex);
        }

        return totalCombis;
    }

    string BuildRegexPattern(List<int> numbers)
    {
        string regexMatches = $"{OptionalDotsStart}{string.Join(OneOrMoreDot, numbers.Select(ConstructHashRegex))}{OptionalDotsEnd}";
        return regexMatches;
    }
    
    static int CheckCombinations(string inputString, int index, string regex)
    {
        var match = 0;
        if (index == inputString.Length)
        {
            // Reached the end of the string, check the pattern
            if (CheckPattern(inputString, regex))
            {
                match = 1;
            }
        }
        else
        {
            if (inputString[index] == '?')
            {
                // Replace '?' with '.' and continue
                inputString = inputString.Substring(0, index) + '.' + inputString.Substring(index + 1);
                match += CheckCombinations(inputString, index + 1, regex);

                // Replace '?' with '#' and continue
                inputString = inputString.Substring(0, index) + '#' + inputString.Substring(index + 1);
                match += CheckCombinations(inputString, index + 1, regex);
            }
            else
            {
                // Continue without replacing
                match += CheckCombinations(inputString, index + 1, regex);
            }
        }
        
        return match;
    }
    
    
    static bool CheckPattern(string sequence, string regex)
    {
        return Regex.IsMatch(sequence, regex);
    }
}