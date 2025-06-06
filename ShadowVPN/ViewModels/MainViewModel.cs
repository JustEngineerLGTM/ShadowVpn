using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using DialogHostAvalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ShadowVPN.Services;

namespace ShadowVPN.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly OpenVpnManager _vpnManager;

    public MainViewModel()
    {
        _vpnManager = new OpenVpnManager();
        ToggleConnectionCommand = ReactiveCommand.CreateFromTask(ToggleConnectionAsync);
        SaveSettingsCommand = ReactiveCommand.CreateFromTask(SaveAndFetchConfigAsync);
        var settings = SettingsService.Load();
        if (settings == null) return;
        HasConfigFile = true;
        ServerIp = settings.Value.ip;
        VpnUsername = settings.Value.username;
        VpnPassword = settings.Value.password;
    }

    [Reactive] public ConnectionStatus ConnectionStatus { get; set; }

    [Reactive]
    [Required(ErrorMessage = "IP сервера обязателен")]
    [RegularExpression(@"^(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)){3}$",
        ErrorMessage = "Неверный формат IP-адреса")]
    public string ServerIp { get; set; } = null!;

    [Required(ErrorMessage = "Укажите имя пользователя")]
    [Reactive]
    public string VpnUsername { get; set; } = null!;

    [Reactive] public string VpnPassword { get; set; } = null!;
    [Reactive] public string? StatusMessage { get; set; }
    [Reactive] public bool HasConfigFile { get; set; }
    public ICommand ToggleConnectionCommand { get; }
    public ICommand SaveSettingsCommand { get; }

    private async Task SaveAndFetchConfigAsync()
    {
        var (success, status) = await VpnAccountService.SaveAndFetchConfigAsync(ServerIp, VpnUsername, VpnPassword);
        StatusMessage = status;
        if (!success) return;
        StatusMessage = null;
        HasConfigFile = true;
        DialogHost.Close(null);
    }

    private Task ToggleConnectionAsync()
    {
        if (ConnectionStatus != ConnectionStatus.Disconnected)
        {
            _vpnManager.Disconnect();
            ConnectionStatus = ConnectionStatus.Disconnected;
            StatusMessage = "VPN отключён.";
            return Task.CompletedTask;
        }

        var cfgPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OpenVPN", "config",
                "client.ovpn")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "shadowvpn",
                "client.ovpn");

        if (!File.Exists(cfgPath))
        {
            StatusMessage = "Конфиг не найден, сохраните сначала настройки.";
        }
        else
        {
            _vpnManager.OnOutputDataReceived += VpnOutputReceived;
            ConnectionStatus = ConnectionStatus.Connecting;
            _vpnManager.Connect(cfgPath);
        }

        return Task.CompletedTask;
    }

    private void VpnOutputReceived(string line)
    {
        if (!line.Contains("Initialization Sequence Completed")) return;
        ConnectionStatus = ConnectionStatus.Connected;
        _vpnManager.OnOutputDataReceived -= VpnOutputReceived;
    }
}