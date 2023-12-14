using FluentAssertions;

namespace Parabolic;

class Program
{
    static void Main(string[] args)
    {
        var parbolic = new Parabolic();
        parbolic.Solve1("dummydata").Should().Be(136);
        Console.WriteLine(parbolic.Solve1("data"));
    }
}

class Parabolic
{
    public int Solve1(string fileName)
    {
        var grid = File.ReadAllLines($"Data/{fileName}")
            .Select(line => line.ToCharArray().ToList())
            .ToList();
        
        // Turn lever
        for (int col = 0; col < grid[0].Count; col++)
        {
            for (int row = 1; row < grid.Count; row++)
            {
                if (grid[row][col] == 'O')
                {
                    grid[row][col] = '.';
                    var continueSearching = true;
                    while (continueSearching)
                    {
                        for (var idx = row; idx >= 0; idx--)
                        {
                            // If it hits a rock, it stays underneath
                            if (grid[idx][col] == '#' || grid[idx][col] == 'O')
                            {
                                grid[idx + 1][col] = 'O';
                                continueSearching = false;
                                break;
                            }

                            if (idx != 0) continue;
                            // If we hit the top, it should be at the first row
                            grid[idx][col] = 'O';
                            continueSearching = false;
                            break;
                        }
                    }
                }
            }
        }

        var totalCount = 0;
        var totalRows = grid.Count;
        // Calculate number
        for (int row = 0; row != totalRows; row++)
        {
            totalCount += grid[row].Count(ch => ch == 'O') * (totalRows - row);
        }

        return totalCount;
    }
}