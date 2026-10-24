using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.Linux.Ui;

/// <summary>Tagesrhythmus-Kurve (Avalonia-Zeichnung, Pendant zum Windows-Chart).</summary>
internal sealed class RhythmChart : Control
{
    private RhythmSettings _settings = RhythmSettings.Default;

    public RhythmSettings Settings
    {
        get => _settings;
        set
        {
            _settings = value;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width < 80 || height < 40)
        {
            return;
        }

        var total = (_settings.DayEnd.ToTimeSpan() - _settings.DayStart.ToTimeSpan()).TotalMinutes;
        if (total <= 0)
        {
            return;
        }

        const int samples = 200;
        var points = new List<Point>(samples + 2);
        for (var i = 0; i <= samples; i++)
        {
            var minutes = total * i / samples;
            var inCycle = minutes % (_settings.WorkMinutes + _settings.BreakMinutes);
            var focus = inCycle < _settings.WorkMinutes;
            var intensity = focus ? 0.85 + 0.15 * Math.Sin(minutes / 55.0) : 0.12;
            var x = width * i / samples;
            var y = 14 + (1 - Math.Clamp(intensity, 0, 1)) * (height - 22);
            points.Add(new Point(x, y));
        }

        var fill = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.FromArgb(90, 124, 92, 255), 0),
                new GradientStop(Color.FromArgb(10, 76, 201, 240), 1),
            },
        };

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(0, height), true);
            foreach (var point in points)
            {
                ctx.LineTo(point);
            }

            ctx.LineTo(new Point(width, height));
            ctx.EndFigure(true);
        }

        context.DrawGeometry(fill, null, geometry);

        var lineBrush = UiKit.Gradient(UiKit.Violet, UiKit.Teal);
        var pen = new Pen(lineBrush, 2.4);
        var line = new StreamGeometry();
        using (var ctx = line.Open())
        {
            ctx.BeginFigure(points[0], false);
            for (var i = 1; i < points.Count; i++)
            {
                ctx.LineTo(points[i]);
            }

            ctx.EndFigure(false);
        }

        context.DrawGeometry(null, pen, line);

        // Pausen-Bänder
        var bandBrush = new SolidColorBrush(Color.FromArgb(22, 79, 227, 193));
        var cycle = _settings.WorkMinutes + _settings.BreakMinutes;
        for (var start = 0d; start + _settings.WorkMinutes < total; start += cycle)
        {
            var x = width * (start + _settings.WorkMinutes) / total;
            var w = Math.Max(2, width * Math.Min(total, start + cycle) / total - x);
            context.DrawRectangle(bandBrush, null, new Rect(x, 0, w, height));
        }
    }
}

/// <summary>Rhythmus-Score-Ring.</summary>
internal sealed class ScoreRing : Control
{
    private readonly TextBlock _percent;

    public ScoreRing()
    {
        _percent = UiKit.TextBlock("0%", 30, FontWeight.Light);
        _percent.HorizontalAlignment = HorizontalAlignment.Center;
        _percent.VerticalAlignment = VerticalAlignment.Center;
    }

    public double Percent { get; set; }

    public Control CreateOverlay()
    {
        var grid = new Grid { Width = 170, Height = 170, HorizontalAlignment = HorizontalAlignment.Center };
        grid.Children.Add(this);
        var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Spacing = 2 };
        stack.Children.Add(_percent);
        stack.Children.Add(UiKit.Caption("Rhythmus-Score"));
        grid.Children.Add(stack);
        return grid;
    }

    public void SetPercent(double percent)
    {
        Percent = Math.Clamp(percent, 0, 1);
        _percent.Text = (int)Math.Round(Percent * 100) + "%";
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        var size = Math.Min(Bounds.Width, Bounds.Height);
        if (size < 40)
        {
            return;
        }

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var radius = size / 2 - 14;
        context.DrawEllipse(null, new Pen(new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)), 12), center, radius, radius);

        if (Percent <= 0.01)
        {
            return;
        }

        var sweep = Percent * 360;
        var start = new Point(center.X, center.Y - radius);
        var rad = (sweep - 90) * Math.PI / 180;
        var end = new Point(center.X + radius * Math.Cos(rad), center.Y + radius * Math.Sin(rad));
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(start, false);
            ctx.ArcTo(end, new Size(radius, radius), 180, sweep > 180, SweepDirection.Clockwise);
            ctx.EndFigure(false);
        }

        context.DrawGeometry(null, new Pen(UiKit.Gradient(UiKit.Violet, UiKit.Teal), 12), geometry);
    }
}
