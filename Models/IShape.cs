using Avalonia;
using Avalonia.Media;

namespace AnotherGraphEditor.Models;

public interface IShape
{
    double X { get; }
    double Y { get; }
    double Width { get; }
    double Height { get; }
    Color FillColor { get; set; }
    Color StrokeColor { get; set; }
    double StrokeThickness { get; set; }

    void Draw(DrawingContext context);
    bool Contains(Point point);
    void Move(double deltaX, double deltaY);
    void Resize(double x, double y, double width, double height);
    IShape Clone();
}
