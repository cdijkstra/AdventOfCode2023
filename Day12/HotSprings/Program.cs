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
        hotSprings.Solve2("dummydata").Should().Be(525152);
        Console.WriteLine(hotSprings.Solve2("data"));
    }
}

class HotSprings()
{
    private const string OptionalDotsStart = @"^\.*";
    private const string OptionalDotsEnd = @"\.*$";
    private const string OneOrMoreDot = @"\.+";
    private const string OneOrMoreHashesEnd = @"#+$";

    private string ConstructHashRegex(int repeats) => $"#{{{repeats}}}";
    public int Solve1(string fileName)
    {
        var totalCombis = 0;
        foreach (var entry in File.ReadAllLines($"Data/{fileName}"))
        {
            var sequence = entry.Split()[0];
            var instructions = entry.Split()[1].Split(",").Select(int.Parse).ToList();
            var totalcoms = CheckCombinations(sequence, 0, instructions);

            totalCombis += totalcoms;
        }

        return totalCombis;
    }
    
    public int Solve2(string fileName)
    {
        var totalCombis = 0;
        foreach (var entry in File.ReadAllLines($"Data/{fileName}"))
        {
            var sequence = string.Join("?", Enumerable.Repeat(entry.Split()[0], 5));
            var simplifiedSequence = Regex.Replace(sequence, @"\.{2,}", ".");

            var instructions = entry.Split()[1].Split(",").Select(int.Parse).ToList();
            var mergedInstructions = string.Join(",", Enumerable.Repeat(instructions, 5).SelectMany(x => x)).Split(",").Select(int.Parse).ToList();

            var totalcoms = CheckCombinations(simplifiedSequence, 0, mergedInstructions);
            totalCombis += totalcoms;
        }

        return totalCombis;
    }

    string BuildRegexPattern(List<int> numbers)
    {
        string regexMatches = $"{OptionalDotsStart}{string.Join(OneOrMoreDot, numbers.Select(ConstructHashRegex))}{OptionalDotsEnd}";
        return regexMatches;
    }

    int CheckCombinations(string inputString, int index, List<int> instructions)
    {
        var match = 0;
        
        int indexOfFirstQuestionMark = inputString.IndexOf('?');
        // If true, no more ? will be replaced
        if (indexOfFirstQuestionMark == -1)
        {
            var regex = BuildRegexPattern(instructions);
            if (CheckPattern(inputString, regex))
            {
                match = 1;
            }

            return match;
        }

        // Check if it still satisfies the right regex, otherwise return
        var substringUntilQuestionMark = inputString.Split("?")[0].TrimEnd('#');
        if (substringUntilQuestionMark.Count(c => c == '#') > 0)
        {
            var occurrences = substringUntilQuestionMark
                .Split('.')
                .Select(part => new
                {
                    Count = part.Count(c => c == '#'),
                    Size = part.Length
                })
                .Where(result => result.Count > 0);
            var hashesFound = occurrences.Count();
            var subRegex = BuildRegexPattern(instructions.Take(hashesFound).ToList());
            if (!CheckPattern(substringUntilQuestionMark, subRegex))
            {
                // return if format is wrong
                return match;
            }
        }
        
        // Checks satisfied, continue replacing
        if (inputString[index] == '?')
        {
            // Replace '?' with '.' and continue
            inputString = inputString.Substring(0, index) + '.' + inputString.Substring(index + 1);
            match += CheckCombinations(inputString, index + 1, instructions);

            // Replace '?' with '#' and continue
            inputString = inputString.Substring(0, index) + '#' + inputString.Substring(index + 1);
            match += CheckCombinations(inputString, index + 1, instructions);
        }
        else
        {
            // Continue without replacing
            match += CheckCombinations(inputString, index + 1, instructions);
        }
        
        return match;
    }
    
    
    static bool CheckPattern(string sequence, string regex)
    {
        return Regex.IsMatch(sequence, regex);
    }
}