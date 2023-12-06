using FluentAssertions;
namespace Ferry;

class Program
{
    static void Main(string[] args)
    {
        var Ferry = new Ferry();
        Ferry.Solve1("dummydata").Should().Be(288);
        Console.WriteLine($"Answer to part 1 = {Ferry.Solve1("data")}");
        Ferry.Solve2("dummydata").Should().Be(71503);
        Console.WriteLine($"Answer to part 2 = {Ferry.Solve2("data")}");
    }
}

class Ferry
{
    private IEnumerable<string> _content;
    public int Solve1(string fileName)
    {
        _content = File.ReadLines($"Data/{fileName}");
        var times = GetData(0);
        var distances = GetData(1);
        var races = times.Zip(distances, (time, distance) => (time, distance)).ToList();

        return races.Aggregate(1, (current, race) => MultiplyScore(race.time, race.distance, current));
    }
    
    public int Solve2(string fileName)
    {
        _content = File.ReadLines($"Data/{fileName}");
        var time = GetMergedData(0);
        var distance = GetMergedData(1);
        
        var multipliedScore = 1;
        multipliedScore = MultiplyScore(time, distance, multipliedScore);

        return multipliedScore;
    }

    private List<int> GetData(int idx)
    {
        return _content.ElementAt(idx).Split(' ')
            .Where(part => int.TryParse(part, out _))
            .Select(int.Parse).ToList();
    }

    private long GetMergedData(int idx)
    {
        return long.Parse(_content.ElementAt(0)
            .Split(' ')
            .Where(part => long.TryParse(part, out _))
            .Aggregate("", (acc, num) => acc + num));
    }

    private static int MultiplyScore(long time, long distance, int multipliedScore)
    {
        var possibleWaysToWin = 0;
        for (var buttonTime = 0; buttonTime < time - 1; buttonTime++)
        {
            var velocity = buttonTime;
            var remainingTime = time - buttonTime;
            var travelledDistance = remainingTime * velocity;
            if (travelledDistance > distance) possibleWaysToWin++;
        }

        multipliedScore *= possibleWaysToWin;
        return multipliedScore;
    }
}