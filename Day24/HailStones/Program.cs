using FluentAssertions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.RootFinding;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.Expression;

namespace HailStones;

class Program
{
    static void Main(string[] args)
    {
        var simulation = new HailSimulation();
        simulation.Solve1("dummydata", 7, 27).Should().Be(2);
        
        simulation.Solve1("data", 200000000000000, 400000000000000);
        simulation.Solve2("dummydata");
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
    
    public int Solve2(string fileName)
    {
        var hailStones = ReadInput(fileName);
        
        var initialGuess = new double[] { 0, 0, 0, 0, 0, 0 }; // x,y,z,vx,vy,vz
        
        // Function to evaluate the system of equations numerically
        Func<double[], double[]> function = variables =>
        {
            double xr = variables[0];
            double yr = variables[1];
            double zr = variables[2];
            double vxr = variables[3];
            double vyr = variables[4];
            double vzr = variables[5];
    
            var amountOfHailStonesToConsider = 3;
            var results = new double[2 * amountOfHailStonesToConsider];
            int index = 0;
    
            foreach (var hailStone in hailStones[..amountOfHailStonesToConsider])
            {
                results[index++] = (xr - hailStone.X) * (hailStone.Vy - vyr) - (yr - hailStone.Y) * (hailStone.Vx - vxr);
                results[index++] = (yr - hailStone.Y) * (hailStone.Vz - vzr) - (zr - hailStone.Z) * (hailStone.Vy - vyr);
            }
    
            return results;
        };
        // Solve the equations
        
        double tolerance = 1e-6;
        int maxIterations = 100;
        
        var result = Broyden.FindRoot(function, initialGuess, tolerance, maxIterations);
        return (int)result[0];
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
