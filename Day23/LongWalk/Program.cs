using FluentAssertions;

namespace LongWalk;

class Program
{
    static void Main(string[] args)
    {
        var maze = new Maze();
        maze.Solve1("dummydata").Should().Be(94);
        Console.Write(maze.Solve1("data"));
    }
}

public enum Direction
{
    N,S,W,E
}

class Maze
{
    private List<List<char>> _grid = new();
    private List<int> _pathLenths = new();
    private readonly List<char> ValidLeft = new() { '.', '<' };
    private readonly List<char> ValidUp = new() { '.', '^' };
    private readonly List<char> ValidRight = new() { '.', '>' };
    private readonly List<char> ValidDown = new() { '.', 'v' };
    private (int, int) _endCoordinates = (0, 0);
    
    public int Solve1(string fileName)
    {
        _grid = File.ReadAllLines($"Data/{fileName}")
            .Select(line => line.ToList())
            .ToList();

        var startIndex = (0, _grid[0].IndexOf('.'));
        _endCoordinates = (_grid.Count - 1, _grid[^1].IndexOf('.'));

        TraversePath(startIndex, 0, Direction.S);
        
        return _pathLenths.Max();
    }

    private void TraversePath((int x, int y) coordinates, int steps, Direction direction)
    {
        if (coordinates == _endCoordinates)
        {
            _pathLenths.Add(steps);
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
}