using System.Text.RegularExpressions;
using FluentAssertions;

namespace Fertilizer;

class Program
{
    static void Main(string[] args)
    {
        var fertilizer = new Fertilizer();
        fertilizer.Solve1("dummydata").Should().Be(35);
        Console.WriteLine($"Answer to part 1 = {fertilizer.Solve1("data")}");
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

    private long _newLeft;
    private string[] _content;
    
    public Int64 Solve1(string fileName)
    {
        _content = File.ReadAllLines($"Data/{fileName}");
        var seedsDictionary = _content
            .Where(line => line.TrimStart().StartsWith(SeedsString))
            .SelectMany(line => Regex.Matches(line, @"\d+").Select(match => Int64.Parse(match.Value)))
            .ToDictionary(num => num);
        
        foreach (var mapType in new List<string>
                 {
                     SeedsToSoilMap,
                     SoilToFertilizerMap,
                     FertilizerToWaterMap,
                     WaterToLightMap,
                     LightToTemperatureMap,
                     TemperatureToHumidity,
                     HumidityToLocationMap
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
                    .Select(match => Int64.Parse(match.Value)).ToList();

                return Enumerable.Range(0, seedParts.Count / 2)
                    .Select(index => (
                        left: seedParts[index * 2],
                        right: seedParts[index * 2] + seedParts[index * 2 + 1] - 1
                        )); 
            }).ToList();
        
        foreach (var mapType in new List<string>
             {
                 SeedsToSoilMap,
                 SoilToFertilizerMap,
                 FertilizerToWaterMap,
                 WaterToLightMap,
                 LightToTemperatureMap,
                 TemperatureToHumidity,
                 HumidityToLocationMap
             })
        {
            var updatedMap = new List<(long left, long right)>();
            // Loop over all seedMaps using intervals 
            foreach (var seedMap in seedsMap.OrderBy(x => x.left))
            {
                var maps = FindMap(mapType);
                var considerMaps = maps.Where(map => Overlaps(map, seedMap)).ToList();
                // If maps should apply, do so. Otherwise add the seedmap unchanged in else statement
                if (considerMaps.Any())
                {
                    ApplyMaps(seedMap, considerMaps, updatedMap);
                }
                else
                {
                    updatedMap.Add(seedMap);
                    _newLeft = seedMap.right + 1;
                }
            }
            
            seedsMap = updatedMap;
        }
        
        return seedsMap.Where(x => x.left != 0).Min(result => result.left);
    }

    private void ApplyMaps((long left, long right) seedMap, IEnumerable<(long dest, long source, long range)> considerMaps, List<(long left, long right)> updatedMap)
    {
        _newLeft = seedMap.left;
        var considerMapsOrdered = considerMaps.OrderBy(x => x.source);
        foreach (var mapToApply in considerMapsOrdered)
        {
            // The maps are sorted. We should check if there's an interval from _newleft to source
            var applyDiff = mapToApply.dest - mapToApply.source;
            if (_newLeft < mapToApply.source)
            {
                updatedMap.Add((_newLeft, mapToApply.source - 1));
                _newLeft = mapToApply.source;
            }

            // We should check if the right interval of the map is within or outside the range of the seeds
            var applyMapUntil = seedMap.right < mapToApply.source + mapToApply.range
                ? seedMap.right
                : mapToApply.source + mapToApply.range - 1;

            updatedMap.Add((_newLeft + applyDiff, applyMapUntil + applyDiff));
            _newLeft = applyMapUntil + 1;
        }
        
        // Maybe we missed a part at the right side
        if (_newLeft < seedMap.right + 1)
        {
            updatedMap.Add((_newLeft, seedMap.right));
        }
    }

    static bool Overlaps((long destination, long source, long range) map, (long left, long right) seedmap)
    {
        var mapStart = map.source;
        var mapEnd = map.source + map.range;
        var seedmapStart = seedmap.left;
        var seedmapEnd = seedmap.right;

        // Check for overlap
        return (mapStart >= seedmapStart && mapStart <= seedmapEnd) || // start of map is within seed range
               (mapEnd >= seedmapStart && mapEnd <= seedmapEnd) ||     // end of map is within seed range
               (mapStart <= seedmapStart && mapEnd >= seedmapEnd);      // map fully contains seed range
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