using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using AnotherGraphEditor.Controls;
using AnotherGraphEditor.Models;

namespace AnotherGraphEditor;

public partial class MainWindow : Window
{
    private readonly CanvasDocument _document = new();

    public MainWindow()
    {
        InitializeComponent();
        Surface.Document = _document;
        Surface.SelectionChanged += UpdateSelectionInfo;
        UpdateSelectionInfo();
    }

    private void SelectTool_Click(object? sender, RoutedEventArgs e)
    {
        Surface.Tool = DrawingTool.Select;
        StatusText.Text = "Режим: выбор и перемещение";
    }

    private void RectangleTool_Click(object? sender, RoutedEventArgs e)
    {
        Surface.Tool = DrawingTool.Rectangle;
        StatusText.Text = "Режим: рисование прямоугольника";
    }

    private void EllipseTool_Click(object? sender, RoutedEventArgs e)
    {
        Surface.Tool = DrawingTool.Ellipse;
        StatusText.Text = "Режим: рисование эллипса";
    }

    private void AddRectangle_Click(object? sender, RoutedEventArgs e)
    {
        double shift = (_document.Shapes.Count % 8) * 20;
        _document.Add(new RectangleShape(40 + shift, 40 + shift, 150, 100));
        Surface.Tool = DrawingTool.Select;
        RefreshCanvas();
        StatusText.Text = "Прямоугольник добавлен";
    }

    private void AddEllipse_Click(object? sender, RoutedEventArgs e)
    {
        double shift = (_document.Shapes.Count % 8) * 20;
        _document.Add(new EllipseShape(40 + shift, 40 + shift, 150, 100));
        Surface.Tool = DrawingTool.Select;
        RefreshCanvas();
        StatusText.Text = "Эллипс добавлен";
    }

    private void Delete_Click(object? sender, RoutedEventArgs e)
    {
        if (_document.SelectedShape == null)
            return;

        _document.DeleteSelected();
        RefreshCanvas();
        StatusText.Text = "Фигура удалена";
    }

    private void Duplicate_Click(object? sender, RoutedEventArgs e)
    {
        if (_document.SelectedShape == null)
            return;

        _document.DuplicateSelected();
        RefreshCanvas();
        StatusText.Text = "Копия создана";
    }

    private void BringToFront_Click(object? sender, RoutedEventArgs e)
    {
        if (_document.SelectedShape == null)
        {
            StatusText.Text = "Сначала выберите фигуру";
            return;
        }

        _document.BringSelectedToFront();
        RefreshCanvas();
        StatusText.Text = "Фигура перемещена на передний план";
    }

    private void SendToBack_Click(object? sender, RoutedEventArgs e)
    {
        if (_document.SelectedShape == null)
        {
            StatusText.Text = "Сначала выберите фигуру";
            return;
        }

        _document.SendSelectedToBack();
        RefreshCanvas();
        StatusText.Text = "Фигура перемещена на задний план";
    }

    private void ApplyProperties_Click(object? sender, RoutedEventArgs e)
    {
        IShape? shape = _document.SelectedShape;

        if (shape == null)
        {
            StatusText.Text = "Сначала выберите фигуру";
            return;
        }

        if (!TryParseColor(FillColorInput.Text, out Color fill) ||
            !TryParseColor(StrokeColorInput.Text, out Color stroke))
        {
            StatusText.Text = "Цвета должны быть в формате #RRGGBB";
            return;
        }

        shape.FillColor = fill;
        shape.StrokeColor = stroke;
        shape.StrokeThickness = (double)(StrokeThicknessInput.Value ?? 2m);
        Surface.InvalidateVisual();
        StatusText.Text = "Свойства изменены";
    }

    private static bool TryParseColor(string? text, out Color color)
    {
        text = text?.Trim();
        color = default;
        return text is { Length: 7 } && text[0] == '#' && Color.TryParse(text, out color);
    }

    private void RefreshCanvas()
    {
        Surface.InvalidateVisual();
        UpdateSelectionInfo();
    }

    private void UpdateSelectionInfo()
    {
        IShape? shape = _document.SelectedShape;

        if (shape == null)
        {
            SelectedInfo.Text = "Фигура не выбрана";
            return;
        }

        SelectedInfo.Text = shape is RectangleShape ? "Прямоугольник" : "Эллипс";
        FillColorInput.Text = $"#{shape.FillColor.R:X2}{shape.FillColor.G:X2}{shape.FillColor.B:X2}";
        StrokeColorInput.Text = $"#{shape.StrokeColor.R:X2}{shape.StrokeColor.G:X2}{shape.StrokeColor.B:X2}";
        StrokeThicknessInput.Value = (decimal)shape.StrokeThickness;
    }
}
