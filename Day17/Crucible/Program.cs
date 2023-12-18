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

public class Entry
{
    public int X { get; set; }
    public int Y { get; set; }
    public int HeatLoss { get; set; }
    public int TotalHeatLoss { get; set; }
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

        Entry initialEntry = _entries[0][0];
        initialEntry.TotalHeatLoss = initialEntry.HeatLoss;
        Queue<Entry> queue = new();
        queue.Enqueue(initialEntry);

        List<Entry> visited = new() { initialEntry };

        while (queue.Count > 0)
        {
            var entry = queue.Dequeue();
            foreach (var neighbor in FindNeighbors(entry))
            {
                if (neighbor.X == _entries.Count - 1 && neighbor.Y == _entries[1].Count - 1)
                {
                    return neighbor.TotalHeatLoss;
                }

                var visitedEntry = visited.SingleOrDefault(visitedEntry =>
                    visitedEntry.X == neighbor.X && visitedEntry.Y == neighbor.Y);
                if (visitedEntry != null)
                {
                    if (visitedEntry.TotalHeatLoss < neighbor.TotalHeatLoss)
                    {
                        visitedEntry.TotalHeatLoss = neighbor.TotalHeatLoss;
                        queue.Enqueue(visitedEntry);
                    }
                }
                else
                {
                    neighbor.TotalHeatLoss = neighbor.HeatLoss + entry.TotalHeatLoss;
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }
        
        return -1;
    }

    private List<Entry> FindNeighbors(Entry entry)
    {
        List<Entry> neighbors = new();
        if (entry.X > 0)
        {
            neighbors.Add(_entries[entry.X - 1][entry.Y]);
        }
        if (entry.X < _entries.Count - 1)
        {
            neighbors.Add(_entries[entry.X + 1][entry.Y]);
        }
        
        if (entry.Y > 0)
        {
            neighbors.Add(_entries[entry.X][entry.Y - 1]);
        }
        if (entry.Y < _entries[0].Count - 1)
        {
            neighbors.Add(_entries[entry.X][entry.Y + 1]);
        }

        return neighbors;
    }
    
    
}