using FluentAssertions;

namespace Crucible;

class Program
{
    static void Main(string[] args)
    {
        var crucible = new Crucible();
        crucible.Solve1("dummydata", 3).Should().Be(102);
        crucible.Solve1("data", 3);
        crucible.Solve2("dummydata", 10).Should().Be(94);
        crucible.Solve2("dummydata2", 10).Should().Be(71);
        crucible.Solve2("data", 10);
        // 931 is too high
    }
}

class Crucible
{
    private List<List<int>> _heatLosses = new();
    private record Vertex(int X, int Y, int Direction, int StepsRemaining);
    private enum Direction { Right = 0, Down = 1, Left = 2, Up = 3 }
    private int MaxSteps;
    
    public int Solve1(string fileName, int maxSteps)
    {
        MaxSteps = maxSteps;
        
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
        
        var startingVertex = new Vertex(0, 0, 0, maxSteps);
        totalHeatLoss[startingVertex] = 0;

        var priorityQueue = new PriorityQueue<Vertex, int>();
        priorityQueue.Enqueue(startingVertex, totalHeatLoss[startingVertex]);
        while (priorityQueue.Count > 0)
        {
            var current = priorityQueue.Dequeue();
            if (visited.Contains(current)) continue;

            var distance = totalHeatLoss.GetValueOrDefault(current, int.MaxValue);
            
            // -1 and +1 mean change direction, 0 same direction
            var allNeighbors = Enumerable.Range(-1, 3)
                .Select(directionChange => StepCrucible(current, directionChange));
            
            var validNeighbors = allNeighbors.Where(ValidEntryCrucible());
            
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
    
    public int Solve2(string fileName, int maxSteps)
    {
        MaxSteps = maxSteps - 1;
        
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
        
        // Start explicitly in both possible directions, due to constraint of not turning before 4 steps
        var startingVertex = new Vertex(0, 0, 0, maxSteps);
        var startingVertex2 = new Vertex(0, 0, 1, maxSteps);
        totalHeatLoss[startingVertex] = 0;
        totalHeatLoss[startingVertex2] = 0;

        var priorityQueue = new PriorityQueue<Vertex, int>();
        priorityQueue.Enqueue(startingVertex, totalHeatLoss[startingVertex]);
        priorityQueue.Enqueue(startingVertex2, totalHeatLoss[startingVertex2]);
        
        while (priorityQueue.Count > 0)
        {
            var current = priorityQueue.Dequeue();
            if (visited.Contains(current)) continue;

            var distance = totalHeatLoss.GetValueOrDefault(current, int.MaxValue);
            
            // -1 and +1 mean change direction, 0 same direction.
            // So Enumerable.Range(0,1) means go straight
            var allNeighbors = current.StepsRemaining > MaxSteps - 4
                ? Enumerable.Repeat(StepCrucible(current, 0), 1)
                : Enumerable.Range(-1, 3)
                    .Select(directionChange => StepCrucible(current, directionChange));
            
            var validNeighbors = allNeighbors.Where(ValidEntryCrucible());
            
            foreach (var neighbor in validNeighbors)
            {
                if (visited.Contains(neighbor)) continue;
                
                var neighborHeatloss = totalHeatLoss.GetValueOrDefault(neighbor, int.MaxValue);
                totalHeatLoss[neighbor] = Math.Min(neighborHeatloss, distance + _heatLosses[neighbor.X][neighbor.Y]);
                priorityQueue.Enqueue(neighbor, totalHeatLoss[neighbor]);
            }

            visited.Add(current);
            if (current.X != maxX || current.Y != maxY || current.StepsRemaining > MaxSteps - 4) continue;
            
            Console.WriteLine(distance);
            return distance;
        }
        return -1;
    }

    private Func<Vertex, bool> ValidEntryCrucible()
    {
        return newVertex => 0 <= newVertex.X && newVertex.X < _heatLosses.Count &&
                            0 <= newVertex.Y && newVertex.Y < _heatLosses[0].Count &&
                            newVertex.StepsRemaining >= 0;
    }

    private Vertex StepCrucible(Vertex vertex, int directionChange)
    {
        var directionNumber = (vertex.Direction + directionChange + 4) % 4;
        Direction newDirection = (Direction)directionNumber;
        
        var xDelta = newDirection switch
        {
            Direction.Right => 0,
            Direction.Down => 1,
            Direction.Left => 0,
            Direction.Up => -1
        };
        var yDelta = newDirection switch
        {
            Direction.Right => 1,
            Direction.Down => 0,
            Direction.Left => -1,
            Direction.Up => 0
        };
        
        var newVertex = vertex with
        {
            X = vertex.X + xDelta,
            Y = vertex.Y + yDelta,
            Direction = directionNumber,
            StepsRemaining = vertex.Direction == directionNumber ? vertex.StepsRemaining - 1 : MaxSteps - 1
        };
        return newVertex;
    }
}