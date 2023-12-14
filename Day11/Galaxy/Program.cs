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
        galaxy.Solve2("dummydata", 10).Should().Be(1030);
        galaxy.Solve2("dummydata", 100).Should().Be(8410);
        Console.WriteLine(galaxy.Solve2("data", 1000000));

    }
}

class Galaxy(DistanceCalculator distanceCalculator)
{
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
        var distances = distanceCalculator.CalculateAllDistances(locations);

        return distances.Cast<int>().Sum();
    }
    
    public long Solve2(string fileName, long expansionFactor)
    {
        _grid = File.ReadAllLines($"Data/{fileName}")
            .Select(line => line.ToList())
            .ToList();
        DenoteHugeExpansionsInGalaxy();
        
        distanceCalculator.SetGrid(_grid);
        distanceCalculator.SetExpansionFactor(expansionFactor);
        
        ExpandGalaxy();
        foreach (var row in _grid)
        {
            row.ForEach(x => Console.Write(x));
            Console.WriteLine();
        }

        var locations = FindGalaxyLocations();
        var distances = distanceCalculator.CalculateAllHugeDistances(locations);

        return distances.Cast<long>().Sum();
    }

    private void DenoteHugeExpansionsInGalaxy()
    {
        var rowCount = _grid.Count;
        var colCount = _grid[0].Count;
        
        // Check and replace empty rows
        for (int row = 0; row < rowCount; row++)
        {
            if (_grid[row].All(c => c == '.'))
            {
                for (int col = 0; col < colCount; col++)
                {
                    _grid[row][col] = '*';
                }
            }
        }

        // Check and replace empty columns
        for (int col = 0; col < colCount; col++)
        {
            if (_grid.All(row => row[col] == '.' || row[col] == '*'))
            {
                for (int row = 0; row < rowCount; row++)
                {
                    _grid[row][col] = '*';
                }
            }
        }
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
        for (var j = 0; j < colCount; j++)
        {
            if (_grid.All(row => row[j] == '.'))
            {
                for (var i = 0; i < rowCount; i++)
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
    private long ExpansionFactor;
    private List<List<char>> _grid;

    public void SetExpansionFactor(long expansionFactor)
    {
        ExpansionFactor = expansionFactor - 1;
    }
    public void SetGrid(List<List<char>> grid)
    {
        _grid = grid;
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
    
    public int CalculateDistance(Location location1, Location location2)
    {
        int deltaX = location1.X - location2.X;
        int deltaY = location1.Y - location2.Y;

        return Math.Abs(deltaX) + Math.Abs(deltaY);
    }
    
    public long[,] CalculateAllHugeDistances(List<Location> locations)
    {
        int n = locations.Count;
        long[,] distances = new long[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                distances[i, j] = CalculateDistanceWithHugeExpansion(locations[i], locations[j]); }
        }

        return distances;
    }
    
    private long CalculateDistanceWithHugeExpansion(Location location1, Location location2)
    {
        long deltaX = location1.X - location2.X;
        long deltaY = location1.Y - location2.Y;

        var amountHugeExpansions = FindAmountExpansions(location1, location2);
        
        return Math.Abs(deltaX) + Math.Abs(deltaY) + amountHugeExpansions * ExpansionFactor;
    }
    
    private long FindAmountExpansions(Location location1, Location location2)
    {
        var amountOfExpansion = 0;
        // When going from location1 to location2, count how many '*' are crossed and multiply by factor
        if (location1.X > location2.X)
        {
            for (var deltaX = location1.X; deltaX >= location2.X; deltaX--)
            {
                if (_grid[deltaX][location1.Y] != '*') continue;
                amountOfExpansion++;
            }
        }
        if (location1.X < location2.X)
        {
            for (var deltaX = location1.X; deltaX <= location2.X; deltaX++)
            {
                if (_grid[deltaX][location1.Y] != '*') continue;
                amountOfExpansion++;
            }
        }
        
        if (location1.Y > location2.Y)
        {
            for (var deltaY = location1.Y; deltaY >= location2.Y; deltaY--)
            {
                if (_grid[location2.X][deltaY] != '*') continue;
                amountOfExpansion++;
            }
        }
        if (location1.Y < location2.Y)
        {
            for (var deltaY = location1.Y; deltaY <= location2.Y; deltaY++)
            {
                if (_grid[location2.X][deltaY] != '*') continue;
                amountOfExpansion++;
            }
        }

        return amountOfExpansion;
    }
}