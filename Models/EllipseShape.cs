using Avalonia;
using Avalonia.Media;

namespace AnotherGraphEditor.Models;

public class EllipseShape : ShapeBase
{
    public EllipseShape(double x, double y, double width, double height)
        : base(x, y, width, height)
    {
    }

    public override void Draw(DrawingContext context)
    {
        context.DrawEllipse(
            new SolidColorBrush(FillColor),
            new Pen(new SolidColorBrush(StrokeColor), StrokeThickness),
            new Point(X + Width / 2, Y + Height / 2),
            Width / 2,
            Height / 2);
    }

    public override bool Contains(Point point)
    {
        double radiusX = Width / 2;
        double radiusY = Height / 2;
        double dx = (point.X - X - radiusX) / radiusX;
        double dy = (point.Y - Y - radiusY) / radiusY;
        return dx * dx + dy * dy <= 1;
    }

    public override IShape Clone()
    {
        var copy = new EllipseShape(X, Y, Width, Height);
        CopyStyleTo(copy);
        return copy;
    }
}
