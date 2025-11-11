using System.Threading.Tasks;
namespace ShadowVPN.Services;

public static class VpnAccountService
{
    public static async Task<(bool, string?)> SaveAndFetchConfigAsync(string ip, string user, string password)
    {
        var (success, status) = await OpenVpnConfigGenerator.CreateAndFetchConfigAsync(ip, user, password);
        if (success)
            SettingsService.Save(ip, user, password);
        return (success, status);
    }
}