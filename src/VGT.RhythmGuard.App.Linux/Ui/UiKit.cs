using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;

namespace VGT.RhythmGuard.Linux.Ui;

/// <summary>Aurora-Glass-Designbausteine für die Avalonia-Oberfläche (1:1 zur Windows-Version).</summary>
internal static class UiKit
{
    public static readonly Color Violet = Color.Parse("#7C5CFF");
    public static readonly Color Blue = Color.Parse("#4CC9F0");
    public static readonly Color Teal = Color.Parse("#4FE3C1");
    public static readonly Color Gold = Color.Parse("#C9A7FF");
    public static readonly Color Pink = Color.Parse("#F72585");
    public static readonly Color Text = Color.Parse("#F4F2FF");
    public static readonly Color TextSecondary = Color.Parse("#B9B3DC");
    public static readonly Color TextMuted = Color.Parse("#847DAB");
    public static readonly Color Danger = Color.Parse("#FF7AA2");

    public static IBrush Solid(string hex) => new SolidColorBrush(Color.Parse(hex));
    public static IBrush Solid(Color color) => new SolidColorBrush(color);

    public static IBrush AccentGradient() => Gradient(Violet, Blue);
    public static IBrush AuroraGradient() => Gradient(Color.Parse("#151033"), Color.Parse("#241B5E"));

    public static LinearGradientBrush Gradient(Color from, Color to) => new()
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops =
        {
            new GradientStop(from, 0),
            new GradientStop(to, 1),
        },
    };

    public static TextBlock TextBlock(string text, double size = 14, FontWeight? weight = null, Color? color = null) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = weight ?? FontWeight.Normal,
        Foreground = new SolidColorBrush(color ?? Text),
        TextWrapping = TextWrapping.Wrap,
    };

    public static TextBlock Caption(string text) => TextBlock(text, 12.5, FontWeight.Normal, TextMuted);

    public static TextBlock Label(string text) => TextBlock(text, 13, FontWeight.SemiBold, TextSecondary);

    public static Border Card(Control child, double padding = 20, double radius = 18) => new()
    {
        Background = Solid("#1AFFFFFF"),
        BorderBrush = Solid("#30FFFFFF"),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(radius),
        Padding = new Thickness(padding),
        Child = child,
    };

    public static Border Inset(Control child) => new()
    {
        Background = Solid("#0DFFFFFF"),
        BorderBrush = Solid("#1AFFFFFF"),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(12),
        Padding = new Thickness(14, 10),
        Child = child,
    };

    public static Button Primary(string text, Action click)
    {
        var button = new Button
        {
            Content = TextBlock(text, 14, FontWeight.SemiBold, Color.Parse("#0B0820")),
            Background = AccentGradient(),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(22, 11),
            BorderThickness = new Thickness(0),
        };
        button.Click += (_, _) => click();
        return button;
    }

    public static Button Secondary(string text, Action click)
    {
        var button = new Button
        {
            Content = TextBlock(text, 13.5),
            Background = Solid("#1FFFFFFF"),
            BorderBrush = Solid("#30FFFFFF"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(20, 10),
        };
        button.Click += (_, _) => click();
        return button;
    }

    public static Button NavItem(string text, Action click)
    {
        var button = new Button
        {
            Content = TextBlock(text, 14, FontWeight.SemiBold, TextSecondary),
            Background = Brushes.Transparent,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(0),
        };
        button.Click += (_, _) => click();
        return button;
    }

    public static Control Spacer(double height = 12) => new Panel { Height = height };

    public static Control Horizontal(params Control[] children)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        return panel;
    }

    public static IBrush StatusColor(string mode) => mode switch
    {
        "PAUSE" or "BREAK" => Solid(Blue),
        "ABENDMODUS" or "EVENING" => Solid(Gold),
        "BETT-MODUS" or "BED" => Solid(Pink),
        _ => Solid(Teal),
    };
}
