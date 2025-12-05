using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Nihon.Static;

public static class Ease
{
    public static IEasingFunction QuinticEase = new QuinticEase { EasingMode = EasingMode.EaseInOut };
    public static IEasingFunction BounceEase = new BounceEase { EasingMode = EasingMode.EaseOut, Bounciness = 2, Bounces = 5 };
    public static IEasingFunction QuarticEase = new QuarticEase { EasingMode = EasingMode.EaseInOut };
}

public static class Animations
{
    public static void Height(DependencyObject Object, double From, double To, double Time, IEasingFunction Ease)
    {
        DoubleAnimation Animation = new DoubleAnimation()
        {
            Duration = TimeSpan.FromSeconds(Time),
            From = From,
            EasingFunction = Ease,
            To = To
        };
        Storyboard.SetTarget(Animation, Object);
        Storyboard.SetTargetProperty(Animation, new PropertyPath("(Panel.Height)"));
        Storyboard AnimationStoryboard = new Storyboard();
        AnimationStoryboard.Children.Add(Animation);
        AnimationStoryboard.Begin();
    }

    public static void Width(DependencyObject Object, double From, double To, double Time, IEasingFunction Ease)
    {
        DoubleAnimation Animation = new DoubleAnimation()
        {
            Duration = TimeSpan.FromSeconds(Time),
            From = From,
            EasingFunction = Ease,
            To = To
        };
        Storyboard.SetTarget(Animation, Object);
        Storyboard.SetTargetProperty(Animation, new PropertyPath("(Panel.Width)"));
        Storyboard AnimationStoryboard = new Storyboard();
        AnimationStoryboard.Children.Add(Animation);
        AnimationStoryboard.Begin();
    }

    public static void Shift(DependencyObject Object, Thickness From, Thickness To, double Time, IEasingFunction Ease)
    {
        ThicknessAnimation Animation = new ThicknessAnimation()
        {
            Duration = TimeSpan.FromSeconds(Time),
            From = From,
            EasingFunction = Ease,
            To = To,
        };
        Storyboard.SetTarget(Animation, Object);
        Storyboard.SetTargetProperty(Animation, new PropertyPath("(Panel.Margin)"));
        Storyboard AnimationStoryboard = new Storyboard();
        AnimationStoryboard.Children.Add(Animation);
        AnimationStoryboard.Begin();
    }

    public static void Fade(DependencyObject Object, double From, double To, double Time, IEasingFunction Ease)
    {
        DoubleAnimation Animation = new DoubleAnimation()
        {
            Duration = TimeSpan.FromSeconds(Time),
            From = From,
            EasingFunction = Ease,
            To = To,
        };
        Storyboard.SetTarget(Animation, Object);
        Storyboard.SetTargetProperty(Animation, new PropertyPath("(Panel.Opacity)"));
        Storyboard AnimationStoryboard = new Storyboard();
        AnimationStoryboard.Children.Add(Animation);
        AnimationStoryboard.Begin();
    }
}
