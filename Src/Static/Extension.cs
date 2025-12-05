using System;
using System.Diagnostics;
using FrameworkControls = System.Windows.Controls;

namespace Nihon.Static;

public static class Extension
{
    public static T Initialize<T>(this T Object, Action<T> Initializer) where T : class
    {
        Initializer(Object);
        return Object;
    }

    public static Process GetProcessByTitle(this Process[] Processes, string Title)
    {
        foreach (Process Proc in Processes)
        {
            if (Proc.MainWindowTitle == Title)
                return Proc;
        }

        return null;
    }

    public static Process GetProcessByModule(this Process[] Processes, string Name)
    {
        foreach (Process Proc in Processes)
        {
            foreach (ProcessModule Module in Proc.Modules)
                if (Module.ModuleName == Name)
                    return Proc;
        }

        return null;
    }

    public static T GetTemplateItem<T>(this FrameworkControls.Control Element, string Name) => Element.Template.FindName(Name, Element) is T CName ? CName : default(T);

}