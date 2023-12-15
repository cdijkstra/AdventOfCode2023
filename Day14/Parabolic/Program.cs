using FluentAssertions;
namespace Parabolic;

static class Program
{
    static void Main(string[] args)
    {
        var parbolic = new Parabolic();
        parbolic.Solve1("dummydata").Should().Be(136);
        Console.WriteLine(parbolic.Solve1("data"));
        parbolic.Solve2("dummydata", 1000000000).Should().Be(64);
        Console.WriteLine(parbolic.Solve2("data", 1000000000));
    }
}

class Parabolic
{
    private List<List<char>> _grid = new();
    private readonly List<char> _rocks = new() { 'O', '#' };
    public int Solve1(string fileName)
    {
        Initialize(fileName);
        FallNorth();
        return CalculateLoad();
    }
    
    public int Solve2(string fileName, int repeatLoops)
    {
        _grid = File.ReadAllLines($"Data/{fileName}")
            .Select(line => line.ToCharArray().ToList())
            .ToList();

        List<int> gridHashes = new();
        var loopStarts = 0;
        var loopLength = 0;
        List<int> loads = new();
        // Apply cycles until we find recurrence!
        foreach (var repeats in Enumerable.Range(0, repeatLoops))
        {
            ApplyCycle();
            var hash = CalculateGridHash();
            if (gridHashes.Contains(hash))
            {
                loopStarts = gridHashes.IndexOf(hash);
                loopLength = repeats - gridHashes.IndexOf(hash);
                Console.WriteLine($"Now at {repeats} and found same hash at {loopStarts} with looplength {loopLength}");
                break;
            }
            gridHashes.Add(hash);
        }
        foreach (var unused in Enumerable.Range(0, loopLength))
        {
            loads.Add(CalculateLoad());
            ApplyCycle();
        }

        var repeatedLoops = (int)Math.Floor((double)(repeatLoops - loopStarts - 1) / loopLength);
        var indexInLoop = repeatLoops - (repeatedLoops * loopLength) - loopStarts - 1;
        
        return loads[indexInLoop];
    }
    
    private void Initialize(string fileName)
    {
        _grid = File.ReadAllLines($"Data/{fileName}")
            .Select(line => line.ToCharArray().ToList())
            .ToList();
    }
    
    private int CalculateGridHash()
    {
        return _grid.SelectMany(row => row).Aggregate(17, (current, cell) => current * 31 + cell.GetHashCode());
    }

    private void ApplyCycle()
    {
        FallNorth();
        FallWest();
        FallSouth();
        FallEast();
    }

    private int CalculateLoad()
    {
        return _grid.Select((row, rowIndex) => row
                .Count(ch => ch == 'O') * (_grid.Count - rowIndex))
            .Sum();
    }

    private void FallNorth()
    {
        for (int col = 0; col < _grid[0].Count; col++)
        {
            for (int row = 1; row < _grid.Count; row++)
            {
                if (_grid[row][col] != 'O') continue;
                
                _grid[row][col] = '.';
                var continueSearching = true;
                while (continueSearching)
                {
                    for (var idx = row; idx >= 0; idx--)
                    {
                        // If it hits a rock, it stays underneath
                        if (_rocks.Contains(_grid[idx][col]))
                        {
                            _grid[idx + 1][col] = 'O';
                            continueSearching = false;
                            break;
                        }

                        if (idx != 0) continue;
                        // If we hit the top, it should be at the first row
                        _grid[idx][col] = 'O';
                        continueSearching = false;
                        break;
                    }
                }
            }
        }
    }
    
    private void FallWest()
    {
        for (int row = 0; row < _grid.Count; row++)
        {
            for (int col = 1; col < _grid[0].Count; col++)
            {
                if (_grid[row][col] != 'O') continue;
                _grid[row][col] = '.';
                var continueSearching = true;
                while (continueSearching)
                {
                    for (var idy = col; idy >= 0; idy--)
                    {
                        // If it hits a rock, it stays underneath
                        if (_rocks.Contains(_grid[row][idy]))
                        {
                            _grid[row][idy + 1] = 'O';
                            continueSearching = false;
                            break;
                        }

                        if (idy != 0) continue;
                        // If we hit the top, it should be fully left
                        _grid[row][idy] = 'O';
                        continueSearching = false;
                        break;
                    }
                }
            }
        }
    }
    
    private void FallEast()
    {
        for (int row = 0; row < _grid.Count; row++)
        {
            for (int col = _grid[0].Count - 2; col >= 0; col--)
            {
                if (_grid[row][col] != 'O') continue;
                _grid[row][col] = '.';
                var continueSearching = true;
                while (continueSearching)
                {
                    for (var idy = col; idy < _grid[0].Count; idy++)
                    {
                        // If it hits a rock, it stays underneath
                        if (_rocks.Contains(_grid[row][idy]))
                        {
                            _grid[row][idy - 1] = 'O';
                            continueSearching = false;
                            break;
                        }

                        if (idy != _grid[0].Count - 1) continue;
                        // If we hit the top, it should be fully left
                        _grid[row][idy] = 'O';
                        continueSearching = false;
                        break;
                    }
                }
            }
        }
    }
    
    private void FallSouth()
    {
        for (int row = _grid.Count - 1; row >= 0; row--)
        {
            for (int col = 0; col < _grid[0].Count; col++)
            {
                if (_grid[row][col] != 'O') continue;
                _grid[row][col] = '.';
                var continueSearching = true;
                while (continueSearching)
                {
                    for (var idx = row; idx < _grid.Count; idx++)
                    {
                        // If it hits a rock, it stays underneath
                        if (_rocks.Contains(_grid[idx][col]))
                        {
                            _grid[idx - 1][col] = 'O';
                            continueSearching = false;
                            break;
                        }

                        if (idx != _grid.Count - 1) continue;
                        // If we hit the top, it should be at the first row
                        _grid[idx][col] = 'O';
                        continueSearching = false;
                        break;
                    }
                }
            }
        }
    }
}