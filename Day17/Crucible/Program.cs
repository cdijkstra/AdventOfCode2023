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
}

public class PathWithHistory(Entry entry, int priority, List<Entry> visited) : IComparable<PathWithHistory>
{
    public Entry Entry { get; } = entry;
    public int Priority { get; } = priority;
    public List<Entry> Visited { get; } = visited;

    public int CompareTo(PathWithHistory? other)
    {
        return Priority.CompareTo(other.Priority);
    }
}

class PriorityQueue<T> where T : IComparable<T>
{
    private readonly List<T> heap = new();
    public int Count => heap.Count;

    public void Enqueue(T item)
    {
        heap.Add(item);
        int i = heap.Count - 1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (heap[parent].CompareTo(heap[i]) <= 0)
                break;

            Swap(parent, i);
            i = parent;
        }
    }

    public T Dequeue()
    {
        if (heap.Count == 0)
            throw new InvalidOperationException("PriorityQueue is empty.");

        T result = heap[0];
        int lastIndex = heap.Count - 1;
        heap[0] = heap[lastIndex];
        heap.RemoveAt(lastIndex);

        int i = 0;
        while (true)
        {
            int leftChild = 2 * i + 1;
            int rightChild = 2 * i + 2;
            int smallest = i;

            if (leftChild < heap.Count && heap[leftChild].CompareTo(heap[smallest]) < 0)
                smallest = leftChild;

            if (rightChild < heap.Count && heap[rightChild].CompareTo(heap[smallest]) < 0)
                smallest = rightChild;

            if (smallest == i)
                break;

            Swap(i, smallest);
            i = smallest;
        }

        return result;
    }

    private void Swap(int i, int j)
    {
        (heap[i], heap[j]) = (heap[j], heap[i]);
    }
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
                    // DistanceFromExit = CalculateDistanceFromExit(numbers.Length, rowIndex, colIndex)
                }
            ).ToList()
        ).ToList();

        var firstEntry = _entries[0][0];
        var priority = firstEntry.HeatLoss;
        List<Entry> visited = new();
        var path = new PathWithHistory(firstEntry, priority, visited);
        PriorityQueue<PathWithHistory> prioQueue = new PriorityQueue<PathWithHistory>();
        prioQueue.Enqueue(path);
        while (prioQueue.Count > 0)
        {
            var pathWithHistory = prioQueue.Dequeue();
            if (pathWithHistory.Visited.Contains(pathWithHistory.Entry)) continue;
            pathWithHistory.Visited.Add(pathWithHistory.Entry);
            if (pathWithHistory.Entry.X == _entries.Count - 1 && pathWithHistory.Entry.Y == _entries[0].Count - 1)
            {
                // End is found
                return pathWithHistory.Priority;
            }
                
            var neighbors = FindNeighbors(pathWithHistory.Entry);
            var validNeighbors = neighbors.Where(neighbor => !pathWithHistory.Visited.Contains(neighbor)).ToList();
            foreach (var validNeighbor in validNeighbors)
            {
                prioQueue.Enqueue(new PathWithHistory(validNeighbor, pathWithHistory.Priority + validNeighbor.HeatLoss, pathWithHistory.Visited));
            }
        }
        
        return 5;
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