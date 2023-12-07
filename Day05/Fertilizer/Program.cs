using System.Text.RegularExpressions;
using System.Collections.Generic;
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

        List<(long left, long right)> seedsMap = _content
            .Where(line => line.TrimStart().StartsWith(SeedsString))
            .SelectMany(line =>
            {
                var seedParts = Regex.Matches(line, @"\d+")
                    .Select(match => Int64.Parse(match.Value))
                    .ToList();

                return Enumerable.Range(0, seedParts.Count / 2)
                    .Select(index => (
                        left: seedParts[index * 2],
                        right: seedParts[index * 2] + seedParts[index * 2 + 1] - 1
                        )); 
            }).ToList();
        
        foreach (var mapType in  new List<string>
                 {
                     SeedsToSoilMap, SoilToFertilizerMap, FertilizerToWaterMap, WaterToLightMap, LightToTemperatureMap,
                     TemperatureToHumidity, HumidityToLocationMap
                 })
        {
            var updatedMap = new List<(long left, long right)>();
            foreach (var seedMap in seedsMap)
            {
                var map = FindMap(mapType);
                var considerMaps = map.Where(mp =>
                    (seedMap.left <= mp.dest + mp.range && seedMap.right >= mp.dest) ||
                    (mp.dest <= seedMap.right && mp.dest + mp.range >= seedMap.left));
                if (considerMaps.Any())
                {
                    var coveringMapExists = map.Any(mp =>
                        seedMap.left >= mp.source && seedMap.right <= mp.source + mp.range - 1
                    );
                    if (coveringMapExists)
                    {
                        var covermap = map.Single(mp => seedMap.left >= mp.source && seedMap.right <= mp.source + mp.range - 1);
                        var mappingDif = covermap.dest - covermap.source;
                        var updatedSeedMap = (seedMap.left + mappingDif, seedMap.right + mappingDif);
                        updatedMap.Add(updatedSeedMap);
                    }
                    // Check for partial coverage at LHS
                    else if (map.Any(mp => seedMap.left > mp.source && seedMap.left < mp.source + mp.range - 1))

                    {
                        // Edge case... Only right entry other mapping....
                        
                        var leftCoverMap = map.Single(mp => seedMap.left > mp.source && seedMap.left < mp.source + mp.range - 1);
                        var mappingDif = leftCoverMap.dest - leftCoverMap.source;
                        var newSeedmap = (seedMap.left + mappingDif, leftCoverMap.source + leftCoverMap.range - 1 + mappingDif);
                        var seedmapUpdatedInterval = (leftCoverMap.source + leftCoverMap.range, seedMap.right);
                        updatedMap.Add(seedmapUpdatedInterval);
                        updatedMap.Add(newSeedmap);
                    }
                    // Check for partial coverage at RHS
                    else if (map.Any(mp => seedMap.left < mp.source && seedMap.right> mp.source))

                    {
                        if (map.Count(mp =>
                                seedMap.left < mp.source &&
                                seedMap.right > mp.source) == 1)
                        {
                            var rightCoverMap = map.Single(mp => seedMap.left < mp.source && seedMap.right >= mp.source);
                            var seedmapUpdatedInterval = (seedMap.left, rightCoverMap.source - 1);
                            var mappingDif = rightCoverMap.dest - rightCoverMap.source;
                            var newSeedmap = (rightCoverMap.source + mappingDif, seedMap.right + mappingDif);
                            updatedMap.Add(seedmapUpdatedInterval);
                            updatedMap.Add(newSeedmap);
                        }
                        else
                        {
                            Console.WriteLine("Hi");
                        }
                    }
                    else if (map.Any(mp => seedMap.left > mp.source && seedMap.right < mp.source + mp.range - 1))

                    {
                        var partialCover = map.Single(mp => seedMap.left > mp.source && seedMap.right < mp.source + mp.range - 1);
                        var seedmapUpdatedIntervalLeft = (seedMap.left, partialCover.source - 1);
                        var mappingDif = partialCover.dest - partialCover.source;
                        var newSeedmap = (partialCover.source + mappingDif, partialCover.source + partialCover.range - 1 + mappingDif);
                        var seedmapUpdatedIntervalRight = (partialCover.source + partialCover.range, seedMap.right);
                        updatedMap.Add(seedmapUpdatedIntervalLeft);
                        updatedMap.Add(newSeedmap);
                        updatedMap.Add(seedmapUpdatedIntervalRight);
                    }
                }
                else
                {
                    updatedMap.Add(seedMap);
                }
            }
            seedsMap = updatedMap;
            Console.WriteLine();
        }
        
        Console.WriteLine();
        return seedsMap.Min(result => result.left);
        // Lower than 4476894655
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