using System.Text.RegularExpressions;
using FluentAssertions;

namespace PipeMaze;

class Program
{
    static void Main(string[] args)
    {
        var maze = new Maze();
        maze.Solve1("dummydata1").Should().Be(4);
        Console.WriteLine(maze.Solve1("data"));
        maze.Solve2("dummydata1").Should().Be(1);
        maze.Solve2("dummydata3").Should().Be(8);
        Console.WriteLine($"Answer = {maze.Solve2("data")}");
    }
}

enum Direction
{
    N,S,W,E
}

class Maze
{
    private List<List<char>> _grid = new();
    private List<List<char>> _subGrid = new();

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
    
    public int Solve2(string fileName)
    {
        _grid = File.ReadAllLines($"Data/{fileName}")
            .Select(line => line.ToList())
            .ToList();

        // Find entry with S
        _startIndices = _grid.SelectMany((line, rowIndex) =>
                line.Select((c, colIndex) => (RowIndex: rowIndex, ColIndex: colIndex, Value: c)))
            .FirstOrDefault(cell => cell.Value == 'S');

        List<(int rowIndex, int colIndex)> pipeCoordinates = new();
        
        // Perform algorithm for all adajacent indices
        foreach (var neighbor in FindAdjacentIndices(_startIndices.RowIndex, _startIndices.ColIndex))
        {
            List<(int rowIndex, int colIndex)> possiblePipeCoordinates = new();
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
                
                if (considerEntry.rowIndex == _startIndices.RowIndex && considerEntry.colIndex == _startIndices.ColIndex && loopLength > 0)
                {
                    possiblePipeCoordinates.Add((considerEntry.rowIndex, considerEntry.colIndex));
                    pipeCoordinates = possiblePipeCoordinates;
                    continueLoop = false;
                }

                if (continueLoop)
                {
                    possiblePipeCoordinates.Add((considerEntry.rowIndex, considerEntry.colIndex));
                    loopLength++;
                    considerEntry = FindNextEntry(considerEntry.rowIndex, considerEntry.colIndex, considerEntry.dir, pipe);
                }
            }
        }

        // Find min,max row and column in pipeCoordinates
        var minRow = pipeCoordinates.Min(x => x.rowIndex);
        var maxRow = pipeCoordinates.Max(x => x.rowIndex);
        var minCol = pipeCoordinates.Min(x => x.colIndex);
        var maxCol = pipeCoordinates.Max(x => x.colIndex);
        // Create subgrid and apply Flooding algorithm

        var padding = 1;
        List<List<char>> subGrid = _grid
            .Skip(minRow)
            .Take(maxRow - minRow + 1)
            .Select(row => row.Skip(minCol).Take(maxCol - minCol + 1).ToList())
            .ToList();
        
        subGrid.Insert(0, new List<char>(Enumerable.Repeat('.', subGrid[0].Count)));
        subGrid.Add(new List<char>(Enumerable.Repeat('.', subGrid[0].Count)));
        subGrid.ForEach(row =>
        {
            row.Insert(0, '.');
            row.Add('.');
        });
        
        _subGrid = subGrid;
        // Now apply flooding algorithm
        (int rowIndex, int colIndex) coordinates = (0, 0);
        FloodCoordinateRecursively(coordinates.rowIndex, coordinates.colIndex);

        // Now check which ones are locked inside as well from the remaining '.' entries
        List<(int rowIndex, int colIndex)> modifyEntries = new();
        for (var rowIndex = 0; rowIndex < _subGrid.Count; rowIndex++)
        {
            var row = _subGrid[rowIndex];
            foreach (var (character, index) in row.Select((c, i) => (c, i)))
            {
                if (character != '.') continue;
                var pipeCrossings = 0;
                // Use LINQ to get the List<char> from the index to the last entry in the row
                string substring = string.Join("", row.Skip(index).ToList());
                pipeCrossings += substring.Count(c => c == '|');
                // Your logic for processing the substring goes here
                var comp1 = Regex.Matches(substring, @"F-*J").Count;
                var comp2 = Regex.Matches(substring, @"L-*7").Count;
                pipeCrossings += comp1 + comp2;
                // For example, print the characters in the substring
                if (pipeCrossings % 2 == 0 && pipeCrossings > 0)
                {
                    modifyEntries.Add((rowIndex, index));
                }
            }
        }
        modifyEntries.ForEach(coordinates => _subGrid[coordinates.rowIndex][coordinates.colIndex] = '0');
        var countLockedEntries = _subGrid.Sum(row => row.Count(entry => entry == '.'));

        Console.WriteLine();

        foreach (var row in _subGrid)
        {
            row.ForEach(x => Console.Write(x));
            Console.WriteLine();
        }
        
        return countLockedEntries;
    }

    private void FloodCoordinateRecursively(int rowIndex, int colIndex)
    {
        if (_subGrid[rowIndex][colIndex] != '.')
        {
            return;
        }
        
        _subGrid[rowIndex][colIndex] = '0';
        var newCoordinates = FindAdjacentIndicesIncludingDiagonal(rowIndex, colIndex);
        var newCoordinatesToFlood =
            newCoordinates.Where(indices => _subGrid[indices.rowIndex][indices.colIndex] == '.').ToList();
        newCoordinatesToFlood.ForEach(coordinates =>
            FloodCoordinateRecursively(coordinates.rowIndex, coordinates.colIndex));
    }
    
    private List<(int rowIndex, int colIndex)> FindAdjacentIndicesIncludingDiagonal(int rowIndex, int colIndex)
    {
        List<(int rowIndex, int colIndex)> adjacentIndices = new();
        if (rowIndex > 0)
        {
            adjacentIndices.Add((rowIndex - 1, colIndex));
        }
        if (rowIndex < _subGrid.Count - 1)
        {
            adjacentIndices.Add((rowIndex + 1, colIndex));
        }
        if (colIndex > 0)
        {
            adjacentIndices.Add((rowIndex, colIndex - 1));
        }
        if (colIndex < _subGrid[0].Count - 1)
        {
            adjacentIndices.Add((rowIndex, colIndex + 1));
        }
        
        // Diagonals
        if (rowIndex > 0 && colIndex > 0)
        {
            adjacentIndices.Add((rowIndex - 1, colIndex - 1));
        }
        if (rowIndex > 0 && colIndex < _subGrid[0].Count - 1)
        {
            adjacentIndices.Add((rowIndex - 1, colIndex + 1));
        }
        if (rowIndex < _subGrid.Count - 1 && colIndex > 0)
        {
            adjacentIndices.Add((rowIndex + 1, colIndex - 1));
        }
        if (rowIndex < _subGrid.Count - 1 && colIndex < _subGrid[0].Count - 1)
        {
            adjacentIndices.Add((rowIndex + 1, colIndex + 1));
        }

        return adjacentIndices;
    }
    
    private List<(int rowIndex, int colIndex, Direction dir)> FindAdjacentIndices(int rowIndex, int colIndex)
    {
        List<(int rowIndex, int colIndex, Direction)> adjacentIndices = new();
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