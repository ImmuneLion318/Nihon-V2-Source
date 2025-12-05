using System;
using System.Windows;
using System.Windows.Media;
using Nihon.ContentView;

namespace Nihon.Interfaces;

public class ThemeControls
{
    public class Border
    {
        public Brush Background;
        public Thickness BorderThickness;
        public Brush BorderBrush;
        public double Opacity;
    }
}

public class ThemeManager
{
    public static string Path = "Theme-Wpf.json";

    [Serializable]
    public class MainWindow
    {
        public string Title;

        public double Width;
        public double Heigth;

        public ThemeControls.Border Top;
    }

    public static MainWindow Get(UserInterface Interface) => new MainWindow
    {
        Title = Interface.Title,

        Width = Interface.Width,
        Heigth = Interface.Height,

        Top = new ThemeControls.Border
        {
            Background = Interface.TopBar.Background,
            BorderThickness = Interface.TopBar.BorderThickness,
            BorderBrush = Interface.TopBar.BorderBrush,
            Opacity = Interface.TopBar.Opacity
        },
    };
}
