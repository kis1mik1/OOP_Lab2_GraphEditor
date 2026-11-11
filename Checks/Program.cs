using Avalonia;
using Avalonia.Media;
using AnotherGraphEditor.Models;

int checks = 0;
void Check(bool value, string description)
{
    if (!value)
        throw new Exception(description);
    checks++;
    Console.WriteLine("PASS: " + description);
}

IShape[] shapes =
{
    new RectangleShape(10, 20, 100, 80),
    new EllipseShape(10, 20, 100, 80)
};

foreach (IShape shape in shapes)
{
    string name = shape.GetType().Name;
    Check(shape.Contains(new Point(60, 60)), name + " contains center");
    Check(!shape.Contains(new Point(200, 200)), name + " excludes outside point");
    shape.Move(-5, 10);
    Check(shape.X == 5 &&
          shape.Y == 30 &&
          shape.Width == 100 &&
          shape.Height == 80,
        name + " movement preserves size");
    shape.Resize(20, 40, 70, 90);
    Check(shape.X == 20 &&
          shape.Y == 40 &&
          shape.Width == 70 &&
          shape.Height == 90,
        name + " resize updates bounds");
    shape.FillColor = Colors.Red;
    shape.StrokeColor = Colors.Green;
    shape.StrokeThickness = 7;
    var copy = shape.Clone();
    Check(copy.GetType() == shape.GetType() &&
          !ReferenceEquals(copy, shape),
        name + " clone is independent same-type instance");
    Check(copy.X == 20 &&
          copy.Y == 40 &&
          copy.Width == 70 &&
          copy.Height == 90 &&
          copy.FillColor == Colors.Red &&
          copy.StrokeColor == Colors.Green &&
          copy.StrokeThickness == 7,
        name + " clone preserves geometry and style");
    copy.Move(100, 100);
    copy.FillColor = Colors.Blue;
    Check(shape.X == 20 &&
          shape.Y == 40 &&
          shape.FillColor == Colors.Red,
        name + " clone mutations leave original unchanged");
    shape.Resize(0, 0, -10, 0);
    Check(shape.Width == 1 && shape.Height == 1, name + " nonpositive sizes are clamped");
}

var document = new CanvasDocument();
var rectangle = new RectangleShape(10, 20, 100, 80);
var ellipse = new EllipseShape(10, 20, 100, 80);
document.Add(rectangle);
document.Add(ellipse);
Check(document.Shapes.Count == 2 &&
      ReferenceEquals(document.SelectedShape, ellipse),
    "adding multiple shapes selects newest");
document.SelectAt(new Point(60, 60));
Check(ReferenceEquals(document.SelectedShape, ellipse), "overlap selects top shape");
document.SelectAt(new Point(11, 21));
Check(ReferenceEquals(document.SelectedShape, rectangle),
    "ellipse corner allows selection of underlying rectangle");
document.BringSelectedToFront();
document.SelectAt(new Point(60, 60));
Check(ReferenceEquals(document.SelectedShape, rectangle) &&
      ReferenceEquals(document.Shapes[^1], rectangle),
    "bring to front changes selection priority");
document.SendSelectedToBack();
document.SelectAt(new Point(60, 60));
Check(ReferenceEquals(document.SelectedShape, ellipse) &&
      ReferenceEquals(document.Shapes[0], rectangle),
    "send to back changes selection priority");
ellipse.FillColor = Colors.Red;
ellipse.StrokeColor = Colors.Green;
ellipse.StrokeThickness = 4;
document.DuplicateSelected();
var duplicate = document.SelectedShape!;
Check(document.Shapes.Count == 3 &&
      duplicate is EllipseShape &&
      !ReferenceEquals(duplicate, ellipse),
    "duplicate adds and selects independent object");
Check(duplicate.X == ellipse.X + 20 &&
      duplicate.Y == ellipse.Y + 20 &&
      duplicate.Width == ellipse.Width &&
      duplicate.Height == ellipse.Height &&
      duplicate.FillColor == ellipse.FillColor &&
      duplicate.StrokeColor == ellipse.StrokeColor &&
      duplicate.StrokeThickness == ellipse.StrokeThickness,
    "duplicate preserves style and size with offset");
document.DeleteSelected();
Check(document.Shapes.Count == 2 &&
      document.SelectedShape == null &&
      document.Shapes.Contains(rectangle) &&
      document.Shapes.Contains(ellipse),
    "delete removes only selected object");
document.SelectAt(new Point(500, 500));
Check(document.SelectedShape == null, "empty canvas point clears selection");
document.DeleteSelected();
document.DuplicateSelected();
document.BringSelectedToFront();
document.SendSelectedToBack();
Check(document.Shapes.Count == 2, "actions without selection leave document unchanged");
Console.WriteLine($"Passed {checks} model checks.");
