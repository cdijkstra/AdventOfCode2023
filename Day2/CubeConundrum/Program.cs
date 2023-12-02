using System.Text.RegularExpressions;
using FluentAssertions;

namespace CubeConundrum;

class Program
{
    static void Main(string[] args)
    {
        var conundrum = new Conundrum();
        // conundrum.Solve("dummydata1").Should().Be(8);
        // Console.WriteLine(conundrum.Solve("data"));
        conundrum.Solve2("dummydata1").Should().Be(2286);
        Console.WriteLine(conundrum.Solve2("data"));
    }
}

class Conundrum
{
    private readonly int _maxRedCount = 12;
    private readonly int _maxGreenCount = 13;
    private readonly int _maxBlueCount = 14;
    private int _totalId = 0;
    private Int64 _totalPower = 0;
    enum RGB
    {
        red, green, blue
    }
    
    public int Solve(string fileName)
    {
        _totalId = 0;
        var games = File.ReadAllLines($"Data/{fileName}");
        var colorRegex = new Regex(@$"(\d+)\s({string.Join("|", Enum.GetNames(typeof(RGB)))})");
        var gameRegex = new Regex(@"\d+");
        foreach (var possibleGame in games)
        {
            var gameId = int.Parse(gameRegex.Matches(possibleGame)[0].Value);
            var configurations = possibleGame.Split(";");

            var conditionSatisfied = colorRegex.Matches(possibleGame)
                .Select(match => (Number: int.Parse(match.Value.Split()[0]), Color: match.Value.Split()[1]))
                .All(tuple => configurations.All(conf =>
                    (tuple.Color == "red" && tuple.Number <= _maxRedCount) ||
                    (tuple.Color == "blue" && tuple.Number <= _maxBlueCount) ||
                    (tuple.Color == "green" && tuple.Number <= _maxGreenCount)));

            if (conditionSatisfied)
            {
                _totalId += gameId;
            }
        } 
        return _totalId;
    }
    
    public Int64 Solve2(string fileName)
    {
        _totalPower = 0;
        var games = File.ReadAllLines($"Data/{fileName}");
        var colorRegex = new Regex(@$"(\d+)\s({string.Join("|", Enum.GetNames(typeof(RGB)))})");
        var gameRegex = new Regex(@"\d+");

        foreach (var possibleGame in games)
        {
            var maxRed = colorRegex.Matches(possibleGame)
                .Select(match => (Number: Int64.Parse(match.Value.Split()[0]), Color: match.Value.Split()[1]))
                .Where(tuple => tuple.Color == "red")
                .Select(x => x.Number)
                .Max();

            var maxGreen = colorRegex.Matches(possibleGame)
                .Select(match => (Number: Int64.Parse(match.Value.Split()[0]), Color: match.Value.Split()[1]))
                .Where(tuple => tuple.Color == "green")
                .Select(x => x.Number)
                .Max();

            var maxBlue = colorRegex.Matches(possibleGame)
                .Select(match => (Number: Int64.Parse(match.Value.Split()[0]), Color: match.Value.Split()[1]))
                .Where(tuple => tuple.Color == "blue")
                .Select(x => x.Number)
                .Max();

            _totalPower += maxRed * maxGreen * maxBlue;

        }

        return _totalPower;
    }
}
