using Avalonia;
using Avalonia.Media;

namespace AnotherGraphEditor.Models;

public class RectangleShape : ShapeBase
{
    public RectangleShape(double x, double y, double width, double height)
        : base(x, y, width, height)
    {
    }

    public override void Draw(DrawingContext context)
    {
        context.DrawRectangle(
            new SolidColorBrush(FillColor),
            new Pen(new SolidColorBrush(StrokeColor), StrokeThickness),
            new Rect(X, Y, Width, Height));
    }

    public override bool Contains(Point point)
    {
        return new Rect(X, Y, Width, Height).Contains(point);
    }

    public override IShape Clone()
    {
        var copy = new RectangleShape(X, Y, Width, Height);
        CopyStyleTo(copy);
        return copy;
    }
}
