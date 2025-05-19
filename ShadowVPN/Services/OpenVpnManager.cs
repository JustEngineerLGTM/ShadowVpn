using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace ShadowVPN.Services;

public class OpenVpnManager
{
    private Process? _vpnProcess;

    public event Action<string>? OnOutputDataReceived;

    public void Connect(string configPath)
    {
        if (_vpnProcess != null)
            Disconnect();

        var openVpnPath = GetOpenVpnExecutablePath();

        _vpnProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = openVpnPath,
                Arguments = $"--config \"{configPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _vpnProcess.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                OnOutputDataReceived?.Invoke(e.Data);
        };

        _vpnProcess.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                OnOutputDataReceived?.Invoke(e.Data);
        };

        try
        {
            if (!_vpnProcess.Start()) return;
            _vpnProcess.BeginOutputReadLine();
            _vpnProcess.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            OnOutputDataReceived?.Invoke($"Ошибка запуска OpenVPN: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        foreach (var process in Process.GetProcessesByName("openvpn"))
        {
            try
            {
                process.Kill();
            }
            catch
            {
                // ignored
            }
        }

        _vpnProcess = null;
    }

    private static string GetOpenVpnExecutablePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return @"C:\Program Files\OpenVPN\bin\openvpn.exe";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "openvpn";

        throw new PlatformNotSupportedException("ОС не поддерживается");
    }
}
