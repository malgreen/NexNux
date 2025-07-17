using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using NexNux.App.ViewModels;

namespace NexNux.App.Utilities;

public static class DialogHelper
{
    public static void ShowMessageDialog(string title, string message)
    {
        var dialogWindow = new Window
        {
            Title = title,
            Content = new TextBlock { Text = message }
        };

        if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;
        if (desktop.MainWindow != null) dialogWindow.ShowDialog(desktop.MainWindow);
    }

    public static Task<T>? ShowDialog<T>(Window window)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            throw new Exception("not a desktop application");
        if (desktop.MainWindow == null)
            throw new Exception("desktop does not have a main window");
        return window.ShowDialog<T>(desktop.MainWindow);
    }
}