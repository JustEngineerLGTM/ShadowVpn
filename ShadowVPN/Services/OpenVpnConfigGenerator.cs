using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShadowVPN.Services;

public static class OpenVpnConfigGenerator
{
    private static readonly HttpClient HttpClient = new HttpClient();

    public static async Task GenerateClientConfigAsync(string username)
    {
        var apiUrl = $"http://109.120.132.39:5000/getvpnconfig?username={username}";
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var configDirectory = Path.Combine(userProfile, "OpenVPN", "config");
        var configFilePath = Path.Combine(configDirectory, "client.ovpn");

        try
        {
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Add("Accept", "*/*");

            var response = await HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var configData = await response.Content.ReadAsStringAsync();

            // Парсинг и подготовка данных
            var formattedConfig = FormatConfig(configData);

            // Сохранение отформатированного конфигурационного файла
            await File.WriteAllTextAsync(configFilePath, formattedConfig);

            Console.WriteLine($"Конфигурационный файл создан: {configFilePath}");
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"Ошибка при запросе: {httpEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static string FormatConfig(string configData)
    {
        // First, handle the JSON string format if needed
        if (configData.StartsWith("\"") && configData.EndsWith("\""))
        {
            configData = configData.Substring(1, configData.Length - 2);
        }

        // Replace all escaped newlines with actual newlines
        configData = configData.Replace("\\n", "\n");

        // Extract the certificate sections
        var caCert = ExtractCertificate(configData, "ca");
        var cert = ExtractCertificate(configData, "cert");
        var key = ExtractCertificate(configData, "key");
        var tlsAuth = ExtractTlsAuthKey(configData);

        // Aggressively clean all certificate data
        caCert = AggressiveCleanCertificate(caCert);
        cert = AggressiveCleanCertificate(cert);
        key = AggressiveCleanCertificate(key);
        tlsAuth = AggressiveCleanCertificate(tlsAuth);

        // Template for the final configuration
        var template =
            $"""
            client
            dev tun
            proto udp
            remote 109.120.132.39 1194
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

        // Format the configuration with clean certificate data
        return template;
    }

// New method for aggressive certificate cleaning
    private static string AggressiveCleanCertificate(string certData)
    {
        // Handle both visible \r characters and actual CR characters
        certData = certData.Replace("\\r", "");
        certData = certData.Replace("\r", "");

        // Split by newlines, trim each line, and rejoin with proper line endings
        var lines = certData.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        return string.Join(Environment.NewLine, lines);
    }

    private static string ExtractCertificate(string configData, string certName)
    {
        var startTag = $"<{certName}>";
        var endTag = $"</{certName}>";
        var startIndex = configData.IndexOf(startTag, StringComparison.Ordinal);

        if (startIndex < 0)
            return string.Empty;

        startIndex += startTag.Length;
        var endIndex = configData.IndexOf(endTag, startIndex, StringComparison.Ordinal);

        if (startIndex > -1 && endIndex > -1)
        {
            return configData.Substring(startIndex, endIndex - startIndex).Trim();
        }

        return string.Empty;
    }

    private static string ExtractTlsAuthKey(string configData)
    {
        var startTag = "<tls-auth>";
        var endTag = "</tls-auth>";
        var startIndex = configData.IndexOf(startTag, StringComparison.Ordinal);

        if (startIndex < 0)
            return string.Empty;

        startIndex += startTag.Length;
        var endIndex = configData.IndexOf(endTag, startIndex, StringComparison.Ordinal);

        if (startIndex > -1 && endIndex > -1)
        {
            return configData.Substring(startIndex, endIndex - startIndex).Trim();
        }

        return string.Empty;
    }
}