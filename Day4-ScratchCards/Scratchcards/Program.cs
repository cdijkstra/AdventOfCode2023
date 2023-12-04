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
        scratchCard.Solve2("dummydata").Should().Be(30);
        scratchCard.Solve2("data");
    }
}

class ScratchCard
{
    private int _totalPoints;
    private Dictionary<int, int> _copies;
    public int Solve1(string fileName)
    {
        _totalPoints = 0;
        foreach (var card in File.ReadAllLines($"Data/{fileName}"))
        {
            var entriesFound = DetermineNumberWinningNumbers(card);
            int score = (int) Math.Floor(Math.Pow(2, entriesFound - 1)); // Floor because entriesFound = 0 leads to 2^-1 1/2 and should become 0. Other integers remain integers
            _totalPoints += score;
        }
        return _totalPoints;
    }

    public int Solve2(string fileName)
    {
        var allCards = File.ReadAllLines($"Data/{fileName}");
        _copies = Enumerable.Range(0, allCards.Length).ToDictionary(x => x, x => 1);
        _totalPoints = 0;
        for (var index = 0; index < allCards.Length; index++)
        {
            var card = allCards[index];
            var entriesFound = DetermineNumberWinningNumbers(card);
            Enumerable.Range(0, _copies[index])
                .SelectMany(_ => Enumerable.Range(index + 1, entriesFound))
                .ToList()
                .ForEach(idx => _copies[idx]++);
        }

        var totalStampCards = _copies.Sum(x => x.Value);
        return totalStampCards;
    }
    
    private static int DetermineNumberWinningNumbers(string line)
    {
        var myNumbers = Regex.Matches(line.Split(" | ")[0].Split(":")[1], @"\d+").Select(m => m.Value).ToList();
        var winningNumbers = Regex.Matches(line.Split(" | ")[1], @"\d+").Select(m => m.Value).ToList();

        var entriesFound = myNumbers.Count(num => winningNumbers.Contains(num));
        return entriesFound;
    }
}