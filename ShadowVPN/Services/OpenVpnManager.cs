using System;
using System.Diagnostics;

namespace ShadowVPN.Services;

public class OpenVpnManager
{
    private Process? _vpnProcess;

    public event Action<string>? OnOutputDataReceived;

    // Запускаем дочерний процесс openvpn с аргументом сгенерированного конфига
    public void Connect(string configPath)
    {
        if (_vpnProcess != null)
        {
            Disconnect();
        }
        
        _vpnProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = @"C:\Program Files\OpenVPN\bin\openvpn.exe",
                Arguments = $"--config \"{configPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };

       
        _vpnProcess.OutputDataReceived += (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            Debug.WriteLine($"VPN Output: {e.Data}");
            OnOutputDataReceived?.Invoke(e.Data);
        };

        _vpnProcess.ErrorDataReceived += (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            Debug.WriteLine($"VPN Error: {e.Data}");
            OnOutputDataReceived?.Invoke(e.Data);
        };

        try
        {
            if (_vpnProcess.Start())
            {
                _vpnProcess.BeginOutputReadLine();
                _vpnProcess.BeginErrorReadLine();
            }
            else
            {
                Debug.WriteLine("Ошибка: OpenVPN не запустился.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка запуска OpenVPN: {ex.Message}");
        }
    }

    // Убиваем все дерево процессов, а так же дочерний процесс openvpn,
    // потому что он не хочет диспозиться вместе с родительским элементом:)
    public void Disconnect()
    {
        foreach (var process in Process.GetProcessesByName("openvpn"))
        {
            try
            {
                process.Kill();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка закрытия OpenVPN: {ex.Message}");
            }
        }

        _vpnProcess = null;
    }
}