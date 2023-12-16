using FluentAssertions;

namespace Lava;

class Program
{
    static void Main(string[] args)
    {
        var lava = new Lava();
        lava.Solve1("dummydata").Should().Be(46);
        Console.WriteLine(lava.Solve1("data"));
    }
}

enum Direction
{
    N,E,S,W, // North, East, South, West
    Unknown // when it still has to be computed
}

class Lava
{
    private List<List<char>> _grid;
    private List<List<char>> _energizedGrid;
    private List<List<List<Direction>>> _directionGrid;

    private List<char> _mirrors = new()
    {
        '/', '\\'
    };
    
    private List<char> _splitters = new()
    {
        '|', '-'
    };
    public int Solve1(string fileName)
    {
        _grid = File.ReadAllLines($"Data/{fileName}")
            .Select(line => line.ToList())
            .ToList();

        // Initialize energizedGrid
        int rows = _grid.Count;
        int cols = _grid[0].Count;

        // Initialize _energizedGrid with the same dimensions and fill it with '.'
        _energizedGrid = Enumerable.Range(0, rows)
            .Select(_ => Enumerable.Repeat('.', cols).ToList())
            .ToList();
        
        _directionGrid = Enumerable.Range(0, rows)
            .Select(_ => Enumerable.Range(0, cols)
                .Select(__ => new List<Direction> { Direction.Unknown })
                .ToList())
            .ToList();
        
        // Determine start position and direction
        (int row, int col) startIndex = (0, 0);
        var startDirection = Direction.E;
        
        // Fill in beams recursively
        CalculateBeamRecursively(startIndex, startDirection);

        foreach (var row in _energizedGrid)
        {
            row.ForEach(x => Console.Write(x));
            Console.WriteLine();
        }
        
        return _energizedGrid.SelectMany(row => row).Count(cell => cell == '#');
    }

    private void CalculateBeamRecursively((int row, int col) index, Direction direction)
    {
        if (index.row < 0 || index.row >= _grid.Count ||
            index.col < 0 || index.col >= _grid[0].Count)
        {
            return;
        }

        if (!_directionGrid[index.row][index.col].Contains(direction))
        {
            _directionGrid[index.row][index.col].Add(direction);
        }
        else
        {
            return;
        }
        
        var entry = _grid[index.row][index.col];
        _energizedGrid[index.row][index.col] = '#';
        if (entry == '.')
        {
            switch (direction)
            {
                case Direction.E:
                    CalculateBeamRecursively((index.row, index.col + 1), Direction.E);
                    break;
                case Direction.W:
                    CalculateBeamRecursively((index.row, index.col - 1), Direction.W);
                    break;
                case Direction.N:
                    CalculateBeamRecursively((index.row - 1, index.col), Direction.N);
                    break;
                case Direction.S:
                    CalculateBeamRecursively((index.row + 1, index.col), Direction.S);
                    break;
            }
        }
        else if (_mirrors.Contains(entry))
        {
            var newRow = -1;
            var newCol = -1;
            var newDirection = Direction.Unknown;
            switch (direction)
            {
                case Direction.E:
                    newRow = entry == '/' ? index.row - 1 : index.row + 1;
                    newDirection = entry == '/' ? Direction.N : Direction.S;
                    CalculateBeamRecursively((newRow, index.col), newDirection);
                    break;
                case Direction.W:
                    newRow = entry == '\\' ? index.row - 1 : index.row + 1;
                    newDirection = entry == '\\' ? Direction.N : Direction.S;
                    CalculateBeamRecursively((newRow, index.col), newDirection);
                    break;
                case Direction.N:
                    newCol = entry == '/' ? index.col + 1 : index.col - 1;
                    newDirection = entry == '/' ? Direction.E : Direction.W;
                    CalculateBeamRecursively((index.row, newCol), newDirection);
                    break;
                case Direction.S:
                    newCol = entry == '\\' ? index.col + 1 : index.col - 1;
                    newDirection = entry == '\\' ? Direction.E : Direction.W;
                    CalculateBeamRecursively((index.row, newCol), newDirection);
                    break;
            }
        }
        else if (_splitters.Contains(entry))
        {
            switch (direction)
            {
                case Direction.E:
                    switch (entry)
                    {
                        case '-':
                            CalculateBeamRecursively((index.row, index.col + 1), Direction.E);
                            break;
                        case '|':
                            // Split into two beams
                            CalculateBeamRecursively((index.row - 1, index.col), Direction.N);
                            CalculateBeamRecursively((index.row + 1, index.col), Direction.S);
                            break;
                    }
                    break;
                case Direction.W:
                    switch (entry)
                    {
                        case '-':
                            CalculateBeamRecursively((index.row, index.col - 1), Direction.W);
                            break;
                        case '|':
                            // Split into two beams
                            CalculateBeamRecursively((index.row - 1, index.col), Direction.N);
                            CalculateBeamRecursively((index.row + 1, index.col), Direction.S);
                            break;
                    }
                    break;
                case Direction.N:
                    switch (entry)
                    {
                        case '|':
                            CalculateBeamRecursively((index.row - 1, index.col), Direction.N);
                            break;
                        case '-':
                            // Split into two beams
                            CalculateBeamRecursively((index.row, index.col - 1), Direction.W);
                            CalculateBeamRecursively((index.row, index.col + 1), Direction.E);
                            break;
                    }
                    break;
                case Direction.S:
                    switch (entry)
                    {
                        case '|':
                            CalculateBeamRecursively((index.row + 1, index.col), Direction.S);
                            break;
                        case '-':
                            // Split into two beams
                            CalculateBeamRecursively((index.row, index.col - 1), Direction.W);
                            CalculateBeamRecursively((index.row, index.col + 1), Direction.E);
                            break;
                    }
                    break;
            }
        }
    }
}
