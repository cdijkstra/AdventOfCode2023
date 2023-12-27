using System.Text.RegularExpressions;
using FluentAssertions;

namespace HotSprings;

class Program
{
    static void Main(string[] args)
    {
        var hotSprings = new HotSprings();
        hotSprings.Solve1("firsttestcase").Should().Be(10);
        hotSprings.Solve1("dummydata").Should().Be(21);
        Console.WriteLine(hotSprings.Solve1("data"));
        hotSprings.Solve2("dummydata").Should().Be(525152);
        Console.WriteLine(hotSprings.Solve2("data"));
    }
}

class HotSprings()
{
    private Dictionary<string, long> _cache = new();
    private long _totalCombis = 0;
    
    public long Solve1(string fileName)
    {
        _totalCombis = 0;
        foreach (var entry in File.ReadAllLines($"Data/{fileName}"))
        {
            var sequence = entry.Split()[0];
            var instructions = entry.Split()[1].Split(",").Select(int.Parse).ToList();
            var contribution = GetCount(sequence, instructions);
            Console.WriteLine($"Contribution = {contribution}");
            _totalCombis += contribution;
        }

        return _totalCombis;
    }
    
    public long Solve2(string fileName)
    {
        _totalCombis = 0;
        foreach (var entry in File.ReadAllLines($"Data/{fileName}"))
        {
            var sequence = string.Join("?", Enumerable.Repeat(entry.Split()[0], 5));
            var simplifiedSequence = Regex.Replace(sequence, @"\.{2,}", ".");

            var instructions = entry.Split()[1].Split(",").Select(int.Parse).ToList();
            var mergedInstructions = string.Join(",", Enumerable.Repeat(instructions, 5).SelectMany(x => x)).Split(",").Select(int.Parse).ToList();

            var contribution = Calculate(simplifiedSequence, mergedInstructions);
            Console.WriteLine($"Contribution = {contribution}");
            _totalCombis += contribution;
        }

        return _totalCombis;
    }

    private long Calculate(string springs, List<int> groups)
    {
        var key = $"{springs}{string.Join(',', groups)}";
        if (_cache.TryGetValue(key, out var calculate))
        {
            return calculate;
        }

        var value = GetCount(springs, groups);
        _cache.Add(key, value);
        return value;
    }
    
    private long GetCount(string springs, List<int> groups)
    {
        // Can perhaps be refactored... 
        while (true)
        {
            if (groups.Count == 0)
            {
                var returnValue = !springs.Contains('#') ? 1 : 0;
                return returnValue;
            }
            if (groups.Count > 0 && string.IsNullOrEmpty(springs))
            {
                return 0;
            }
            
            if (springs.StartsWith('.'))
            {
                springs = springs.Trim('.'); // Remove all dots from the beginning
                continue;
            }

            if (springs.StartsWith('?'))
            {
                return Calculate('.' + springs[1..], groups) + Calculate('#' + springs[1..], groups);
            }

            if (springs.StartsWith('#')) // Start of a group
            {
                if (groups.Count == 1 && springs.Length == groups[0])
                {
                    if (springs.All(ch => ch == '#' || ch == '?'))
                    {
                        return 1;
                    }
                }
                
                if (groups.Count > 0)
                {
                    if (springs.Length <= groups[0] || springs[groups[0]] == '#')
                    {
                        return 0; // Group cannot be followed by a spring, and there must be enough characters left
                    }
                }
                
                var substringWithCorrectLength = springs[..groups[0]];
                if (substringWithCorrectLength.Contains('.')) // If it contains ? they have to become #, resulting in 1 option
                {
                    return 0;
                }

                if (springs.Length == groups[0])
                {
                    return 1;
                }
                
                springs = springs[(groups[0] + 1)..];
                groups = groups[1..];
            }
        }
    }
}