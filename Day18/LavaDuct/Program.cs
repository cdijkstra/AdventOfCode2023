using FluentAssertions;

namespace LavaDuct;

class Program
{
    static void Main(string[] args)
    {
        var lavaduct = new Lavaduct();
        lavaduct.Solve1("dummydata").Should().Be(62);
        Console.WriteLine(lavaduct.Solve1("data"));

        List<(int x, int y)> locations = new()
        {
            (1, 1), (4, 1), (4, 5), (1, 1)
        };
        lavaduct.TestShoelaceFormula(locations).Should().Be(6);
        
        List<(int x, int y)> locations2 = new()
        {
            (1,1),(5,1),(5,3),(1,3),(1,1)
        };
        lavaduct.TestShoelaceFormula(locations2).Should().Be(8);        
        
        List<(int x, int y)> locations3 = new()
        {
            (2,1),(4,1),(5,3),(4,5),(2,5),(1,3),(2,1)
        };
        lavaduct.TestShoelaceFormula(locations3).Should().Be(12);
        
        lavaduct.Solve2("dummydata").Should().Be(952408144115);
    }
}

enum Direction
{
    R = 0, D = 1, L = 2, U = 3
}


class Lavaduct
{
    private List<List<char>> _grid = new();

    private List<List<char>> _subgrid = new();

    private List<(int x, int y)> _floodLocations = new();

    public long TestShoelaceFormula(List<(int x, int y)> locations)
    {
        return ShoelaceFormula(locations);
    }
    
    public int Solve1(string fileName)
    {
        _grid = Enumerable.Range(0, 600)
            .Select(_ => Enumerable.Repeat('.', 600).ToList())
            .ToList();
        _subgrid = new();
        
        List<(int x, int y)> locations = new();
        (int x, int y) location = (300, 300);
        locations.Add(location);
        _grid[location.x][location.y] = '#';
        
        foreach (var line in File.ReadAllLines($"Data/{fileName}"))
        {
            var dir = (Direction) Enum.Parse(typeof(Direction), line.Split()[0]);
            var steps = int.Parse(line.Split()[1]);
            switch (dir)
            {
                case Direction.L:
                    foreach (var step in Enumerable.Range(1, steps))
                    {
                        _grid[location.x][location.y - step] = '#';
                        locations.Add((location.x, location.y - step));
                    }
                    location.y -= steps;
                    break;
                case Direction.R:
                    foreach (var step in Enumerable.Range(1, steps))
                    {
                        _grid[location.x][location.y + step] = '#';
                        locations.Add((location.x, location.y + step));

                    }
                    location.y += steps;
                    break;
                case Direction.U:
                    foreach (var step in Enumerable.Range(1, steps))
                    {
                        _grid[location.x - step][location.y] = '#';
                        locations.Add((location.x - step, location.y));
                    }
                    location.x -= steps;
                    break;
                case Direction.D:
                    foreach (var step in Enumerable.Range(1, steps))
                    {
                        _grid[location.x + step][location.y] = '#';
                        locations.Add((location.x + step, location.y));
                    }
                    location.x += steps;
                    break;
            }
        }
        
        // Find min,max row and column in pipeCoordinates
        var minRow = locations.Min(loc => loc.x);
        var maxRow = locations.Max(loc => loc.x);
        var minCol = locations.Min(loc => loc.y);
        var maxCol = locations.Max(loc => loc.y);
        
        _subgrid = _grid
            .Skip(minRow)
            .Take(maxRow - minRow + 1)
            .Select(row => row.Skip(minCol).Take(maxCol - minCol + 1).ToList())
            .ToList();

        _subgrid.Insert(0, new List<char>(Enumerable.Repeat('.', _subgrid[0].Count)));
        _subgrid.Add(new List<char>(Enumerable.Repeat('.', _subgrid[0].Count)));
        _subgrid.ForEach(row =>
        {
            row.Insert(0, '.');
            row.Add('.');
        });
        FloodCoordinateIteratively(0, 0);
        _floodLocations.ForEach(loc => _subgrid[loc.x][loc.y] = '0');
        
        foreach (var row in _subgrid)
        {
            row.ForEach(x => Console.Write(x));
            Console.WriteLine();
        }
        
        return _subgrid.Sum(row => row.Count(c => c is '#' or '.'));
    }
    
    public long Solve2(string fileName)
    {
        List<(int x, int y)> locations = new() { (1, 1) };
        // Shoelace algorithm
        foreach (var line in File.ReadAllLines($"Data/{fileName}"))
        {
            var hex = line.Split()[2].Trim('(', '#').TrimEnd(')');
            var steps = int.Parse(hex.Substring(0, 5), System.Globalization.NumberStyles.HexNumber);
            var direction = (Direction)int.Parse(hex.Substring(5));
            
            var lastLocationEntry = locations.Last();
            switch (direction)
            {
                case Direction.U:
                    locations.Add(((lastLocationEntry.x), lastLocationEntry.y - steps));
                    break;
                case Direction.D:
                    locations.Add(((lastLocationEntry.x), lastLocationEntry.y + steps));
                    break;
                case Direction.L:
                    locations.Add(((lastLocationEntry.x - steps), lastLocationEntry.y));
                    break;
                case Direction.R:
                    locations.Add(((lastLocationEntry.x + steps), lastLocationEntry.y));
                    break;
            }
        }

        return ShoelaceFormula(locations);
    }

    private long ShoelaceFormula(List<(int x, int y)> locations)
    {
        long sum = 0;
        for (var i = 0; i != locations.Count - 1; i++)
        {
            sum += locations[i].x * locations[i + 1].y;
            sum -= locations[i].y * locations[i + 1].x;
        }

        return Math.Abs(sum) / 2;
    }

    void FloodCoordinateIteratively(int startRow, int startCol)
    {
        int rows = _subgrid.Count;
        int cols = _subgrid[0].Count;

        Queue<(int, int)> queue = new Queue<(int, int)>();
        queue.Enqueue((startRow, startCol));

        while (queue.Count > 0)
        {
            var (row, col) = queue.Dequeue();

            if (_floodLocations.Contains((row, col)) || row < 0 || row > _subgrid.Count - 1 || col < 0 || col > _subgrid[0].Count - 1 || _subgrid[row][col] == '#')
            {
                continue;
            }

            _floodLocations.Add((row, col));

            // Enqueue adjacent cells
            queue.Enqueue((row - 1, col)); // Up
            queue.Enqueue((row + 1, col)); // Down
            queue.Enqueue((row, col - 1)); // Left
            queue.Enqueue((row, col + 1)); // Right
        }
    }
}
