using FluentAssertions;
namespace Mirage;

class Program
{
    static void Main(string[] args)
    {
        var mirage = new Mirage();
        mirage.Solve1("dummydata").Should().Be(114);
        Console.WriteLine(mirage.Solve1("data"));
        mirage.Solve2("dummydata").Should().Be(2);
        Console.WriteLine(mirage.Solve2("data"));
    }
}

class Mirage
{
    private int _result;
    public int Solve1(string fileName)
    {
        return File.ReadAllLines($"Data/{fileName}")
            .Select(line => FindDerivativesEqualToZero(line))
            .SelectMany(derivatives => derivatives.Select(row => row.Last()))
            .Sum();
    }
    
    public int Solve2(string fileName)
    {
        return File.ReadAllLines($"Data/{fileName}")
            .Select(line => FindDerivativesEqualToZero(line))
            .Select(derivatives =>
            {
                int leftNumber = 0;
                // Now do the math. We can obtain the missing number by working backwards with derivatives
                // calculatedNumber starts at 0 and we have to go in reverse over rows updating calculcatedNumber = firstEntry - calculcatedNumber
                return derivatives
                    .Reverse<List<int>>()
                    .Select(row => row.First() - leftNumber)
                    .Aggregate((acc, val) => leftNumber = val);
            }).Sum();
    }

    private static List<List<int>> FindDerivativesEqualToZero(string line)
    {
        var initialSequence = line.Split().Select(x => int.Parse(x)).ToList();
        List<List<int>> derivatives = new List<List<int>> { initialSequence };

        // Start calculating derivatives until they are all 0
        while (derivatives.Last().Any(dif => dif != 0))
        {
            var sequence = derivatives.Last();
            var differences = sequence
                .Skip(1) // Skip the first element since there is no element before it
                .Select((current, index) => current - sequence[index]);
            derivatives.Add(differences.ToList());
        }

        return derivatives;
    }
}