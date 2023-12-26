using FluentAssertions;
namespace Crucible;

class Program
{
    static void Main(string[] args)
    {
        var crucible = new Crucible();
        crucible.Solve1("dummydata").Should().Be(102);
        Console.WriteLine(crucible.Solve1("data"));
    }
}

public enum Direction
{
    N,S,E,W,Unknown
}

public class Entry
{
    public int X { get; set; }
    public int Y { get; set; }
    public int HeatLoss { get; set; }
    public int TotalHeatLoss { get; set; }
    public Direction Direction { get; set; }
    public int DirectionRepeat { get; set; } // Max 3
    public List<Entry> History { get; set; } = new();
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
                    TotalHeatLoss = 0,
                    Direction = Direction.Unknown
                }
            ).ToList()
        ).ToList();

        Entry initialEntry = new Entry()
        {
            X = 0,
            Y = 0,
            TotalHeatLoss = 0,
            Direction = Direction.Unknown
        };
        PriorityQueue<Entry, int> queue = new();
        queue.Enqueue(initialEntry, 0);

        List<Entry> visited = new() { initialEntry };
        var maxX = _entries.Count - 1;
        var maxY = _entries[1].Count - 1;
        while (queue.Count > 0)
        {
            var entry = queue.Dequeue();
            Console.WriteLine($"{entry.TotalHeatLoss} at ({entry.X},{entry.Y})");
            foreach (var neighbor in FindValidMoves(entry))
            {
                if (neighbor.X == maxX && neighbor.Y == maxY)
                {
                    return neighbor.TotalHeatLoss;
                }
                
                var visitedEntry = visited.Where(visitedEntry => 
                    visitedEntry.X == neighbor.X && visitedEntry.Y == neighbor.Y && 
                    visitedEntry.Direction == neighbor.Direction && visitedEntry.DirectionRepeat == neighbor.DirectionRepeat);
                if (visitedEntry.Any(en => en.TotalHeatLoss < neighbor.TotalHeatLoss)) continue;
                
                // Take sooner from queue for closer to solution;
                var priority = neighbor.TotalHeatLoss + 4 * (maxX - neighbor.X + maxY - neighbor.Y);
                queue.Enqueue(neighbor, priority);
                visited.Add(neighbor);
            }
        }

        return -1;
    }

    private List<Entry> FindValidMoves(Entry entry)
    {
        List<Entry> neighbors = new();
        if (entry.X > 0 && entry.Direction != Direction.S) //  && entry.Direction != Direction.S
        { // Moving north, not allowed from previous south
            var newEntry = new Entry()
            {
                X = entry.X - 1,
                Y = entry.Y,
                TotalHeatLoss = entry.TotalHeatLoss + _entries[entry.X - 1][entry.Y].HeatLoss,
                Direction = Direction.N
            };
            newEntry.History = entry.History.Concat(new List<Entry> { newEntry }).ToList();
            
            if (entry is { Direction: Direction.N, DirectionRepeat: <= 2 })
            {
                newEntry.DirectionRepeat = entry.DirectionRepeat + 1;
                neighbors.Add(newEntry);
            }
            else if (entry.Direction != Direction.N)
            {
                newEntry.DirectionRepeat = 1;
                neighbors.Add(newEntry);
            }
        }
        if (entry.X < _entries.Count - 1 && entry.Direction != Direction.N)
        {
            // Moving south, not allowed from previous north
            var newEntry = new Entry()
            {
                X = entry.X + 1,
                Y = entry.Y,
                TotalHeatLoss = entry.TotalHeatLoss + _entries[entry.X + 1][entry.Y].HeatLoss,
                Direction = Direction.S
            };
            newEntry.History = entry.History.Concat(new List<Entry> { newEntry }).ToList();
            if (entry is { Direction: Direction.S, DirectionRepeat: <= 2 })
            {
                newEntry.DirectionRepeat = entry.DirectionRepeat + 1;
                neighbors.Add(newEntry);
            }
            else if (entry.Direction != Direction.S)
            {
                newEntry.DirectionRepeat = 1;
                neighbors.Add(newEntry);
            }
        }
        if (entry.Y > 0 && entry.Direction != Direction.E)
        {
            // Moving west, not allowed from previous east
            var newEntry = new Entry()
            {
                X = entry.X,
                Y = entry.Y - 1,
                TotalHeatLoss = entry.TotalHeatLoss + _entries[entry.X][entry.Y - 1].HeatLoss,
                Direction = Direction.W
            };
            newEntry.History = entry.History.Concat(new List<Entry> { newEntry }).ToList();

            if (entry is { Direction: Direction.W, DirectionRepeat: <= 2 })
            {
                newEntry.DirectionRepeat = entry.DirectionRepeat + 1;
                neighbors.Add(newEntry);
            }
            else if (entry.Direction != Direction.W)
            {
                newEntry.DirectionRepeat = 1;
                neighbors.Add(newEntry);
            }
        }
        if (entry.Y < _entries[0].Count - 1 && entry.Direction != Direction.W)
        {
            // Moving east, not allowed from previous west
            var newEntry = new Entry()
            {
                X = entry.X,
                Y = entry.Y + 1,
                TotalHeatLoss = entry.TotalHeatLoss + _entries[entry.X][entry.Y + 1].HeatLoss,
                Direction = Direction.E
            };
            newEntry.History = entry.History.Concat(new List<Entry> { newEntry }).ToList();
            
            if (entry is { Direction: Direction.E, DirectionRepeat: <= 2 })
            {
                newEntry.DirectionRepeat = entry.DirectionRepeat + 1;
                neighbors.Add(newEntry);
            }
            else if (entry.Direction != Direction.E)
            {
                newEntry.DirectionRepeat = 1;
                neighbors.Add(newEntry);
            }
        }

        return neighbors;
    }
}