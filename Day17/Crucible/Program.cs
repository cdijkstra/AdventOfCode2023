using FluentAssertions;
namespace Crucible;

class Program
{
    static void Main(string[] args)
    {
        var crucible = new Crucible();
        crucible.Solve1("dummydata");
        crucible.Solve1("data");
    }
}

class Crucible
{
    private List<List<int>> _heatLosses = new();
    private record Vertex(int X, int Y, int Direction, int StepsRemaining);
    private enum Direction { Up = 0, Right = 1, Down = 2, Left = 3 }

    private static readonly int MaxStepsRemaining = 2;
    
    public void Solve1(string fileName)
    {
        var grid = File.ReadAllLines($"Data/{fileName}")
            .Select(line => line.ToCharArray()).ToList()
            .ToList();
        _heatLosses = grid.Select((numbers, rowIndex) =>
            numbers.Select((heatLoss, colIndex) =>
                int.Parse(heatLoss.ToString())
            ).ToList()
        ).ToList();

        var maxX = _heatLosses.Count - 1;
        var maxY = _heatLosses[0].Count - 1;
        
        var visited = new HashSet<Vertex>();
        var distances = new Dictionary<Vertex, int>();
        var entriesToConsider = new HashSet<Vertex>();
        
        var startingVertex = new Vertex(0, 0, 0, 3);
        distances[startingVertex] = 0;
        entriesToConsider.Add(startingVertex);
        
        while (entriesToConsider.Count > 0)
        {
            var current = entriesToConsider.OrderBy(v => distances[v]).First();
            entriesToConsider.Remove(current);
            if (visited.Contains(current))
            {
                continue;
            }

            var distance = distances.GetValueOrDefault(current, int.MaxValue);
            
            // -1 and +1 mean change direction, 0 same direction
            var validNeighbors = Enumerable.Range(-1, 3)
                .Select(directionChange => Step(current, directionChange))
                .Where(ValidEntry());
            
            foreach (var neighbor in validNeighbors)
            {
                if (visited.Contains(neighbor)) continue;
                
                var neighborDistance = distances.GetValueOrDefault(neighbor, int.MaxValue);
                distances[neighbor] = Math.Min(neighborDistance, distance + _heatLosses[neighbor.X][neighbor.Y]);
                entriesToConsider.Add(neighbor);
            }

            if (current.X == maxX && current.Y == maxY)
            {
                Console.WriteLine(distance);
                return;
            }

            visited.Add(current);
        }
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