using System.Text.RegularExpressions;
using FluentAssertions;

namespace Wasteland;

class Program
{
    static void Main(string[] args)
    {
        var wasteLand = new Wasteland();
        wasteLand.Solve1("dummydata").Should().Be(2);
        Console.WriteLine(wasteLand.Solve1("data"));
        wasteLand.Solve2("dummydata2").Should().Be(6);
        Console.WriteLine(wasteLand.Solve2("data"));
    }
}

class Network
{
    public string Origin { get; set; }
    public List<string> Destinations = new();
}

enum Direction { L, R }

class Wasteland
{
    private static readonly string Start = "AAA";
    private static readonly string Exit = "ZZZ";

    private char[] _choices;
    private List<Network> _networks = new();
    private void Initialize(string fileName)
    {
        var allLines = File.ReadAllLines($"Data/{fileName}");
        _choices = allLines[0].ToCharArray();
        _networks = allLines.Skip(2).Select(line =>
        {
            var parts = line.Split("=");
            var pattern = @"\((.*?)\)";
            return new Network()
            {
                Origin = parts[0].Trim(),
                Destinations = Regex.Match(line, pattern).Groups[1].Value
                    .Split(',')
                    .Select(dest => dest.Trim())
                    .ToList()
            };
        }).ToList();
    }
    
    public int Solve1(string fileName)
    {
        Initialize(fileName);
        
        bool exitFound = false;
        var currentNetwork = _networks.Single(nw => nw.Origin == Start);
        var idx = 0;
        while (!exitFound)
        {
            var direction = (Direction) Enum.Parse(typeof(Direction), _choices[idx % _choices.Length].ToString());
            currentNetwork = direction switch
            {
                Direction.L => _networks.Single(nw => nw.Origin == currentNetwork.Destinations[0]),
                Direction.R => _networks.Single(nw => nw.Origin == currentNetwork.Destinations[1]),
                _ => throw new Exception("Unknown instruction")
            };

            exitFound = currentNetwork.Origin == Exit;
            if (!exitFound)
            {
                idx++;
            }
        }
        
        return idx + 1;
    }
    
    public long Solve2(string fileName)
    {
        Initialize(fileName);
        
        bool allEndingOnLetterZ = false;
        var currentNetworks = _networks.Where(nw => nw.Origin.EndsWith("A")).ToList();
        long idx = 0;
        while (!allEndingOnLetterZ)
        {
            var direction = (Direction) Enum.Parse(typeof(Direction), _choices[idx % _choices.Length].ToString());
            var leftOrRightIndex = direction switch
            {
                Direction.L => 0,
                Direction.R => 1,
                _ => throw new Exception("Unknown instruction")
            };
            
            currentNetworks = currentNetworks
                .Select(network => _networks.Single(nw => nw.Origin == network.Destinations[leftOrRightIndex]))
                .ToList();
            
            allEndingOnLetterZ = currentNetworks.All(nw => nw.Origin.EndsWith("Z"));
            if (!allEndingOnLetterZ)
            {
                idx++;
            }
        }
        
        return idx + 1;
    }
}
