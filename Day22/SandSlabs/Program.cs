using FluentAssertions;
namespace SandSlabs;

class Program
{
    static void Main(string[] args)
    {
        var tetris = new Tetris();
        tetris.Solve1("dummydata").Should().Be(5);
        Console.WriteLine(tetris.Solve1("data"));
        tetris.Solve2("dummydata").Should().Be(7);
        Console.WriteLine(tetris.Solve2("data"));
    }
}

class Block
{
    public int entry { get; set; }
    public (int minx, int miny, int minz, int maxx, int maxy, int maxz) Values { get; set; }
}

class Tetris
{
    private Dictionary<int, List<(int x, int y, int z, int value)>> _blocksLocations = new();
    private List<Block> _blocks = new();
    private List<List<List<int>>> _grid = new();
    private int _count = 0;
    public int Solve1(string fileName)
    {
        Initialize(fileName);
        BlocksFall();

        _count = 0;
        foreach (var block in _blocks)
        {
            var blockCoordinates = FindBlockCoordinates(block.entry);
            var highestCoordinates = FindHighestBlockCoordinates(blockCoordinates);
            
            var blocksAbove = highestCoordinates
                .Select(coors => _grid[coors.x][coors.y][coors.z + 1])
                .Distinct()
                .Where(value => value != 0)
                .ToList();
            
            if (blocksAbove.All(ens => ens == 0)) _count++;
            else
            {
                var blocksWontFall = blocksAbove.All(blockAbove =>
                {
                    var blockAboveCoordinates = FindBlockCoordinates(blockAbove);
                    var lowestCoordinates = FindLowestBlockCoordinates(blockAboveCoordinates);

                    return lowestCoordinates.Any(vals =>
                        _grid[vals.x][vals.y][vals.z - 1] != 0 && _grid[vals.x][vals.y][vals.z - 1] != block.entry);
                });
                
                if (blocksWontFall) _count++;
            }
        }

        return _count;
    }
    
    public int Solve2(string fileName)
    {
        Initialize(fileName);
        BlocksFall();

        _count = 0;
        foreach (var block in _blocks)
        {
            var blockCoordinates = FindBlockCoordinates(block.entry);
            var highestCoordinates = FindHighestBlockCoordinates(blockCoordinates);
            
            var blocksAbove = highestCoordinates
                .Select(coors => _grid[coors.x][coors.y][coors.z + 1])
                .Where(value => value != 0)
                .Distinct().ToList();

            if (blocksAbove.Count == 0) continue;
            
            var newGrid = ConstructNewGrid(block);
            List<int> deletedBlocks = new() { block.entry };
            ChainReaction(blocksAbove, newGrid, deletedBlocks);
        }

        return _count;
    }

    private List<List<List<int>>> ConstructNewGrid(Block block)
    {
        var newGrid = _grid.Select(iList => iList
                .Select(jList => jList
                    .Select(value => value == block.entry ? 0 : value).ToList())
                .ToList())
            .ToList();
        return newGrid;
    }

    private void ChainReaction(List<int> currentBlocks, List<List<List<int>>> newGrid, List<int> deletedBlocks)
    {
        var blocksFalling = false;
        List<int> blocksAbove = new();
        foreach (var blockAbove in currentBlocks)
        {
            var blockAboveCoordinates = FindBlockCoordinates(blockAbove).ToList();
            var lowestCoordinates = FindLowestBlockCoordinates(blockAboveCoordinates);
            if (!lowestCoordinates.All(vals => newGrid[vals.x][vals.y][vals.z - 1] == 0)) continue;
            
            deletedBlocks.Add(blockAbove);
            blocksFalling = true;
            _count++;
            var highestCoordinates = FindHighestBlockCoordinates(blockAboveCoordinates);

            var newBlocks = highestCoordinates
                .Select(coors => _grid[coors.x][coors.y][coors.z + 1])
                .Where(value => value != 0)
                .Distinct()
                .ToList();
            blocksAbove.AddRange(newBlocks);

            newGrid = newGrid
                .Select(iList => iList
                    .Select(jList => jList
                        .Select(value => value == blockAbove ? 0 : value)
                        .ToList())
                    .ToList())
                .ToList();
        }
        
        if (!blocksFalling || !blocksAbove.Any()) return;
        
        var uniqueBlocksAbove = blocksAbove.Distinct().Where(value => !deletedBlocks.Contains(value)).ToList();
        ChainReaction(uniqueBlocksAbove, newGrid, deletedBlocks);
    }

