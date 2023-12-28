using FluentAssertions;

namespace StepCounter;

class Program
{
    static void Main(string[] args)
    {
        var garden = new Garden();
        garden.Solve1("dummydata", 6).Should().Be(16);
        Console.WriteLine(garden.Solve1("data", 64));
    }
}

class Garden
{
    private List<List<char>> _grid = new();
    private List<List<char>> _newgrid = new();
    private List<char> _notFreeGarden = new() { '#', '0' };
    
    public int Solve1(string fileName, int repeats)
    {
        _grid = File.ReadLines($"Data/{fileName}")
            .Select(line => line.ToList())
            .ToList();

        var startCoordinates = _grid.SelectMany((row, rowIndex) =>
            row.Select((cell, columnIndex) => new { Row = rowIndex, Column = columnIndex, Cell = cell })
        ).FirstOrDefault(x => x.Cell == 'S');
        
        var coordinates = new List<(int row, int column)>() { (startCoordinates.Row, startCoordinates.Column )};
        
        foreach (int idx in Enumerable.Range(0, repeats))
        {
            List<(int row, int col)> newLocations = coordinates
                .SelectMany(presentAt => FindPossibleLocations(presentAt.Item1, presentAt.Item2))
                .Distinct()
                .ToList();

            coordinates = newLocations;
        }
        
        coordinates.ForEach(coors =>
        {
            _grid[coors.row][coors.column] = '0';
        });
        
        foreach (var row in _grid)
        {
            row.ForEach(col => Console.Write(col));
            Console.WriteLine();
        }

        return coordinates.Count;
    }

    private List<(int row, int col)> FindPossibleLocations(int row, int col)
    {
        List<(int row, int col)> newCoords = new();
        if (row > 0 && !_notFreeGarden.Contains(_grid[row - 1][col]))
        {
            newCoords.Add((row - 1, col));
        }
        if (row < _grid.Count - 1 && !_notFreeGarden.Contains(_grid[row + 1][col]))
        {
            newCoords.Add((row + 1, col));
        }
        if (col > 0 && !_notFreeGarden.Contains(_grid[row][col - 1]))
        {
            newCoords.Add((row, col - 1));
        }
        if (col < _grid[0].Count && !_notFreeGarden.Contains(_grid[row][col + 1]))
        {
            newCoords.Add((row, col + 1));
        }

        return newCoords;
    }
}
