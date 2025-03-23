using System;
using System.Diagnostics;

namespace ShadowVPN.Services;

public class OpenVpnManager
{
    private Process? _vpnProcess;
    public event Action<string>? OnOutputDataReceived;

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
            if (!string.IsNullOrEmpty(e.Data))
            {
                Debug.WriteLine($"VPN Output: {e.Data}");
                OnOutputDataReceived?.Invoke(e.Data);
            }
        };

        _vpnProcess.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Debug.WriteLine($"VPN Error: {e.Data}");
                OnOutputDataReceived?.Invoke(e.Data);
            }
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

    public void Disconnect()
    {
        if (_vpnProcess is { HasExited: false })
        {
            _vpnProcess.Kill();
        }
        _vpnProcess = null;
    }
}