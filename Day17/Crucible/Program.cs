using FluentAssertions;

namespace Crucible;

class Program
{
    static void Main(string[] args)
    {
        var crucible = new Crucible();
        crucible.Solve1("dummydata").Should().Be(102);
        crucible.Solve1("data");
    }
}

class Crucible
{
    private List<List<int>> _heatLosses = new();
    private record Vertex(int X, int Y, int Direction, int StepsRemaining);
    private enum Direction { Up = 0, Right = 1, Down = 2, Left = 3 }
    private static readonly int MaxStepsRemaining = 2;
    
    public int Solve1(string fileName)
    {
        _heatLosses = File.ReadAllLines($"Data/{fileName}")
            .Select(line => line.ToCharArray()).ToList()
            .Select((numbers, rowIndex) =>
            numbers.Select((heatLoss, colIndex) =>
                int.Parse(heatLoss.ToString())
            ).ToList()
        ).ToList();

        var maxX = _heatLosses.Count - 1;
        var maxY = _heatLosses[0].Count - 1;
        
        var visited = new HashSet<Vertex>();
        var totalHeatLoss = new Dictionary<Vertex, int>();
        
        var startingVertex = new Vertex(0, 0, 0, 3);
        totalHeatLoss[startingVertex] = 0;

        var priorityQueue = new PriorityQueue<Vertex, int>();
        priorityQueue.Enqueue(startingVertex, totalHeatLoss[startingVertex]);
        while (priorityQueue.Count > 0)
        {
            var current = priorityQueue.Dequeue();
            if (visited.Contains(current)) continue;

            var distance = totalHeatLoss.GetValueOrDefault(current, int.MaxValue);
            
            // -1 and +1 mean change direction, 0 same direction
            var validNeighbors = Enumerable.Range(-1, 3)
                .Select(directionChange => Step(current, directionChange))
                .Where(ValidEntry());
            
            foreach (var neighbor in validNeighbors)
            {
                if (visited.Contains(neighbor)) continue;
                
                var neighborDistance = totalHeatLoss.GetValueOrDefault(neighbor, int.MaxValue);
                totalHeatLoss[neighbor] = Math.Min(neighborDistance, distance + _heatLosses[neighbor.X][neighbor.Y]);
                priorityQueue.Enqueue(neighbor, totalHeatLoss[neighbor]);
            }

            visited.Add(current);
            if (current.X != maxX || current.Y != maxY) continue;
            
            Console.WriteLine(distance);
            return distance;
        }
        return -1;
    }

    private Func<Vertex, bool> ValidEntry()
    {
        return newVertex => 0 <= newVertex.X && newVertex.X < _heatLosses.Count &&
                            0 <= newVertex.Y && newVertex.Y < _heatLosses[0].Count &&
                            newVertex.StepsRemaining >= 0;
    }

    private static Vertex Step(Vertex vertex, int directionChange)
    {
        var directionNumber = (vertex.Direction + directionChange + 4) % 4;
        Direction newDirection = (Direction)directionNumber;
        
        var xDelta = newDirection switch
        {
            Direction.Up => 0,
            Direction.Right => 1,
            Direction.Down => 0,
            Direction.Left => -1
        };
        var yDelta = newDirection switch
        {
            Direction.Up => 1,
            Direction.Right => 0,
            Direction.Down => -1,
            Direction.Left => 0
        };
        
        var newVertex = vertex with
        {
            X = vertex.X + xDelta,
            Y = vertex.Y + yDelta,
            Direction = directionNumber,
            StepsRemaining = vertex.Direction == directionNumber ? vertex.StepsRemaining - 1 : MaxStepsRemaining
        };
        return newVertex;
    }
}