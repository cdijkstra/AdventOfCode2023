using FluentAssertions;

namespace Galaxy;

class Program
{
    static void Main(string[] args)
    {
        var distanceCalculator = new DistanceCalculator();
        var galaxy = new Galaxy(distanceCalculator);
        galaxy.Solve1("dummydata").Should().Be(374);
        Console.WriteLine(galaxy.Solve1("data"));
    }
}

class Galaxy
{
    private DistanceCalculator _distanceCalculator;
    public Galaxy(DistanceCalculator distanceCalculator)
    {
        _distanceCalculator = distanceCalculator;
    }

    private List<List<char>> _grid = new();
    public int Solve1(string fileName)
    {
        _grid = File.ReadAllLines($"Data/{fileName}")
            .Select(line => line.ToList())
            .ToList();
        
        ExpandGalaxy();
        foreach (var row in _grid)
        {
            row.ForEach(x => Console.Write(x));
            Console.WriteLine();
        }

        var locations = FindGalaxyLocations();
        var distances = _distanceCalculator.CalculateAllDistances(locations);

        return distances.Cast<int>().Sum();
    }

    private void ExpandGalaxy()
    {
        var rowCount = _grid.Count;
        var colCount = _grid[0].Count;
        
        for (int i = 0; i < rowCount; i++)
        {
            if (_grid[i].All(cell => cell == '.'))
            {
                _grid.Insert(i, new List<char>(new string('.', colCount).ToCharArray()));
                i++; // Skip the newly added row
                rowCount++;
            }
        }

        // Check and add additional columns
        for (int j = 0; j < colCount; j++)
        {
            if (_grid.All(row => row[j] == '.'))
            {
                for (int i = 0; i < rowCount; i++)
                {
                    _grid[i].Insert(j, '.');
                }

                j++; // Skip the newly added column
                colCount++;
            }
        }
    }

    private List<Location> FindGalaxyLocations()
    {
        return _grid
            .SelectMany((row, rowIndex) =>
                row.Select((character, colIndex) => (cell: character, rowIndex, colIndex))
            )
            .Where(coord => coord.cell == '#')
            .Select(coord => new Location(coord.rowIndex, coord.colIndex))
            .ToList();
    }
}

public class Location(int x, int y)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
}

public class DistanceCalculator
{
    public int CalculateDistance(Location location1, Location location2)
    {
        int deltaX = location1.X - location2.X;
        int deltaY = location1.Y - location2.Y;

        return Math.Abs(deltaX) + Math.Abs(deltaY);
    }

    public int[,] CalculateAllDistances(List<Location> locations)
    {
        int n = locations.Count;
        int[,] distances = new int[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                distances[i, j] = CalculateDistance(locations[i], locations[j]); }
        }

        return distances;
    }
}