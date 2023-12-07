using System.Text.RegularExpressions;
using System.Xml.Xsl;
using FluentAssertions;

namespace Fertilizer;

class Program
{
    static void Main(string[] args)
    {
        var fertilizer = new Fertilizer();
        // fertilizer.Solve1("dummydata").Should().Be(35);
        // Console.WriteLine($"Answer to part 1 = {fertilizer.Solve1("data")}");
        fertilizer.Solve2("dummydata").Should().Be(46);
        Console.WriteLine($"Answer to part 2 = {fertilizer.Solve2("data")}");
    }
}

class Fertilizer
{
    private const string SeedsString = "seeds: ";
    private const string SeedsToSoilMap = "seed-to-soil";
    private const string SoilToFertilizerMap = "soil-to-fertilizer";
    private const string FertilizerToWaterMap = "fertilizer-to-water";
    private const string WaterToLightMap = "water-to-light";
    private const string LightToTemperatureMap = "light-to-temperature";
    private const string TemperatureToHumidity = "temperature-to-humidity";
    private const string HumidityToLocationMap = "humidity-to-location";

    private string[] _content;
    
    public Int64 Solve1(string fileName)
    {
        _content = File.ReadAllLines($"Data/{fileName}");
        var seedsDictionary = _content
            .Where(line => line.TrimStart().StartsWith(SeedsString))
            .SelectMany(line => Regex.Matches(line, @"\d+").Select(match => Int64.Parse(match.Value)))
            .ToDictionary(num => num);
        
        foreach (var mapType in  new List<string>
                 {
                     SeedsToSoilMap, SoilToFertilizerMap, FertilizerToWaterMap, WaterToLightMap, LightToTemperatureMap,
                     TemperatureToHumidity, HumidityToLocationMap
                 })
        {
            var map = FindMap(mapType);
            foreach (var seed in seedsDictionary)
            {
                if (map.Any(tuple => seed.Value >= tuple.dest && seed.Value <= tuple.dest + tuple.range))
                {
                    var destinationRange = map.First(tuple => seed.Value >= tuple.source && seed.Value <= tuple.source + tuple.range);
                    var offset = seed.Value - destinationRange.source;

                    seedsDictionary[seed.Key] = destinationRange.dest + offset;
                }
            }
        }

        return seedsDictionary.Min(x => x.Value);
    }
    
    public Int64 Solve2(string fileName)
    {
        _content = File.ReadAllLines($"Data/{fileName}");
        var seedRanges = _content
            .Where(line => line.TrimStart().StartsWith(SeedsString))
            .SelectMany(line =>
            {
                var seedParts = Regex.Matches(line, @"\d+")
                    .Select(match => Int64.Parse(match.Value))
                    .ToList();
                
                return Enumerable.Range(0, seedParts.Count / 2)
                    .Select(index => (left: seedParts[index * 2], right: seedParts[index * 2 + 1]));
            })
            .ToList();
        
        foreach (var mapType in  new List<string>
                 {
                     SeedsToSoilMap, SoilToFertilizerMap, FertilizerToWaterMap, WaterToLightMap, LightToTemperatureMap,
                     TemperatureToHumidity, HumidityToLocationMap
                 })
        {
        }

        return 1;
    }
    
    static List<(long left, long right)> SplitTuple((long left, long right) seedTuple, List<(long dest, long source, long range)> mappings)
    {
        var result = new List<(long left, long right)>();

        var currentLeft = seedTuple.left;
        var currentRight = seedTuple.right;

        foreach (var mapping in mappings.OrderBy(t => t.source))
        {
            if (currentLeft >= mapping.dest || currentRight <= mapping.dest + mapping.range)
            {
                result.Add((currentLeft, mapping.source));
            }

            currentLeft = Math.Max(currentLeft, mapping.source);
        }

        return result;
    }
    
    private List<(long dest, long source, long range)> FindMap(string mapType)
    {
        var seedToSoilMap = _content
            .SkipWhile(line => !line.TrimStart().StartsWith($"{mapType} map:"))
            .Skip(1) // Skip the line containing "seed-to-soil map:"
            .TakeWhile(line => !string.IsNullOrWhiteSpace(line))
            .Select(values =>
            {
                var parts = values.Split();
                return (
                    dest: Int64.Parse(parts[0]),
                    source: Int64.Parse(parts[1]),
                    range: Int64.Parse(parts[2])
                );
            }).ToList();

        return seedToSoilMap;
    }
}