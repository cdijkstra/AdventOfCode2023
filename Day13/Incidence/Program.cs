using FluentAssertions;

namespace Incidence;

class Program
{
    static void Main(string[] args)
    {
        var incidence = new Incidence();
        incidence.Solve1("dummydata").Should().Be(405);
        Console.WriteLine(incidence.Solve1("data"));
        // incidence.Solve2("dummydata").Should().Be(400);
        Console.WriteLine(incidence.Solve2("data"));
    }
}

class Incidence
{
    public int Solve1(string fileName)
    {
        var grids = File.ReadAllLines($"Data/{fileName}")
            .Aggregate(new List<List<string>> { new List<string>() },
                (acc, line) =>
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        // If the line is blank, start a new entry
                        acc.Add(new List<string>());
                    }
                    else
                    {
                        // Add the line to the current data entry
                        acc.Last().Add(line);
                    }
                    return acc;
                })
            .Where(entry => entry.Any())  // Remove empty entries
            .Select(entry => entry.ToArray());
        
        var totalCount = 0;
        foreach (var grid in grids)
        {
            int width = grid[0].Length;
            var amountVerticalMirrors = width - 1;
            // Vertical mirrors
            for (var idx = 0; idx < amountVerticalMirrors; idx++)
            {
                var takeEntries = Math.Min(idx + 1, width - idx - 1);
                var startLeft = idx + 1 + takeEntries < width - 1 ? 0 : idx + 1 - takeEntries;
                
                if (grid.All(row => row.Skip(startLeft).Take(takeEntries).SequenceEqual(row.Skip(idx + 1).Take(takeEntries).Reverse())))
                {
                    totalCount += idx + 1;
                }
            }
            
            int height = grid.Length;
            var amountHorizontalMirrors = height - 1;
            // Horizontal mirrors
            for (int idy = 0; idy < amountHorizontalMirrors; idy++)
            {
                int takeEntries = Math.Min(idy + 1, height - idy - 1);
                int startTop = idy + 1 + takeEntries < height - 1 ? 0 : idy + 1 - takeEntries;
                
                var topRows = grid.Skip(startTop).Take(takeEntries);
                var bottomRows = grid.Skip(idy + 1).Take(takeEntries).Reverse();
                
                if (topRows.SequenceEqual(bottomRows))
                {
                    totalCount += 100 * (idy + 1);
                }
            }

        }
        return totalCount;
    }
    
    public int Solve2(string fileName)
    {
        var grids = File.ReadAllLines($"Data/{fileName}")
            .Aggregate(new List<List<string>> { new List<string>() },
                (acc, line) =>
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        // If the line is blank, start a new entry
                        acc.Add(new List<string>());
                    }
                    else
                    {
                        // Add the line to the current data entry
                        acc.Last().Add(line);
                    }
                    return acc;
                })
            .Where(entry => entry.Any())  // Remove empty entries
            .Select(entry => entry.ToArray());
        
        var totalCount = 0;
        foreach (var grid in grids)
        {
            var originalMirrorIdx = Int32.MaxValue;
            var originalMirrorIdy = Int32.MaxValue;
            // Find coordinates of current symmetry
            int width = grid[0].Length;
            var amountVerticalMirrors = width - 1;
            // Vertical mirrors
            for (var idx = 0; idx < amountVerticalMirrors; idx++)
            {
                var takeEntries = Math.Min(idx + 1, width - idx - 1);
                var startLeft = idx + 1 + takeEntries < width - 1 ? 0 : idx + 1 - takeEntries;
                
                if (grid.All(row => row.Skip(startLeft).Take(takeEntries).SequenceEqual(row.Skip(idx + 1).Take(takeEntries).Reverse())))
                {
                    originalMirrorIdx = idx;
                }
            }
            
            int height = grid.Length;
            var amountHorizontalMirrors = height - 1;
            // Horizontal mirrors
            for (int idy = 0; idy < amountHorizontalMirrors; idy++)
            {
                int takeEntries = Math.Min(idy + 1, height - idy - 1);
                int startTop = idy + 1 + takeEntries < height - 1 ? 0 : idy + 1 - takeEntries;
                
                var topRows = grid.Skip(startTop).Take(takeEntries);
                var bottomRows = grid.Skip(idy + 1).Take(takeEntries).Reverse();
                
                if (topRows.SequenceEqual(bottomRows))
                {
                    originalMirrorIdy = idy;
                }
            }
            
            var continueChanging = true;
            while (continueChanging)
            {
                for (int idx = 0; idx < height; idx++)
                {
                    for (int idy = 0; idy < width; idy++)
                    {
                        // Toggle '#' and '.'
                        var updatedGrid = grid
                            .Select((row, rowIndex) =>
                                rowIndex == idx
                                    ? row.Select((cell, colIndex) => colIndex == idy ? (cell == '.' ? '#' : '.') : cell).ToArray()
                                    : row.ToArray()
                            )
                            .ToList();
                        
                        // Vertical mirrors
                        for (var mirrorIdx = 0; mirrorIdx < amountVerticalMirrors; mirrorIdx++)
                        {
                            if (mirrorIdx == originalMirrorIdx ) continue;
                            
                            var takeEntries = Math.Min(mirrorIdx + 1, width - mirrorIdx - 1);
                            var startLeft = mirrorIdx + 1 + takeEntries < width - 1 ? 0 : mirrorIdx + 1 - takeEntries;
                    
                            if (updatedGrid.All(row => row.Skip(startLeft).Take(takeEntries).SequenceEqual(row.Skip(mirrorIdx + 1).Take(takeEntries).Reverse())))
                            {
                                totalCount += mirrorIdx + 1;
                                continueChanging = false;
                                break;
                            }
                        }
                        if (!continueChanging)
                        {
                            break; // Break out of the outer loop
                        }
                
                        // Horizontal mirrors
                        for (int mirrorIdy = 0; mirrorIdy < amountHorizontalMirrors; mirrorIdy++)
                        {
                            if (mirrorIdy == originalMirrorIdy) continue;
                            
                            int takeEntries = Math.Min(mirrorIdy + 1, height - mirrorIdy - 1);
                            int startTop = mirrorIdy + 1 + takeEntries < height - 1 ? 0 : mirrorIdy + 1 - takeEntries;
                    
                            var topRows = updatedGrid.Skip(startTop).Take(takeEntries);
                            var bottomRows = updatedGrid.Skip(mirrorIdy + 1).Take(takeEntries).Reverse();
                    
                            bool allEqual = true;
                            using (var enumeratorTop = topRows.GetEnumerator())
                            using (var enumeratorBottom = bottomRows.GetEnumerator())
                            {
                                while (enumeratorTop.MoveNext() && enumeratorBottom.MoveNext())
                                {
                                    if (!enumeratorTop.Current.SequenceEqual(enumeratorBottom.Current))
                                    {
                                        allEqual = false;
                                        break;
                                    }
                                }
                            }
                            
                            if (allEqual)
                            {
                                // They are all equal
                                totalCount += 100 * (mirrorIdy + 1);
                                continueChanging = false;
                                break;
                            }
                        }
                        if (!continueChanging)
                        {
                            break; // Break out of the outer loop
                        }
                    }
                    if (!continueChanging)
                    {
                        break; // Break out of the outer loop
                    }
                }
            }
        }
        return totalCount;
    }
}