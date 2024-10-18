using FluentAssertions;
using MathNet.Numerics.LinearAlgebra.Double;
using Expr = MathNet.Symbolics.Expression;

namespace HailStones;

class Program
{
    static void Main(string[] args)
    {
        var simulation = new HailSimulation();
        simulation.Solve1("dummydata", 7, 27).Should().Be(2);
        simulation.Solve1("data", 200000000000000, 400000000000000);
        simulation.Solve2("dummydata").Should().Be(47);
        Console.WriteLine(simulation.Solve2("data"));
    }
}

class HailSimulation
{
    public int Solve1(string fileName, long MinPosition, long MaxPosition)
    {
        var hailStones = ReadInput(fileName);
        var collisions = 0;
        
        // Loop over all possible pairs
        for (var i = 0; i < hailStones.Count - 1; i++)
        {
            for (var j = i + 1; j < hailStones.Count; j++)
            {
                var intersection = FindIntersectionPoint(hailStones[i], hailStones[j]);
                if (intersection.X < MinPosition || intersection.X > MaxPosition || 
                    intersection.Y < MinPosition || intersection.Y > MaxPosition ||
                    intersection.T1 < 0 || intersection.T2 < 0) continue;
                collisions++;
            }
        }
        
        return collisions;
    }
    
    public long Solve2(string fileName)
    {
        var hailStones = ReadInput(fileName);
        
        List<long> finalSums = new();
        for (var idx = 0; idx != hailStones.Count - 2; idx++)
        {
            // Running this algorithm once should suffice...
            // However the answers differ based on which hailstones are taken due to rounding errors.
            var h1 = hailStones[idx]; var h2 = hailStones[idx + 1]; var h3 = hailStones[idx + 2];

            var A = DenseMatrix.OfArray(new double[,]
            {
                { h1.Vy - h2.Vy, h2.Vx - h1.Vx, 0, h2.Y - h1.Y, h1.X - h2.X, 0 },
                { h1.Vy - h3.Vy, h3.Vx - h1.Vx, 0, h3.Y - h1.Y, h1.X - h3.X, 0 },
                { 0, h1.Vz - h2.Vz, h2.Vy - h1.Vy, 0, h2.Z - h1.Z, h1.Y - h2.Y },
                { 0, h1.Vz - h3.Vz, h3.Vy - h1.Vy, 0, h3.Z - h1.Z, h1.Y - h3.Y },
                { h1.Vz - h2.Vz, 0, h2.Vx - h1.Vx, h2.Z - h1.Z, 0, h1.X - h2.X },
                { h1.Vz - h3.Vz, 0, h3.Vx - h1.Vx, h3.Z - h1.Z, 0, h1.X - h3.X }
            });

            var B = DenseVector.OfArray(new double[]
            {
                h1.X * h1.Vy - h1.Y * h1.Vx - (h2.X * h2.Vy - h2.Y * h2.Vx),
                h1.X * h1.Vy - h1.Y * h1.Vx - (h3.X * h3.Vy - h3.Y * h3.Vx),
                h1.Y * h1.Vz - h1.Z * h1.Vy - (h2.Y * h2.Vz - h2.Z * h2.Vy),
                h1.Y * h1.Vz - h1.Z * h1.Vy - (h3.Y * h3.Vz - h3.Z * h3.Vy),
                h1.X * h1.Vz - h1.Z * h1.Vx - (h2.X * h2.Vz - h2.Z * h2.Vx),
                h1.X * h1.Vz - h1.Z * h1.Vx - (h3.X * h3.Vz - h3.Z * h3.Vx),
            });

            var aInverse = A.Inverse();
            var resultVector = aInverse * B;

            var roundedValues = resultVector.Take(3)
                .Select(v => (long)Math.Round(v)).ToList();
            
            finalSums.Add(roundedValues.Sum());
        }

        // Take the most occurring value in the list!
        var mostOccurringItem = finalSums
            .GroupBy(x => x).MaxBy(g => g.Count())!.Key;
        
        return mostOccurringItem;
    }
    
    private static List<Hail> ReadInput(string fileName)
    {
        List<Hail> hails = File.ReadAllLines($"Data/{fileName}").Select(line => new Hail()
        {
            X = long.Parse(line.Split("@")[0].Split(", ")[0]),
            Y = long.Parse(line.Split("@")[0].Split(", ")[1]),
            Z = long.Parse(line.Split("@")[0].Split(", ")[2]),
            Vx = long.Parse(line.Split("@")[1].Split(", ")[0]),
            Vy = long.Parse(line.Split("@")[1].Split(", ")[1]),
            Vz = long.Parse(line.Split("@")[1].Split(", ")[2]),
        }).ToList();
        return hails;
    }

    static (double X, double Y, double T1, double T2) FindIntersectionPoint(Hail first, Hail second)
    {
        // Calculate the denominator of the expressions for t and s
        double denominator = first.Vx * second.Vy - first.Vy * second.Vx;

        // Check if the lines are parallel
        if (Math.Abs(denominator) < double.Epsilon)
        {
            // Lines are parallel, handle this case (e.g., return a special value or throw an exception)
            return (int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
        }

        // Calculate the numerator for t
        double t1 = ((second.X - first.X) * second.Vy - (second.Y - first.Y) * second.Vx) / denominator;

        // Calculate the intersection point
        double intersectionX = first.X + t1 * first.Vx;
        double intersectionY = first.Y + t1 * first.Vy;
        
        // Now find out if the time was future or past for the second hailstone.
        double t2 = (intersectionX - second.X) / second.Vx;
        
        return (intersectionX, intersectionY, t1, t2);
    }
}
