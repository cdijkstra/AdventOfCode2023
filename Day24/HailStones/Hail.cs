namespace HailStones;

class Hail
{
  public long X { get; set; }
  public long Y { get; set; }
  public long Z { get; set; }
  public long Vx { get; set; }
  public long Vy { get; set; }
  public long Vz { get; set; }
  
  public long MaxAbsValue()
  {
    return Math.Max(Math.Max(Math.Abs(X), Math.Abs(Y)), Math.Abs(Z));
  }
}