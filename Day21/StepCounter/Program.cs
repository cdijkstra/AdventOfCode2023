namespace StepCounter;

class Program
{
    static void Main(string[] args)
    {
        var garden = new Garden();
        // garden.Solve1("dummydata", 6).Should().Be(16);
        // Console.WriteLine(garden.Solve1("data", 64));
        //
        Console.WriteLine(garden.Solve2("data", 26501365));
        // Higher than 618257138282108
    }
}

class Garden
{
    private List<List<char>> _grid = new();
    private readonly List<char> _notFreeGarden = new() { '#', '0' };
    
    public int Solve1(string fileName, int repeats)
    {
        Initialize(fileName);
        var coordinates = FindStartCoordinates();

        return CalculateAmountOfReachablePoints(repeats, coordinates);
    }

    private int AmountOfPointsWithStartingPosition(string fileName, int row, int col, int steps)
    {
        Initialize(fileName);
        var coordinates = new List<(int row, int column)>()
        {
            (row, col)
        };

        return CalculateAmountOfReachablePoints(steps, coordinates);
    }

    private int CalculateAmountOfReachablePoints(int repeats, List<(int row, int column)> coordinates)
    {
        foreach (int unused in Enumerable.Range(0, repeats))
        {
            List<(int row, int col)> newLocations = coordinates
                .SelectMany(presentAt => FindPossibleLocations(presentAt.Item1, presentAt.Item2))
                .Distinct()
                .ToList();

            coordinates = newLocations;
        }

        coordinates.ForEach(coors => { _grid[coors.row][coors.column] = '0'; });

        foreach (var row in _grid)
        {
            row.ForEach(ch => Console.Write(ch));
            Console.WriteLine();
        }
        
        return coordinates.Count;
    }

    private List<(int row, int column)> FindStartCoordinates()
    {
        var startCoordinates = _grid.SelectMany((row, rowIndex) =>
            row.Select((cell, columnIndex) => new { Row = rowIndex, Column = columnIndex, Cell = cell })
        ).FirstOrDefault(x => x.Cell == 'S');
        var coordinates = new List<(int row, int column)> { (startCoordinates.Row, startCoordinates.Column) };
        return coordinates;
    }

    private void Initialize(string fileName)
    {
        _grid = File.ReadLines($"Data/{fileName}")
            .Select(line => line.ToList())
            .ToList();
    }

    public long Solve2(string fileName, int repeats)
    {
        Initialize(fileName);
        var furthestGrid = (repeats - (_grid.Count - 1) / 2) / _grid.Count; // = 202300
        // We see that the amount of steps allows us to reach the end of a grid exactly when walking in one direction
        // The first one has to traverse distance of (_grid.Count - 1) / 2) and then 202299 remaining ones
        //         [] 
        //       [][][]
        //     [][][][][]
        //   [][][][][][][]
        // [][][][][][][][][]
        //   [][][[][][][]
        //     [][][][][]
        //       [][][]
        //         []
        
        // Within all except the most outer ones N,S,W,E we can very likely reach all points, given that the matrix is sparse
        // The grid has an odd size. That means that we can reach different points for 'odd' and 'even' grids, where the origin is an 'even' grid.
        // There is however a difference for how many points can be reached for even and odd grids. 
        var evenPoints = Solve1(fileName,  302);
        var oddPoints = Solve1(fileName,  303);
        
        Initialize(fileName);
        var coordinates = FindStartCoordinates();
        var startRow = coordinates[0].row;
        var startCol = coordinates[0].column;
        var gridSize = _grid.Count;
        
        var cornerTop = AmountOfPointsWithStartingPosition(fileName, 0, startCol,gridSize - 1);
        var cornerRight = AmountOfPointsWithStartingPosition(fileName, startRow, 0,gridSize - 1);
        var cornerBottom = AmountOfPointsWithStartingPosition(fileName, gridSize - 1, startCol,gridSize - 1);
        var cornerLeft = AmountOfPointsWithStartingPosition(fileName, startRow, gridSize - 1,gridSize - 1);
        
        // Now we have to add small and big segments across the diagonals.
        var tr_small = AmountOfPointsWithStartingPosition(fileName, 0, gridSize - 1,gridSize / 2 - 1);
        var tl_small = AmountOfPointsWithStartingPosition(fileName, 0, 0,gridSize / 2 - 1);
        var dl_small = AmountOfPointsWithStartingPosition(fileName, gridSize - 1, gridSize - 1,gridSize / 2 - 1);
        var dr_small = AmountOfPointsWithStartingPosition(fileName, gridSize - 1, 0,gridSize / 2 - 1);
        var smallOccurencesPerQuarter = furthestGrid;
        
        var tr_large = AmountOfPointsWithStartingPosition(fileName, 0, 0,(3 * gridSize) / 2 - 1);
        var tl_large = AmountOfPointsWithStartingPosition(fileName, 0, gridSize - 1,(3 * gridSize) / 2 - 1);
        var dl_large = AmountOfPointsWithStartingPosition(fileName, gridSize - 1, gridSize - 1,(3 * gridSize) / 2 - 1);
        var dr_large = AmountOfPointsWithStartingPosition(fileName, gridSize - 1, 0,(3 * gridSize) / 2 - 1);
        var largeOccurencesPerQuarter = furthestGrid - 1;
        
        Console.WriteLine($"{oddPoints},{evenPoints},{cornerTop},{cornerRight},{cornerBottom},{cornerLeft},{tr_small},{tl_small},{dr_small},{dl_small},{tr_large},{tl_large},{dr_large},{dl_large}");
        
        // we have to find out how many odd and even grids exist where all points can be reached!
        var oddGrids = Math.Pow(furthestGrid - 1, 2);
        var evenGrids = Math.Pow(furthestGrid, 2);
        
        Console.WriteLine($"{oddGrids},{evenGrids}");
        var answer = evenPoints * evenGrids +
                     oddPoints * oddGrids +
                     cornerTop + cornerBottom + cornerLeft + cornerRight +
                     smallOccurencesPerQuarter * (tr_small + tl_small + dl_small + dr_small) +
                     largeOccurencesPerQuarter * (tr_large + tl_large + dl_large + dr_large);
        return (long) answer;
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
        if (col < _grid[0].Count - 1 && !_notFreeGarden.Contains(_grid[row][col + 1]))
        {
            newCoords.Add((row, col + 1));
        }

        return newCoords;
    }
}
