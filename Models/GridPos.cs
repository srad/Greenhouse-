namespace Greenhouse.Models
{
  public class GridPos
  {
    public int GridX;
    public int GridY;
    public int PixelX;
    public int PixelY;
    public override string ToString() => $"GridPos(GridX={GridX}, GridY={GridY}, PixelX={PixelX}, PixelY={PixelY})";
  }
}
