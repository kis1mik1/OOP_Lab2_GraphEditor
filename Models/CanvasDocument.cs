using Avalonia;

namespace AnotherGraphEditor.Models;

public class CanvasDocument
{
    private readonly List<IShape> _shapes = new();

    public IReadOnlyList<IShape> Shapes => _shapes;
    public IShape? SelectedShape { get; private set; }

    public void Add(IShape shape)
    {
        _shapes.Add(shape);
        SelectedShape = shape;
    }

    public void SelectAt(Point point)
    {
        SelectedShape = null;

        for (int i = _shapes.Count - 1; i >= 0; i--)
        {
            if (_shapes[i].Contains(point))
            {
                SelectedShape = _shapes[i];
                break;
            }
        }
    }

    public void ClearSelection()
    {
        SelectedShape = null;
    }

    public void DeleteSelected()
    {
        if (SelectedShape == null)
            return;

        _shapes.Remove(SelectedShape);
        SelectedShape = null;
    }

    public void DuplicateSelected()
    {
        if (SelectedShape == null)
            return;

        IShape copy = SelectedShape.Clone();
        copy.Move(20, 20);
        Add(copy);
    }

    public void BringSelectedToFront()
    {
        if (SelectedShape == null)
            return;

        _shapes.Remove(SelectedShape);
        _shapes.Add(SelectedShape);
    }

    public void SendSelectedToBack()
    {
        if (SelectedShape == null)
            return;

        _shapes.Remove(SelectedShape);
        _shapes.Insert(0, SelectedShape);
    }
}
