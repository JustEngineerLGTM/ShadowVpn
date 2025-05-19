using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShadowVPN.Services;

public static class OpenVpnConfigGenerator
{
    private static readonly HttpClient HttpClient = new();

    public static async Task<(bool success, string? status)> CreateAndFetchConfigAsync(string serverIp, string username, string password)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
            var raw = $"{username}={hash}";
            var escapedRaw = Uri.EscapeDataString(raw);

            var createUri = $"http://{serverIp}:5000/createvpnuser?raw={escapedRaw}";
            var createRes = await HttpClient.PostAsync(createUri, null);
            if (!createRes.IsSuccessStatusCode)
                return (false, $"Ошибка ({createRes.StatusCode})");

            var getUri = $"http://{serverIp}:5000/getvpnconfig?raw={escapedRaw}";
            var getRes = await HttpClient.GetAsync(getUri);
            getRes.EnsureSuccessStatusCode();

            var rawConfig = await getRes.Content.ReadAsStringAsync();
            var formattedConfig = FormatConfig(rawConfig,serverIp);
            SaveToFile(formattedConfig);

            return (true, "Конфигурация успешно создана и сохранена.");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Сетевая ошибка: {ex.Message} {serverIp}");
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка: {ex.Message}");
        }
    }

    private static void SaveToFile(string config)
    {
        var configDir = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OpenVPN", "config")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "shadowvpn");

        var path = Path.Combine(configDir, "client.ovpn");

        Directory.CreateDirectory(configDir);
        File.WriteAllText(path, config, Encoding.UTF8);
    }

    private static string FormatConfig(string configData, string serverIp)
    {
        configData = configData.Trim('"').Replace("\\n", "\n");

        var caCert = Clean(Extract(configData, "ca"));
        var cert = Clean(Extract(configData, "cert"));
        var key = Clean(Extract(configData, "key"));
        var tlsAuth = Clean(Extract(configData, "tls-auth"));

        return $"""
                client
                dev tun
                proto udp
                remote {serverIp} 1194
                resolv-retry infinite
                nobind
                persist-key
                persist-tun

                <ca>
                {caCert}
                </ca>

                <cert>
                {cert}
                </cert>

                <key>
                {key}
                </key>

                <tls-auth>
                {tlsAuth}
                </tls-auth>

                remote-cert-tls server
                data-ciphers AES-256-GCM:AES-128-GCM:CHACHA20-POLY1305
                cipher AES-256-CBC
                key-direction 1
                auth SHA256
                verb 3
                """;
    }

    private static string Extract(string data, string tag)
    {
        var start = $"<{tag}>";
        var end = $"</{tag}>";
        var i1 = data.IndexOf(start, StringComparison.Ordinal);
        var i2 = data.IndexOf(end, StringComparison.Ordinal);

        return (i1 >= 0 && i2 > i1)
            ? data[(i1 + start.Length)..i2].Trim()
            : string.Empty;
    }

    private static string Clean(string input)
    {
        return string.Join(
            Environment.NewLine,
            input.Replace("\\r", "")
                .Replace("\r", "")
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
        );
    }
}