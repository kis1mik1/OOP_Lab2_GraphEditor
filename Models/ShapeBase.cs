using Avalonia.Media;

namespace AnotherGraphEditor.Models;

public abstract class ShapeBase : IShape
{
    public double X { get; private set; }
    public double Y { get; private set; }
    public double Width { get; private set; }
    public double Height { get; private set; }
    public Color FillColor { get; set; } = Colors.LightBlue;
    public Color StrokeColor { get; set; } = Colors.DarkBlue;
    public double StrokeThickness { get; set; } = 2;

    protected ShapeBase(double x, double y, double width, double height)
    {
        Resize(x, y, width, height);
    }

    public void Move(double deltaX, double deltaY)
    {
        X += deltaX;
        Y += deltaY;
    }

    public void Resize(double x, double y, double width, double height)
    {
        X = x;
        Y = y;
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);
    }

    protected void CopyStyleTo(ShapeBase target)
    {
        target.FillColor = FillColor;
        target.StrokeColor = StrokeColor;
        target.StrokeThickness = StrokeThickness;
    }

    public abstract void Draw(DrawingContext context);
    public abstract bool Contains(Avalonia.Point point);
    public abstract IShape Clone();
}
