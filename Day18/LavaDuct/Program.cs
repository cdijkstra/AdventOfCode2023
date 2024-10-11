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
        List<(int x, int y)> locations = new() { (1, 1) };
        // Shoelace algorithm
        foreach (var line in File.ReadAllLines($"Data/{fileName}"))
        {
            var hex = line.Split()[2].Trim('(', '#').TrimEnd(')');
            var distance = int.Parse(hex.Substring(0, 5), System.Globalization.NumberStyles.HexNumber);
            var direction = (Direction)int.Parse(hex.Substring(5));
            
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

        return ShoelaceFormula(locations);
    }

    private long ShoelaceFormula(List<(int x, int y)> locations)
    {
        long interiorArea = 0;
        long boundaryPoints = 0;
        for (var i = 0; i != locations.Count - 1; i++)
        {
            interiorArea += Math.Abs(locations[i].x * locations[i + 1].y - locations[i].y * locations[i + 1].x);
            // if (locations[i + 1].x == locations[i].x) // Move up or down
            // {
            //     boundaryPoints += Math.Abs(locations[i + 1].y + locations[i].y) + 1;
            // }
            // else if (locations[i + 1].y == locations[i].y) // Move up or down
            // {
            //     boundaryPoints += Math.Abs(locations[i + 1].x + locations[i].x) + 1;
            // }
        }

        interiorArea = Math.Abs(interiorArea) / 2;
        
        // long interiorPoints = interiorArea - (boundaryPoints / 2) + 1;
        // long totalArea = interiorPoints + (boundaryPoints / 2) - 1;

        return interiorArea;
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
