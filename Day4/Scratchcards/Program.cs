using System.Text.RegularExpressions;
using FluentAssertions;

namespace Scratchcards;

class Program
{
    static void Main(string[] args)
    {
        var scratchCard = new ScratchCard();
        scratchCard.Solve1("dummydata").Should().Be(13);
        scratchCard.Solve1("data");
    }
}

class ScratchCard
{
    private int _totalPoints;
    public int Solve1(string fileName)
    {
        _totalPoints = 0;
        foreach (var line in File.ReadAllLines($"Data/{fileName}"))
        {
            var cardNumber = Regex.Match(line, @"Card\s+\d").Value.Split()[1];
            var myNumbers = Regex.Matches(line.Split(" | ")[0].Split(":")[1], @"\d+").Select(m => m.Value).ToList();
            var winningNumbers = Regex.Matches(line.Split(" | ")[1], @"\d+").Select(m => m.Value).ToList();

            var entriesFound = myNumbers.Count(num => winningNumbers.Contains(num));
            int score = (int) Math.Floor(Math.Pow(2, entriesFound - 1)); 
            // Floor because entriesFound = 0 leads to 2^-1 1/2 and should become 0. Other integers remain integers
            _totalPoints += score;
        }

        Console.WriteLine($"Found {_totalPoints}");
        return _totalPoints;
    }
}