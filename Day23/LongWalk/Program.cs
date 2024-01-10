using FluentAssertions;
namespace LongWalk;

class Program
{
    static void Main(string[] args)
    {
        var maze = new Maze();
        maze.Solve1("dummydata").Should().Be(94);
        Console.WriteLine(maze.Solve1("data"));
        maze.Solve2("dummydata").Should().Be(154);
        Console.WriteLine(maze.Solve2("data"));
    }
}

class Maze
{
    private enum Direction { N,S,W,E }
    private List<List<char>> _grid = new();
    private List<int> _pathLengths = new();
    private readonly List<char> ValidLeft = new() { '.', '<' };
    private readonly List<char> ValidUp = new() { '.', '^' };
    private readonly List<char> ValidRight = new() { '.', '>' };
    private readonly List<char> ValidDown = new() { '.', 'v' };
    
    private readonly List<char> ValidChars = new() { '.', '<', '^', '>', 'v' };
    private (int x, int y) _startCoordinates = (0, 0);
    private (int x, int y) _endCoordinates = (0, 0);
    private Dictionary<(int x, int y), List<(int x, int y, int distance)>> _specialPointToSpecialPoints = new();
    
    public int Solve1(string fileName)
    {
        Init(fileName);
        TraversePath(_startCoordinates, 0, Direction.S);
        return _pathLengths.Max();
    }

    private void Init(string fileName)
    {
        _pathLengths = new();
        _grid = File.ReadAllLines($"Data/{fileName}")
            .Select(line => line.ToList())
            .ToList();

        _startCoordinates = (0, _grid[0].IndexOf('.'));
        _endCoordinates = (_grid.Count - 1, _grid[^1].IndexOf('.'));
    }

    public int Solve2(string fileName)
    {
        Init(fileName);

        var crossRoads = Enumerable.Range(0, _grid.Count)
            .SelectMany(x => Enumerable.Range(0, _grid[0].Count), (x, y) => (x, y))
            .Where(el => _grid[el.x][el.y] == '.' && GetAdjacentChars(el.x, el.y).Count(el => ValidChars.Contains(el)) >= 3)
            .Concat(new[] { _startCoordinates, _endCoordinates }) // Add start and endpoints also as points of interest
            .ToList();

        _specialPointToSpecialPoints =
            crossRoads.ToDictionary(
                point => (point.x, point.y),
                point => new List<(int x, int y, int distance)>());
        
        foreach ((int x, int y) crossRoad in crossRoads)
        {
            foreach (var neighbor in GetValidAdjacentCoordinates(crossRoad.x, crossRoad.y)
                         .Where(el => ValidChars.Contains(GetChar(el.x, el.y))))
            {
                List<(int x, int y)> visited = new() { crossRoad };
                var continueLoop = true;
                var steps = 0;
                var currentPos = neighbor;
                while (continueLoop)
                {
                    steps++;
                    if (!crossRoads.Contains(currentPos))
                    {
                        var nextMove = GetValidAdjacentCoordinates(currentPos.x, currentPos.y)
                            .Where(el => ValidChars.Contains(GetChar(el.x, el.y)) && 
                                         !visited.Contains(el)).ToList();
                        visited.Add(currentPos);
                        currentPos = nextMove[0];
                    }
                    else
                    {
                        continueLoop = false;
                        _specialPointToSpecialPoints[crossRoad].Add((currentPos.x, currentPos.y, steps));
                    }
                }
            }
        }
        
        CalculateLengths(_startCoordinates, new() { _startCoordinates }, 0);
        return _pathLengths.Max();
    }

    private void CalculateLengths((int x, int y) position, List<(int x, int y)> visited, int totalLength)
    {
        var newPoints = _specialPointToSpecialPoints[position].Where(point => !visited.Contains((point.x, point.y)));
        foreach (var newPoint in newPoints)
        {
            if (newPoint.x == _endCoordinates.x && newPoint.y == _endCoordinates.y)
            {
                _pathLengths.Add(totalLength + newPoint.distance);
            }
            else
            {
                var newVisited = new List<(int x, int y)>(visited);
                newVisited.Add((newPoint.x, newPoint.y));
                CalculateLengths((newPoint.x, newPoint.y), newVisited, totalLength + newPoint.distance);
            }
        }
    }
    
    private char GetChar(int i, int j)
    {
        // Helper method to safely get a point from the grid
        return i >= 0 && i < _grid.Count && j >= 0 && j < _grid[0].Count ? _grid[i][j] : 'X';
    }

