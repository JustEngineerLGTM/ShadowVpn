using System;
using System.Text;
using Avalonia;
using Avalonia.ReactiveUI;
namespace ShadowVPN.Desktop;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Устанавливаем кодировку вывода в консоль для корректного отображения UTF-8
        Console.OutputEncoding = Encoding.UTF8;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
    }
}