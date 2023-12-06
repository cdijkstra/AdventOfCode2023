using System.Text.RegularExpressions;
using FluentAssertions;

namespace Ferry;

class Program
{
    static void Main(string[] args)
    {
        var Ferry = new Ferry();
        Ferry.Solve1("dummydata").Should().Be(288);
        Console.WriteLine($"Answer to part 1 = {Ferry.Solve1("data")}");
        // fertilizer.Solve2("dummydata").Should().Be(46);
        // Console.WriteLine($"Answer to part 2 = {fertilizer.Solve2("data")}");
    }
}

class Ferry
{
 
    public int Solve1(string fileName)
    {
        var content = File.ReadLines($"Data/{fileName}");
        var times = content.ElementAt(0).Split(' ').Where(part => int.TryParse(part, out _)).Select(int.Parse).ToList();
        var distances = content.ElementAt(1).Split(' ').Where(part => int.TryParse(part, out _)).Select(int.Parse).ToList();
        var races = times.Zip(distances, (time, distance) => (time, distance)).ToList();

        var multipliedScore = 1;
        
        foreach (var race in races)
        {
            var possibleWaysToWin = 0;
            foreach (var buttonTime in Enumerable.Range(0, race.time - 1))
            {
                var velocity = buttonTime;
                var remainingTime = race.time - buttonTime;
                var travelledDistance = remainingTime * velocity;
                if (travelledDistance > race.distance) possibleWaysToWin++;
            }

            multipliedScore *= possibleWaysToWin;
        }
        
        return multipliedScore;
    }
    
    public int Solve2(string fileName)
    {
        var ccontent = File.ReadAllLines($"Data/{fileName}");
        // var seedsInfo = _content
        //     .Where(line => line.TrimStart().StartsWith(SeedsString))
        //     .SelectMany(line => Regex.Matches(line, @"\d+").Select(match => Int64.Parse(match.Value)))
        //     .ToList();

        return 1;
    }
}