    private char[] GetAdjacentChars(int i, int j)
    {
        // Get characters of adjacent points in the grid
        return new[]
        {
            GetChar(i - 1, j), // Up
            GetChar(i + 1, j), // Down
            GetChar(i, j - 1), // Left
            GetChar(i, j + 1)  // Right
        };
    }
    
    private List<(int x, int y)> GetValidAdjacentCoordinates(int i, int j)
    {
        List<(int x, int y)> coors = new();
        (int x, int y)[] directions = { (-1, 0), (1, 0), (0, -1), (0, 1) };

        foreach (var direction in directions)
        {
            int xNeighbor = i + direction.x;
            int yNeighbor = j + direction.y;
            if (GetChar(xNeighbor, yNeighbor) == 'X') continue;
            coors.Add((xNeighbor, yNeighbor));
        }

        return coors;
    }

    private void TraversePath((int x, int y) coordinates, int steps, Direction direction)
    {
        if (coordinates == _endCoordinates)
        {
            _pathLengths.Add(steps);
            return;
        }
        
        (int x, int y) leftCoordinates = (coordinates.x, coordinates.y - 1);
        (int x, int y) rightCoordinates = (coordinates.x, coordinates.y + 1);
        (int x, int y) upCoordinates = (coordinates.x - 1, coordinates.y);
        (int x, int y) downCoordinates = (coordinates.x + 1, coordinates.y);
        
        switch (direction)
        {
            case Direction.N:
                CheckUp(upCoordinates, steps);
                CheckLeft(upCoordinates, steps);
                CheckRight(upCoordinates, steps);
                break;
            case Direction.E:
                CheckRight(rightCoordinates, steps);
                CheckUp(rightCoordinates, steps);
                CheckDown(rightCoordinates, steps);
                break;
            case Direction.S:
                CheckDown(downCoordinates, steps);
                CheckLeft(downCoordinates, steps);
                CheckRight(downCoordinates, steps);
                break;
            case Direction.W:
                CheckLeft(leftCoordinates, steps);
                CheckUp(leftCoordinates, steps);
                CheckDown(leftCoordinates, steps);
                break;
        }
    }

    private void CheckLeft((int x, int y) leftCoordinates, int steps)
    {
        if (leftCoordinates.y < 0 || !ValidLeft.Contains(_grid[leftCoordinates.x][leftCoordinates.y])) return;
        TraversePath(leftCoordinates, steps + 1, Direction.W);
    }
    
    private void CheckRight((int x, int y) rightCoordinates, int steps)
    {
        if (rightCoordinates.y > _grid[0].Count - 1 || !ValidRight.Contains(_grid[rightCoordinates.x][rightCoordinates.y])) return;
        TraversePath(rightCoordinates, steps + 1, Direction.E);
    }
    
    private void CheckUp((int x, int y) upCoordinates, int steps)
    {
        if (upCoordinates.x < 0 || !ValidUp.Contains(_grid[upCoordinates.x][upCoordinates.y])) return;
        TraversePath(upCoordinates, steps + 1, Direction.N);
    }
    
    private void CheckDown((int x, int y) downCoordinates, int steps)
    {
        if (downCoordinates.x > _grid.Count - 1 || !ValidDown.Contains(_grid[downCoordinates.x][downCoordinates.y])) return;
        TraversePath(downCoordinates, steps + 1, Direction.S);
    }
    
    private void CheckLeft2((int x, int y) leftCoordinates, int steps)
    {
        if (leftCoordinates.y < 0 || !ValidChars.Contains(_grid[leftCoordinates.x][leftCoordinates.y])) return;
        TraversePath(leftCoordinates, steps + 1, Direction.W);
    }
    
    private void CheckRight2((int x, int y) rightCoordinates, int steps)
    {
        if (rightCoordinates.y > _grid[0].Count - 1 || !ValidChars.Contains(_grid[rightCoordinates.x][rightCoordinates.y])) return;
        TraversePath(rightCoordinates, steps + 1, Direction.E);
    }
    
    private void CheckUp2((int x, int y) upCoordinates, int steps)
    {
        if (upCoordinates.x < 0 || !ValidChars.Contains(_grid[upCoordinates.x][upCoordinates.y])) return;
        TraversePath(upCoordinates, steps + 1, Direction.N);
    }
    
    private void CheckDown2((int x, int y) downCoordinates, int steps)
    {
        if (downCoordinates.x > _grid.Count - 1 || !ValidChars.Contains(_grid[downCoordinates.x][downCoordinates.y])) return;
        TraversePath(downCoordinates, steps + 1, Direction.S);
    }
}