    private void BlocksFall()
    {
        var continueLoop = true;
        while (continueLoop)
        {
            continueLoop = false; // Unless a block fell
            // Now let the blocks fall
            foreach (var block in _blocks)
            {
                var blockCoordinates = FindBlockCoordinates(block.entry);
                var lowestCoordinates = FindLowestBlockCoordinates(blockCoordinates);
                var maxZ = blockCoordinates.Max(val => val.z);

                if (lowestCoordinates.All(vals => vals.z == 1)) continue;
                if (!lowestCoordinates.All(vals => _grid[vals.x][vals.y][vals.z - 1] == 0)) continue;

                continueLoop = true;
                lowestCoordinates.ForEach(coors =>
                {
                    _grid[coors.x][coors.y][coors.z - 1] = coors.value;
                    _grid[coors.x][coors.y][maxZ] = 0;
                });

                var updatedCache = _blocksLocations[block.entry]
                    .Select(data => (data.x, data.y, data.z - 1, data.value)).ToList();
                _blocksLocations[block.entry] = updatedCache;
            }
        }
    }

    private void Initialize(string fileName)
    {
        _blocksLocations = new();
        _blocks = new();
        _grid = new();
        var entry = 0;
        foreach (var line in File.ReadLines($"Data/{fileName}"))
        {
            var values = line.Split('~');
            var minValues = values[0].Split(',').Select(int.Parse).ToArray();
            var maxValues = values[1].Split(',').Select(int.Parse).ToArray();
            _blocks.Add(new Block
            {
                entry = ++entry,
                Values = (minx: minValues[0], miny: minValues[1], minz: minValues[2], maxx: maxValues[0], maxy: maxValues[1], maxz: maxValues[2])
            });
        }

        var maximumValueX = _blocks.Max(block => Math.Max(block.Values.minx, block.Values.maxx));
        var maximumValueY = _blocks.Max(block => Math.Max(block.Values.miny, block.Values.maxy));
        var maximumValueZ = _blocks.Max(block => Math.Max(block.Values.minz, block.Values.maxz));
        for (int x = 0; x <= maximumValueX; x++)
        {
            var secondDimension = new List<List<int>>();
            for (int y = 0; y <= maximumValueY; y++)
            {
                var thirdDimension = new List<int>(Enumerable.Repeat(0, maximumValueZ + 1)); // 0 is the floor so+1
                secondDimension.Add(thirdDimension);
            }

            _grid.Add(secondDimension);
        }

        foreach (var block in _blocks)
        {
            for (int x = block.Values.minx; x <= block.Values.maxx; x++)
            {
                for (int y = block.Values.miny; y <= block.Values.maxy; y++)
                {
                    for (int z = block.Values.minz; z <= block.Values.maxz; z++)
                    {
                        _grid[x][y][z] = block.entry;
                    }
                }
            }
        }
    }

    private static List<(int x, int y, int z, int value)> FindLowestBlockCoordinates(IEnumerable<(int x, int y, int z, int value)> blockCoordinates)
    {
        var lowestCoordinates = blockCoordinates
            .GroupBy(coord => (coord.x, coord.y))
            .Select(group => group.MinBy(coord => coord.z))
            .Select(coord => (coord.x, coord.y, coord.z, coord.value))
            .ToList();
        return lowestCoordinates;
    }
    
    private static List<(int x, int y, int z, int value)> FindHighestBlockCoordinates(IEnumerable<(int x, int y, int z, int value)> blockCoordinates)
    {
        var lowestCoordinates = blockCoordinates
            .GroupBy(coord => (coord.x, coord.y))
            .Select(group => group.MaxBy(coord => coord.z))
            .Select(coord => (coord.x, coord.y, coord.z, coord.value))
            .ToList();
        return lowestCoordinates;
    }
    
    private List<(int x, int y, int z, int value)> FindBlockCoordinates(int blockEntry)
    {
        if (_blocksLocations.TryGetValue(blockEntry, out var coordinates))
        {
            return coordinates;
        }
        
        var blockCoordinates = _grid
            .SelectMany((xValues, x) =>
                xValues.SelectMany((yValues, y) =>
                    yValues.Select((zValue, z) => (x, y, z, value: zValue))))
            .Where(coord => coord.value == blockEntry).ToList();
        _blocksLocations[blockEntry] = blockCoordinates;
        return blockCoordinates;
    }
}