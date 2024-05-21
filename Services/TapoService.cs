using Simplz.EnergyMonitor.Models;

namespace Simplz.EnergyMonitor.Services;

internal sealed class TapoService
{
    private readonly TapoConnect.TapoDeviceClient _deviceClient;
    private readonly List<TapoConnect.TapoDeviceKey> _tapoDeviceKeys;

    private TapoService(TapoConnect.TapoDeviceClient deviceClient, List<TapoConnect.TapoDeviceKey> tapoDeviceKeys)
    {
        _deviceClient = deviceClient;
        _tapoDeviceKeys = tapoDeviceKeys;
    }

    internal static Task<TapoService> CreateServiceAsync()
    {
        var ips = Environment.GetEnvironmentVariable("TAPO_IPS")
            ?? throw new InvalidOperationException("Environment variable TAPO_IPS is not set.");
        var email = Environment.GetEnvironmentVariable("TAPO_EMAIL")
            ?? throw new InvalidOperationException("Environment variable TAPO_EMAIL is not set.");
        var password = Environment.GetEnvironmentVariable("TAPO_PASSWORD")
            ?? throw new InvalidOperationException("Environment variable TAPO_PASSWORD is not set.");
        return CreateServiceAsync(ips, email, password);
    }

    internal static async Task<TapoService> CreateServiceAsync(string ips, string email, string password)
    {
        TapoConnect.TapoDeviceClient deviceClient = new([new TapoConnect.Protocol.KlapDeviceClient()]);
        List<TapoConnect.TapoDeviceKey> tapoDeviceKeys = [];
        foreach (var ip in ips.Split(','))
        {
            try
            {
                tapoDeviceKeys.Add(await deviceClient.LoginByIpAsync(ip, email, password));
            }
            catch
            {

            }
        }
        return new TapoService(deviceClient, tapoDeviceKeys);
    }

    internal async Task<List<TapoDeviceResponse>> ReadDeviceInfoAsync()
    {
        List<TapoDeviceResponse> tapoDeviceResponses = [];
        foreach (var tapoDeviceKey in _tapoDeviceKeys)
        {
            var energyUsageResult = await _deviceClient.GetEnergyUsageAsync(tapoDeviceKey);
            var deviceGetInfoResult = await _deviceClient.GetDeviceInfoAsync(tapoDeviceKey);
            tapoDeviceResponses.Add(new(deviceGetInfoResult.Nickname, energyUsageResult.CurrentPower / 1000, energyUsageResult.LocalTime));
        }
        return tapoDeviceResponses;
    }
}
