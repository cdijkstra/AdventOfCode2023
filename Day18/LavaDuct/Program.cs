using FluentAssertions;
namespace LavaDuct;

class Program
{
    static void Main(string[] args)
    {
        var lavaduct = new Lavaduct();
        lavaduct.Solve1("dummydata").Should().Be(62);
        Console.WriteLine(lavaduct.Solve1("data"));
        lavaduct.Solve2("dummydata").Should().Be(952408144115);
        Console.WriteLine(lavaduct.Solve2("data"));
    }
}

class Lavaduct
{
    private List<List<char>> _grid = new();
    private List<List<char>> _subgrid = new();
    private List<(int x, int y)> _floodLocations = new();
    private enum Direction
    {
        R = 0, D = 1, L = 2, U = 3
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
        var minRow = locations.Min(loc => loc.x); var maxRow = locations.Max(loc => loc.x);
        var minCol = locations.Min(loc => loc.y); var maxCol = locations.Max(loc => loc.y);
        
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
        List<(long x, long y)> locations = new() { (0,0) };
        long totalDistance = 0;
        // Shoelace algorithm
        foreach (var line in File.ReadAllLines($"Data/{fileName}"))
        {
            var hex = line.Split()[2].Trim('(', '#').TrimEnd(')');
            var distance = long.Parse(hex.Substring(0, 5), System.Globalization.NumberStyles.HexNumber);
            var direction = (Direction)long.Parse(hex.Substring(5));
            totalDistance += distance;
            
            var lastLocationEntry = locations.Last();
            switch (direction)
            {
                case Direction.U:
                    locations.Add(((lastLocationEntry.x), lastLocationEntry.y - distance));
                    break;
                case Direction.D:
                    locations.Add(((lastLocationEntry.x), lastLocationEntry.y + distance));
                    break;
                case Direction.L:
                    locations.Add(((lastLocationEntry.x - distance), lastLocationEntry.y));
                    break;
                case Direction.R:
                    locations.Add(((lastLocationEntry.x + distance), lastLocationEntry.y));
                    break;
            }
        }

        return ShoelaceFormula(locations, totalDistance);
    }

    private long ShoelaceFormula(List<(long x, long y)> vertices, long totalDistance)
    {
        var amountOfVertices = vertices.Count;
        // Shoelace theorem area calculation
        var area = Math.Abs(vertices.Take(amountOfVertices - 1)
            .Select((vertex, i) => vertex.x * vertices[i + 1].y - vertex.y * vertices[i + 1].x)
            .Sum()) / 2;
        
        // Calculate the boundary points using GCD
        var boundaryPoints = vertices.Take(vertices.Count - 1)
            .Select((vertex, i) => GCD(Math.Abs(vertices[i + 1].x - vertex.x), Math.Abs(vertices[i + 1].y - vertex.y)))
            .Sum();

        // Apply Pick's theorem
        var totalArea = area + boundaryPoints / 2 + 1;
        
        return totalArea;
    }

    // Function to compute the GCD of two numbers
    private long GCD(long a, long b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }
        return Math.Abs(a);
    }
    
    void FloodCoordinateIteratively(int startRow, int startCol)
    {
        var queue = new Queue<(int, int)>();
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
