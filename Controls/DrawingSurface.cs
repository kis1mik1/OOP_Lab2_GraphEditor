using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AnotherGraphEditor.Models;

namespace AnotherGraphEditor.Controls;

public class DrawingSurface : Control
{
    private const double HandleSize = 8;
    private const double MinimumSize = 10;

    private InteractionMode _mode;
    private ResizeHandle _resizeHandle;
    private Point _startPoint;
    private Point _lastPoint;
    private Rect _startBounds;

    public CanvasDocument Document { get; set; } = new();
    public DrawingTool Tool { get; set; } = DrawingTool.Select;
    public event Action? SelectionChanged;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.DrawRectangle(Brushes.White, null, new Rect(Bounds.Size));

        foreach (IShape shape in Document.Shapes)
            shape.Draw(context);

        if (Document.SelectedShape != null)
            DrawSelection(context, Document.SelectedShape);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        Point point = e.GetPosition(this);
        _startPoint = point;
        _lastPoint = point;
        _mode = InteractionMode.None;

        if (Tool == DrawingTool.Select)
        {
            IShape? selected = Document.SelectedShape;
            _resizeHandle = selected == null ? ResizeHandle.None : FindHandle(selected, point);

            if (selected != null && _resizeHandle != ResizeHandle.None)
            {
                _startBounds = new Rect(selected.X, selected.Y, selected.Width, selected.Height);
                _mode = InteractionMode.Resizing;
            }
            else
            {
                Document.SelectAt(point);

                if (Document.SelectedShape != null)
                    _mode = InteractionMode.Moving;
            }
        }
        else
        {
            IShape shape = Tool == DrawingTool.Rectangle
                ? new RectangleShape(point.X, point.Y, 1, 1)
                : new EllipseShape(point.X, point.Y, 1, 1);

            Document.Add(shape);
            _mode = InteractionMode.Drawing;
        }

        SelectionChanged?.Invoke();
        InvalidateVisual();

        if (_mode != InteractionMode.None)
            e.Pointer.Capture(this);

        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_mode == InteractionMode.None || Document.SelectedShape == null)
            return;

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            FinishInteraction();
            e.Pointer.Capture(null);
            return;
        }

        Point point = e.GetPosition(this);
        IShape selected = Document.SelectedShape;

        if (_mode == InteractionMode.Drawing)
        {
            double x = Math.Min(_startPoint.X, point.X);
            double y = Math.Min(_startPoint.Y, point.Y);
            double width = Math.Abs(point.X - _startPoint.X);
            double height = Math.Abs(point.Y - _startPoint.Y);
            selected.Resize(x, y, width, height);
        }
        else if (_mode == InteractionMode.Moving)
        {
            selected.Move(point.X - _lastPoint.X, point.Y - _lastPoint.Y);
        }
        else if (_mode == InteractionMode.Resizing)
        {
            ResizeSelected(point);
        }

        _lastPoint = point;
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        FinishInteraction();
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        FinishInteraction();
    }

    private void FinishInteraction()
    {
        if (_mode == InteractionMode.Drawing && Document.SelectedShape is IShape shape)
        {
            if (shape.Width < MinimumSize || shape.Height < MinimumSize)
            {
                Document.DeleteSelected();
                SelectionChanged?.Invoke();
            }
        }

        _mode = InteractionMode.None;
        InvalidateVisual();
    }

    private void ResizeSelected(Point point)
    {
        if (Document.SelectedShape == null)
            return;

        double dx = point.X - _startPoint.X;
        double dy = point.Y - _startPoint.Y;
        double left = _startBounds.Left;
        double top = _startBounds.Top;
        double right = _startBounds.Right;
        double bottom = _startBounds.Bottom;

        if (_resizeHandle is ResizeHandle.TopLeft or ResizeHandle.MiddleLeft or ResizeHandle.BottomLeft)
            left = Math.Min(right - MinimumSize, left + dx);

        if (_resizeHandle is ResizeHandle.TopRight or ResizeHandle.MiddleRight or ResizeHandle.BottomRight)
            right = Math.Max(left + MinimumSize, right + dx);

        if (_resizeHandle is ResizeHandle.TopLeft or ResizeHandle.TopMiddle or ResizeHandle.TopRight)
            top = Math.Min(bottom - MinimumSize, top + dy);

        if (_resizeHandle is ResizeHandle.BottomLeft or ResizeHandle.BottomMiddle or ResizeHandle.BottomRight)
            bottom = Math.Max(top + MinimumSize, bottom + dy);

        Document.SelectedShape.Resize(left, top, right - left, bottom - top);
    }

    private void DrawSelection(DrawingContext context, IShape shape)
    {
        var outline = new Pen(Brushes.DodgerBlue, 1);
        context.DrawRectangle(outline, new Rect(shape.X, shape.Y, shape.Width, shape.Height), 0);

        foreach (var item in GetHandles(shape))
        {
            context.DrawRectangle(
                Brushes.White,
                new Pen(Brushes.DodgerBlue, 1.5),
                new Rect(item.Position.X - HandleSize / 2,
                         item.Position.Y - HandleSize / 2,
                         HandleSize, HandleSize));
        }
    }

    private ResizeHandle FindHandle(IShape shape, Point point)
    {
        ResizeHandle closest = ResizeHandle.None;
        double closestDistance = double.MaxValue;

        foreach (var item in GetHandles(shape))
        {
            var area = new Rect(item.Position.X - HandleSize,
                                item.Position.Y - HandleSize,
                                HandleSize * 2, HandleSize * 2);
            double dx = point.X - item.Position.X;
            double dy = point.Y - item.Position.Y;
            double distance = dx * dx + dy * dy;

            if (area.Contains(point) && distance < closestDistance)
            {
                closest = item.Handle;
                closestDistance = distance;
            }
        }

        return closest;
    }

    private (ResizeHandle Handle, Point Position)[] GetHandles(IShape shape)
    {
        double left = shape.X;
        double top = shape.Y;
        double right = shape.X + shape.Width;
        double bottom = shape.Y + shape.Height;
        double middleX = (left + right) / 2;
        double middleY = (top + bottom) / 2;

        return new[]
        {
            (ResizeHandle.TopLeft, new Point(left, top)),
            (ResizeHandle.TopMiddle, new Point(middleX, top)),
            (ResizeHandle.TopRight, new Point(right, top)),
            (ResizeHandle.MiddleLeft, new Point(left, middleY)),
            (ResizeHandle.MiddleRight, new Point(right, middleY)),
            (ResizeHandle.BottomLeft, new Point(left, bottom)),
            (ResizeHandle.BottomMiddle, new Point(middleX, bottom)),
            (ResizeHandle.BottomRight, new Point(right, bottom))
        };
    }

    private enum InteractionMode
    {
        None,
        Drawing,
        Moving,
        Resizing
    }

    private enum ResizeHandle
    {
        None,
        TopLeft,
        TopMiddle,
        TopRight,
        MiddleLeft,
        MiddleRight,
        BottomLeft,
        BottomMiddle,
        BottomRight
    }
}
