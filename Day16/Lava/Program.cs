using FluentAssertions;
namespace Lava;

class Program
{
    static void Main(string[] args)
    {
        var lava = new Lava();
        lava.Solve1("dummydata").Should().Be(46);
        Console.WriteLine(lava.Solve1("data"));
        lava.Solve2("dummydata").Should().Be(51);
        Console.WriteLine(lava.Solve2("data"));
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
        Initialize(fileName);
        // Determine start position and direction
        (int row, int col) startIndex = (0, 0);
        var startDirection = Direction.E;
        
        // Fill in beams recursively
        CalculateBeamRecursively(startIndex, startDirection);
        return _energizedGrid.SelectMany(row => row).Count(cell => cell == '#');
    }
    
    public int Solve2(string fileName)
    {
        var validDirections = Enum.GetValues(typeof(Direction)).Cast<Direction>().Where(d => d != Direction.Unknown).ToList();
        List<int> listEnergizedEntries = new();
        Initialize(fileName);
        
        foreach (var startRow in Enumerable.Range(0, _grid.Count))
        {
            foreach (var startCol in Enumerable.Range(0, _grid[0].Count))
            {
                foreach (Direction startDirection in validDirections)
                {
                    Initialize(fileName);
                    (int row, int col) startIndex = (startRow, startCol);
                    CalculateBeamRecursively(startIndex, startDirection);
                    var energizedEntries = _energizedGrid.SelectMany(row => row).Count(cell => cell == '#');
                    listEnergizedEntries.Add(energizedEntries);
                }
            }
        }

        var totalEntries = _grid.Count * _grid[0].Count * 4;
        Console.WriteLine($"Expected {totalEntries} entries and found {listEnergizedEntries.Count}");
        // var higherEntries = listEnergizedEntries.Where(val => val > 6978).OrderBy(x => x).ToList();
        // higherEntries.ForEach(x => Console.WriteLine(x));
        return listEnergizedEntries.Max();
    }

    private void Initialize(string fileName)
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
    }

    private void CalculateBeamRecursively((int row, int col) index, Direction direction)
    {
        // Check index, is it within grid?
        if (index.row < 0 || index.row >= _grid.Count ||
            index.col < 0 || index.col >= _grid[0].Count)
        {
            return;
        }

        // Check if position has not been visited before with same Direction
        if (!_directionGrid[index.row][index.col].Contains(direction))
        {
            _directionGrid[index.row][index.col].Add(direction);
        }
        else
        {
            return;
        }
        
        // Retrieve currentChar and set energizedgrid entry
        var currentChar = _grid[index.row][index.col];
        _energizedGrid[index.row][index.col] = '#';
        
        if (currentChar == '.')
        {
            // Continue in same direction
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
        else if (_mirrors.Contains(currentChar))
        {
            // Rotate 90 beams degrees, i.e. change Direction and (row or column)
            var newRow = -1;
            var newCol = -1;
            var newDirection = Direction.Unknown;
            switch (direction)
            {
                case Direction.E:
                    newRow = currentChar == '/' ? index.row - 1 : index.row + 1;
                    newDirection = currentChar == '/' ? Direction.N : Direction.S;
                    CalculateBeamRecursively((newRow, index.col), newDirection);
                    break;
                case Direction.W:
                    newRow = currentChar == '\\' ? index.row - 1 : index.row + 1;
                    newDirection = currentChar == '\\' ? Direction.N : Direction.S;
                    CalculateBeamRecursively((newRow, index.col), newDirection);
                    break;
                case Direction.N:
                    newCol = currentChar == '/' ? index.col + 1 : index.col - 1;
                    newDirection = currentChar == '/' ? Direction.E : Direction.W;
                    CalculateBeamRecursively((index.row, newCol), newDirection);
                    break;
                case Direction.S:
                    newCol = currentChar == '\\' ? index.col + 1 : index.col - 1;
                    newDirection = currentChar == '\\' ? Direction.E : Direction.W;
                    CalculateBeamRecursively((index.row, newCol), newDirection);
                    break;
            }
        }
        else if (_splitters.Contains(currentChar))
        {
            // Two options.
            // Perpendicular => Continue in same direction
            // Parallel => Split into two beams
            switch (direction)
            {
                case Direction.E:
                    switch (currentChar)
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
                    switch (currentChar)
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
                    switch (currentChar)
                    {
                        case '-':
                            // Split into two beams
                            CalculateBeamRecursively((index.row, index.col - 1), Direction.W);
                            CalculateBeamRecursively((index.row, index.col + 1), Direction.E);
                            break;
                        case '|':
                            CalculateBeamRecursively((index.row - 1, index.col), Direction.N);
                            break;
                    }
                    break;
                case Direction.S:
                    switch (currentChar)
                    {
                        case '-':
                            // Split into two beams
                            CalculateBeamRecursively((index.row, index.col - 1), Direction.W);
                            CalculateBeamRecursively((index.row, index.col + 1), Direction.E);
                            break;
                        case '|':
                            CalculateBeamRecursively((index.row + 1, index.col), Direction.S);
                            break;
                    }
                    break;
            }
        }
    }
}
