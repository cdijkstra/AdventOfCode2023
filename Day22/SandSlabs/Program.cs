using FluentAssertions;

namespace SandSlabs;

class Program
{
    static void Main(string[] args)
    {
        var tetris = new Tetris();
        tetris.Solve1("dummydata").Should().Be(1);
    }
}

class Block
{
    public int entry { get; set; }
    public int minx { get; set; }
    public int miny { get; set; }
    public int minz { get; set; }
    public int maxx { get; set; }
    public int maxy { get; set; }
    public int maxz { get; set; }
}

class Tetris
{
    private List<Block> _blocks = new();
    private List<List<List<int>>> _grid = new();
    public int Solve1(string fileName)
    {
        var entry = 0;
        foreach (var line in File.ReadLines($"Data/{fileName}"))
        {
            var values = line.Split('~');
            var minValues = values[0].Split(',').Select(int.Parse).ToArray();
            var maxValues = values[1].Split(',').Select(int.Parse).ToArray();
            _blocks.Add(new Block
            {
                entry = ++entry,
                minx = minValues[0],
                miny = minValues[1],
                minz = minValues[2],
                maxx = maxValues[0],
                maxy = maxValues[1],
                maxz = maxValues[2],
            });
        }
        
        var maximumValueX = _blocks.Max(block => Math.Max(block.minx, block.maxx));
        var maximumValueY = _blocks.Max(block => Math.Max(block.miny, block.maxy));
        var maximumValueZ = _blocks.Max(block => Math.Max(block.minz, block.maxz));
        for (int x = 0; x <= maximumValueX; x++)
        {
            var secondDimension = new List<List<int>>();
            for (int y = 0; y <= maximumValueY; y++)
            {
                var thirdDimension = new List<int>(Enumerable.Repeat(1, maximumValueZ + 1)); // 0 is the floor so+1
                secondDimension.Add(thirdDimension);
            }
            _grid.Add(secondDimension);
        }
        
        foreach (var block in _blocks)
        {
            for (int x = block.minx; x <= block.maxx; x++)
            {
                for (int y = block.miny; y <= block.maxy; y++)
                {
                    for (int z = block.minz; z <= block.maxz; z++)
                    {
                        _grid[x][y][z] = block.entry;
                    }
                }
            }
        }

        // Now let the blocks fall
        var test = _grid
            .SelectMany((xValues, x) =>
                xValues.SelectMany((yValues, y) =>
                    yValues.Select((zValue, z) => (x, y, z, value: zValue))))
            .Where(coord => coord.value == 2)
            .GroupBy(coord => (coord.x, coord.y))
            .Select(group => group.MinBy(coord => coord.z))
            .Where(coord => AllValuesUnderneathAreZero(coord.x, coord.y, coord.z))
            .Select(coord => (coord.x, coord.y, coord.z, coord.value))
            .ToList();
        
        return 1;
    }
    
    private bool AllValuesUnderneathAreZero(int x, int y, int z)
    {
        for (int currentZ = z - 1; currentZ >= 0; currentZ--)
        {
            if (_grid[x][y][currentZ] != 0)
            {
                return false; // There is a non-zero value underneath
            }
        }
        return true; // All values underneath are zero
    }
}