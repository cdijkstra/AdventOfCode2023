using System.Collections;
using FluentAssertions;
namespace Crucible;

class Program
{
    static void Main(string[] args)
    {
        var crucible = new Crucible();
        crucible.Solve1("dummydata").Should().Be(102);
    }
}

public enum Direction
{
    N,S,E,W
}

public class Entry
{
    public int X { get; set; }
    public int Y { get; set; }
    public int HeatLoss { get; set; }
    public int TotalHeatLoss { get; set; }
    public Direction Direction { get; set; }
    public int DirectionRepeat { get; set; } // Max 3
}

class Crucible
{
    private List<List<Entry>> _entries = new();
    
    public int Solve1(string fileName)
    {
        var grid = File.ReadAllLines($"Data/{fileName}")
            .Select(line => line.ToCharArray()).ToList()
            .ToList();
        _entries = grid.Select((numbers, rowIndex) =>
            numbers.Select((heatLoss, colIndex) =>
                new Entry
                {
                    X = rowIndex,
                    Y = colIndex,
                    HeatLoss = heatLoss - '0',
                    TotalHeatLoss = 0
                    // DistanceFromExit = CalculateDistanceFromExit(numbers.Length, rowIndex, colIndex)
                }
            ).ToList()
        ).ToList();

        Entry initialEntry = _entries[1][0];
        initialEntry.TotalHeatLoss = 0;
        Queue<Entry> queue = new();
        queue.Enqueue(initialEntry);

        List<Entry> visited = new() { initialEntry };
        List<int> totalHeatLosses = new();
        while (queue.Count > 0)
        {
            var entry = queue.Dequeue();
            foreach (var neighbor in FindValidMoves(entry))
            {
                if (neighbor.X == _entries.Count - 1 && neighbor.Y == _entries[1].Count - 1)
                {
                    totalHeatLosses.Add(entry.TotalHeatLoss + neighbor.HeatLoss);
                }
                else
                {
                    var visitedEntry = visited.SingleOrDefault(visitedEntry => visitedEntry.X == neighbor.X && visitedEntry.Y == neighbor.Y);
                    if (visitedEntry != null)
                    {
                        if (visitedEntry.TotalHeatLoss < entry.TotalHeatLoss + neighbor.HeatLoss) continue;
                        visitedEntry.TotalHeatLoss = entry.TotalHeatLoss + neighbor.HeatLoss;
                        queue.Enqueue(neighbor);
                    }
                    else
                    {
                        neighbor.TotalHeatLoss = entry.TotalHeatLoss + neighbor.HeatLoss;
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
        }
        
        return totalHeatLosses.Min();
    }

    private List<Entry> FindValidMoves(Entry entry)
    {
        List<Entry> neighbors = new();
        if (entry.X > 0 && entry.Direction != Direction.E)
        {
            // Moving west, not allowed from previous east
            var newEntry = _entries[entry.X - 1][entry.Y];
            newEntry.Direction = Direction.W;
            if (entry.Direction != Direction.W)
            {
                newEntry.DirectionRepeat = 1;
                neighbors.Add(newEntry);
            }
            else if (entry.Direction == Direction.W && entry.DirectionRepeat <= 2)
            {
                newEntry.DirectionRepeat = entry.DirectionRepeat + 1;
                neighbors.Add(newEntry);
            }
        }
        if (entry.X < _entries.Count - 1 && entry.Direction != Direction.W)
        {
            // Moving east, not allowed from previous west
            var newEntry = _entries[entry.X + 1][entry.Y];
            newEntry.Direction = Direction.E;
            if (entry.Direction != Direction.E)
            {
                newEntry.DirectionRepeat = 1;
                neighbors.Add(newEntry);
            }
            else if (entry.Direction == Direction.E && entry.DirectionRepeat <= 2)
            {
                newEntry.DirectionRepeat = entry.DirectionRepeat + 1;
                neighbors.Add(newEntry);
            }
        }
        if (entry.Y > 0 && entry.Direction != Direction.S)
        {
            // Moving north, not allowed from previous south
            var newEntry = _entries[entry.X][entry.Y - 1];
            newEntry.Direction = Direction.N;
            if (entry.Direction != Direction.N)
            {
                newEntry.DirectionRepeat = 1;
                neighbors.Add(newEntry);
            }
            else if (entry.Direction == Direction.N && entry.DirectionRepeat <= 2)
            {
                newEntry.DirectionRepeat = entry.DirectionRepeat + 1;
                neighbors.Add(newEntry);
            }
        }
        if (entry.Y < _entries[0].Count - 1 && entry.Direction != Direction.N)
        {
            // Moving south, not allowed from previous north
            var newEntry = _entries[entry.X][entry.Y + 1];
            newEntry.Direction = Direction.S;
            if (entry.Direction != Direction.S)
            {
                newEntry.DirectionRepeat = 1;
                neighbors.Add(newEntry);
            }
            else if (entry.Direction == Direction.S && entry.DirectionRepeat <= 2)
            {
                newEntry.DirectionRepeat = entry.DirectionRepeat + 1;
                neighbors.Add(newEntry);
            }
        }

        return neighbors;
    }
    
    
}