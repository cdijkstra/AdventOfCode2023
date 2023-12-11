using FluentAssertions;

namespace PipeMaze;

class Program
{
    static void Main(string[] args)
    {
        var maze = new Maze();
        maze.Solve1("dummydata1").Should().Be(4);
        Console.WriteLine(maze.Solve1("data"));
    }
}

enum Direction
{
    N,S,W,E
}

class Maze
{
    private List<List<char>> _grid = new();
    private (int RowIndex, int ColIndex, char Value) _startIndices;

    private List<char> ValidFromNorth = new List<char>()
    {
        '|', 'L', 'J'
    };
    
    private List<char> ValidFromSouth = new List<char>()
    {
        '|', 'F', '7'
    };
    
    private List<char> ValidFromEast = new List<char>()
    {
        '-', 'L', 'F', 
    };
    
    private List<char> ValidFromWest = new List<char>()
    {
        '-', 'J', '7', 
    };
    
    public int Solve1(string fileName)
    {
        _grid = File.ReadAllLines($"Data/{fileName}")
            .Select(line => line.ToList())
            .ToList();

        // Find entry with S
        _startIndices = _grid.SelectMany((line, rowIndex) =>
                line.Select((c, colIndex) => (RowIndex: rowIndex, ColIndex: colIndex, Value: c)))
            .FirstOrDefault(cell => cell.Value == 'S');

        List<int> lengths = new();
        
        // Perform algorithm for all adjacent indices
        foreach (var neighbor in FindAdjacentIndices(_startIndices.RowIndex, _startIndices.ColIndex))
        {
            var continueLoop = true;
            var loopLength = 1;
            var considerEntry = neighbor;
            while (continueLoop)
            {
                if (considerEntry.rowIndex < 0 || considerEntry.rowIndex >= _grid.Count ||
                    considerEntry.colIndex < 0 || considerEntry.colIndex >= _grid[0].Count)
                {
                    continueLoop = false;
                }
                
                var pipe = _grid[considerEntry.rowIndex][considerEntry.colIndex];
                if (considerEntry.dir == Direction.S && !ValidFromNorth.Contains(pipe) ||
                    considerEntry.dir == Direction.N && !ValidFromSouth.Contains(pipe) ||
                    considerEntry.dir == Direction.E && !ValidFromWest.Contains(pipe) ||
                    considerEntry.dir == Direction.W && !ValidFromEast.Contains(pipe))
                {
                    continueLoop = false;
                }
                
                if (considerEntry.rowIndex == _startIndices.RowIndex && considerEntry.colIndex == _startIndices.ColIndex && 
                                    loopLength > 0)
                {
                    lengths.Add(loopLength / 2);
                    continueLoop = false;
                }

                if (continueLoop)
                {
                    loopLength++;
                    considerEntry = FindNextEntry(considerEntry.rowIndex, considerEntry.colIndex, considerEntry.dir, pipe);
                }
            }
        }

        return lengths.Min();
    }

    private List<(int rowIndex, int colIndex, Direction dir)> FindAdjacentIndices(int rowIndex, int colIndex)
    {
        List<(int, int colIndex, Direction)> adjacentIndices = new();
        if (rowIndex > 0)
        {
            adjacentIndices.Add((rowIndex - 1, colIndex, Direction.N));
        }
        if (rowIndex < _grid.Count - 1)
        {
            adjacentIndices.Add((rowIndex + 1, colIndex, Direction.S));
        }
        if (colIndex > 0)
        {
            adjacentIndices.Add((rowIndex, colIndex - 1, Direction.W));
        }
        if (colIndex < _grid[0].Count - 1)
        {
            adjacentIndices.Add((rowIndex, colIndex + 1, Direction.E));
        }

        return adjacentIndices;
    }
    
    private (int rowIndex, int colIndex, Direction dir) FindNextEntry(int rowIndex, int colIndex, Direction dir, char pipe)
    {
        switch (dir)
        {
            case Direction.N when pipe == '|':
                return (rowIndex - 1, colIndex, Direction.N);
            case Direction.N when pipe == 'F':
                return (rowIndex, colIndex + 1, Direction.E);
            case Direction.N when pipe == '7':
                return (rowIndex, colIndex - 1, Direction.W);
            case Direction.S when pipe == '|':
                return (rowIndex + 1, colIndex, Direction.S);
            case Direction.S when pipe == 'L':
                return (rowIndex, colIndex + 1, Direction.E);
            case Direction.S when pipe == 'J':
                return (rowIndex, colIndex - 1, Direction.W);
            case Direction.E when pipe == '-':
                return (rowIndex, colIndex + 1, Direction.E);
            case Direction.E when pipe == 'J':
                return (rowIndex - 1, colIndex, Direction.N);
            case Direction.E when pipe == '7':
                return (rowIndex + 1, colIndex, Direction.S);
            case Direction.W when pipe == '-':
                return (rowIndex, colIndex - 1, Direction.W);
            case Direction.W when pipe == 'L':
                return (rowIndex - 1, colIndex, Direction.N);
            case Direction.W when pipe == 'F':
                return (rowIndex + 1, colIndex, Direction.S);
            default:
                throw new Exception();
        }
    }
}