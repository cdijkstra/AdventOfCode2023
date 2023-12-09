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
        var considerNetworks = _networks.Where(nw => nw.Origin.EndsWith("A")).ToList();
        var firstNetwork = considerNetworks.First();
        
        List<long> indicesToConsider = new();
        long idx = 0;
        long startIdx = 0;
        
        while (!allEndingOnLetterZ)
        {
            // Find 100 matching numbers for the first network
            var totalIndicesToConsider = 2;
            while (indicesToConsider.Count < totalIndicesToConsider + 1)
            {
                var direction = (Direction) Enum.Parse(typeof(Direction), _choices[idx % _choices.Length].ToString());
                firstNetwork = direction switch
                {
                    Direction.L => _networks.Single(nw => nw.Origin == firstNetwork.Destinations[0]),
                    Direction.R => _networks.Single(nw => nw.Origin == firstNetwork.Destinations[1]),
                    _ => throw new Exception("Unknown instruction")
                };
                
                idx++;
                if (firstNetwork.Origin.EndsWith("Z")) indicesToConsider.Add(idx);
            }

            // Check for second network if they end with Z as well for those indices.
            // Update indicesToConsider (remove ones not matching for network 2)
            // Then check for network 3, update indices and check for network 4
            var networkIndex = 1;
            var newStartIdx = indicesToConsider.Max();
            while (networkIndex < considerNetworks.Count && indicesToConsider.Any())
            {
                var currentNetwork = considerNetworks[networkIndex];
                List<long> remainingIndicesToConsider = new();
                for (var currentIdx = startIdx; currentIdx < indicesToConsider.Max(); currentIdx++)
                {
                    Direction direction = (Direction)Enum.Parse(typeof(Direction), _choices[currentIdx % _choices.Length].ToString());
                    currentNetwork = direction switch
                    {
                        Direction.L => _networks.Single(nw => nw.Origin == currentNetwork.Destinations[0]),
                        Direction.R => _networks.Single(nw => nw.Origin == currentNetwork.Destinations[1]),
                        _ => throw new Exception("Unknown instruction")
                    };

                    if (startIdx == indicesToConsider.Max())
                    {
                        
                    }
                    
                    if (indicesToConsider.Contains(currentIdx + 1) && currentNetwork.Origin.EndsWith("Z"))
                    {
                        remainingIndicesToConsider.Add(currentIdx + 1);
                    }
                }

                indicesToConsider = remainingIndicesToConsider;
                networkIndex++; // Break out when still remaining index and networkIndex = _networks.Count
            }

            startIdx = newStartIdx;

            if (indicesToConsider.Any())
            {
                allEndingOnLetterZ = true;
            }
        }

        return indicesToConsider.Min();
    }
}
