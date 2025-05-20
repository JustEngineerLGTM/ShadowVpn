using System;
using System.IO;
using System.Runtime.InteropServices;
using Tomlyn;
using Tomlyn.Model;

namespace ShadowVPN.Services;

public static class SettingsService
{
    public static string GetSettingsDirectory() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ShadowVPN")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "shadowvpn");

    public static string GetSettingsFilePath() =>
        Path.Combine(GetSettingsDirectory(), "settings.toml");

    public static (string ip, string username, string password)? Load()
    {
        var path = GetSettingsFilePath();
        if (!File.Exists(path)) return null;
        var model = Toml.ToModel(File.ReadAllText(path));
        return (
            model["server_ip"]?.ToString() ?? "",
            model["username"]?.ToString() ?? "",
            model["password"]?.ToString() ?? ""
        );
    }

    public static void Save(string ip, string username, string password)
    {
        var dir = GetSettingsDirectory();
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        var table = new TomlTable
        {
            ["server_ip"] = ip,
            ["username"] = username,
            ["password"] = password
        };
        File.WriteAllText(GetSettingsFilePath(), Toml.FromModel(table));
    }
}