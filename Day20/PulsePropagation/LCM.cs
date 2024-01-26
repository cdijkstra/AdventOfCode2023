namespace PulsePropagation;

public static class LCM
{
  public static long CalculateLCM(int[] numbers)
  {
    if (numbers.Length == 0)
      throw new ArgumentException("At least one number must be provided.");

    long lcm = numbers[0];

    for (long i = 1; i < numbers.Length; i++)
    {
      lcm = CalculateLCM(lcm, numbers[i]);
    }

    return lcm;
  }

  private static long CalculateLCM(long a, int b)
  {
    return Math.Abs(a * b) / CalculateGCD(a, b);
  }

  private static long CalculateGCD(long a, long b)
  {
    while (b != 0)
    {
      var temp = b;
      b = a % b;
      a = temp;
    }

    return a;
  }
}
