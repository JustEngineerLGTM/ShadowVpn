using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ShadowVPN.Services;
using ShadowVPN.ViewModels;
using ShadowVPN.Views;
namespace ShadowVPN;

public class App : Application
{
    private MainViewModel? _mainViewModel;
    private OpenVpnManager? _vpnManager;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _vpnManager = new OpenVpnManager();
        _mainViewModel = new MainViewModel();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };
            // Выключаем сервис openvpn 
            desktop.ShutdownRequested += (s, e) => _vpnManager?.Disconnect();
            desktop.Exit += (s, e) => _vpnManager?.Disconnect();

            AppDomain.CurrentDomain.ProcessExit += (s, e) => _vpnManager?.Disconnect();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = _mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}