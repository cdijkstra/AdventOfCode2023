using System.Text.RegularExpressions;
using FluentAssertions;

namespace Gear;

class Program
{
    static void Main()
    {
        var gear = new Gear();
        gear.Solve1("dummydata").Should().Be(4361);
        gear.Solve1("data");
        gear.Solve2("dummydata").Should().Be(467835);
        gear.Solve2("data");
    }
}

class Gear
{
    private int _totalNumber;
    private List<string> _grid = new();
    private List<(int y, int leftX, int rightX, int value)> _numbersInfo = new();
    private List<(int y, int x)> _gearLocations = new();
    public int Solve1(string fileName)
    {
        _totalNumber = 0;
        _grid = new();
        foreach (var line in File.ReadAllLines($"Data/{fileName}"))
        {
            _grid.Add(line);
        }

        for (var rowIdy = 0; rowIdy < _grid.Count; rowIdy++)
        {
            var row = _grid[rowIdy];
            MatchCollection matches = Regex.Matches(row, @"\d+");
            foreach (Match match in matches)
            {
                var leftIndex =  match.Index;
                var rightIndex = leftIndex + match.Length - 1;

                List<(int y, int x)> indexesToConsider = new();
                
                if (leftIndex == 0)
                {
                    if (rowIdy == 0)
                    {
                        foreach (var idx in Enumerable.Range(leftIndex, match.Length + 1))
                        {
                            indexesToConsider.Add((rowIdy + 1, idx));
                        }
                        indexesToConsider.Add((rowIdy, rightIndex + 1));
                    }
                    else if (rowIdy == _grid.Count - 1)
                    {
                        foreach (var idx in Enumerable.Range(leftIndex, match.Length + 1))
                        {
                            indexesToConsider.Add((rowIdy - 1, idx));
                        }
                        indexesToConsider.Add((rowIdy, rightIndex + 1));
                    }
                    else
                    {
                        foreach (var idx in Enumerable.Range(leftIndex, match.Length + 1))
                        {
                            indexesToConsider.Add((rowIdy - 1, idx));
                            indexesToConsider.Add((rowIdy + 1, idx));
                        }
                        indexesToConsider.Add((rowIdy, rightIndex + 1));
                    }
                }
                else if (rightIndex == row.Length - 1)
                {
                    if (rowIdy == 0)
                    {
                        foreach (var idx in Enumerable.Range(leftIndex, match.Length + 1))
                        {
                            indexesToConsider.Add((rowIdy + 1, idx - 1));
                        }
                        indexesToConsider.Add((rowIdy, leftIndex - 1));
                    }
                    else if (rowIdy == _grid.Count - 1)
                    {
                        foreach (var idx in Enumerable.Range(leftIndex, match.Length + 1))
                        {
                            indexesToConsider.Add((rowIdy - 1, idx - 1));
                        }
                        indexesToConsider.Add((rowIdy, leftIndex - 1));
                    }
                    else
                    {
                        foreach (var idx in Enumerable.Range(leftIndex, match.Length + 1))
                        {
                            indexesToConsider.Add((rowIdy - 1, idx - 1));
                            indexesToConsider.Add((rowIdy + 1, idx - 1));
                        }
                        indexesToConsider.Add((rowIdy, leftIndex - 1));
                    }
                }
                else
                {
                    if (rowIdy == 0)
                    {
                        foreach (var idx in Enumerable.Range(leftIndex, match.Length + 2))
                        {
                            indexesToConsider.Add((rowIdy + 1, idx - 1));
                        }
                        indexesToConsider.Add((rowIdy, rightIndex + 1));
                    }
                    else if (rowIdy == _grid.Count - 1)
                    {
                        foreach (var idx in Enumerable.Range(leftIndex, match.Length + 2))
                        {
                            indexesToConsider.Add((rowIdy - 1, idx - 1));
                        }
                        indexesToConsider.Add((rowIdy, leftIndex - 1));
                    }
                    else
                    {
                        foreach (var idx in Enumerable.Range(leftIndex, match.Length + 2))
                        {
                            indexesToConsider.Add((rowIdy - 1, idx - 1));
                            indexesToConsider.Add((rowIdy + 1, idx - 1));
                        }
                        indexesToConsider.Add((rowIdy, leftIndex - 1));
                        indexesToConsider.Add((rowIdy, rightIndex + 1));
                    }
                }
                
                if (indexesToConsider.Any(symbolMatch => !char.IsDigit(_grid.ElementAt(symbolMatch.y).ElementAt(symbolMatch.x)) &&
                    _grid.ElementAt(symbolMatch.y).ElementAt(symbolMatch.x) != '.'))
                {
                    var value = int.Parse(match.Value);
                    _totalNumber += value;
                }
            }
        }

        Console.WriteLine($"Found {_totalNumber}");
        
        return _totalNumber;
    }
    
    public int Solve2(string fileName)
    {
        _totalNumber = 0;
        _grid = new();
        _numbersInfo = new();
        foreach (var line in File.ReadAllLines($"Data/{fileName}"))
        {
            _grid.Add(line);
        }

        for (var rowIdy = 0; rowIdy < _grid.Count; rowIdy++)
        {
            var row = _grid[rowIdy];
            MatchCollection numberMatches = Regex.Matches(row, @"\d+");
            foreach (Match match in numberMatches)
            {
                var leftIndex = match.Index;
                var value = int.Parse(match.Value);
                _numbersInfo.Add((rowIdy, leftIndex, leftIndex + match.Length - 1, value));
            }
            foreach (Match gearMatch in Regex.Matches(row, @"\*"))
            {
                _gearLocations.Add((rowIdy, gearMatch.Index));
            }
        }

        foreach (var gearLocation in _gearLocations)
        {
            var numbersFound = 0;
            List<int> numbers = new();
            foreach (var numberInfo in _numbersInfo)
            {
                var xIndices = Enumerable.Range(numberInfo.leftX, numberInfo.rightX - numberInfo.leftX + 1).ToList();
                if (numberInfo.y == gearLocation.y &&
                    (xIndices.Contains(gearLocation.x - 1) || xIndices.Contains(gearLocation.x + 1)) ||
                    (numberInfo.y == gearLocation.y - 1 || numberInfo.y == gearLocation.y + 1) &&
                    (xIndices.Contains(gearLocation.x - 1) || xIndices.Contains(gearLocation.x) || xIndices.Contains(gearLocation.x + 1)))
                {
                    numbersFound++;
                    numbers.Add(numberInfo.value);
                }
            }

            if (numbersFound == 2)
            {
                _totalNumber += numbers[0] * numbers[1];
            }
        }
        
        Console.WriteLine($"Found {_totalNumber}");
        
        return _totalNumber;
    }
}
