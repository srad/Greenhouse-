namespace Greenhouse.Models
{
  public class SizeInfo
  {
    public double DisplayedHeight;
    public double DisplayWidth;
    public double Height;
    public double Width;
    public double Ratio => DisplayedHeight / Height;
    public override string ToString() => $"SizeInfo(DisplayedHeight={DisplayedHeight}, DisplayWidth={DisplayWidth}, Height={Height}, Width={Width}, Ratio={Ratio})";
  }
}
