using System.Text.RegularExpressions;
using FluentAssertions;

namespace Fertilizer;

class Program
{
    static void Main(string[] args)
    {
        var fertilizer = new Fertilizer();
        fertilizer.Solve1("dummydata").Should().Be(35);
        fertilizer.Solve1("data").Should().Be(35);
    }
}

class Fertilizer
{
    private const string SeedsString = "seeds: ";
    private const string SeedsToSoilMap = "seed-to-soil";
    private const string SeedsToFertilizerMap = "soil-to-fertilizer";
    private const string FertilizerToWaterMap = "fertilizer-to-water";
    private const string WaterToLightMap = "water-to-light";
    private const string LightToTemperatureMap = "light-to-temperature";
    private const string TemperatureToHumidity = "temperature-to-humidity";
    private const string HumidityToLocationMap = "humidity-to-location";

    private string[] _content;
    
    public Int64 Solve1(string fileName)
    {
        _content = File.ReadAllLines($"Data/{fileName}");
        var seeds = _content
            .Where(line => line.TrimStart().StartsWith(SeedsString))
            .SelectMany(line => Regex.Matches(line, @"\d+").Select(match => Int64.Parse(match.Value)))
            .ToList();

        Dictionary<Int64, Int64> seedsDictionary = seeds.ToDictionary(num => num);
        seedsDictionary =  ResultAfterMap(seedsDictionary, SeedsToSoilMap);
        seedsDictionary =  ResultAfterMap(seedsDictionary, SeedsToFertilizerMap);
        seedsDictionary =  ResultAfterMap(seedsDictionary, FertilizerToWaterMap);
        seedsDictionary =  ResultAfterMap(seedsDictionary, WaterToLightMap);
        seedsDictionary =  ResultAfterMap(seedsDictionary, LightToTemperatureMap);
        seedsDictionary =  ResultAfterMap(seedsDictionary, TemperatureToHumidity);
        seedsDictionary =  ResultAfterMap(seedsDictionary, HumidityToLocationMap);

        return seedsDictionary.Min(x => x.Value);
    }
    
    private Dictionary<Int64, Int64> ResultAfterMap(Dictionary<Int64, Int64> seedsDictionary, string mapType)
    {
        var values = FindMap(mapType);
        foreach (var seed in seedsDictionary)
        {
            if (values.Any(val => seed.Value >= val.source && seed.Value <= val.source + val.range))
            {
                var destinationRange = values.First(tuple => seed.Value >= tuple.source && seed.Value <= tuple.source + tuple.range);
                var offset = seed.Value - destinationRange.source;

                seedsDictionary[seed.Key] = destinationRange.dest + offset;
            }
            else
            {
                seedsDictionary[seed.Key] = seed.Value;
            }
        }

        return seedsDictionary;
    }
    
    private List<(Int64 dest, Int64 source, Int64 range)> FindMap(string mapType)
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