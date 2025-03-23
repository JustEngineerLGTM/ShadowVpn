using System;
using Avalonia;
using Avalonia.ReactiveUI;

namespace ShadowVPN.Desktop;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Устанавливаем кодировку вывода в консоль для корректного отображения UTF-8
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}