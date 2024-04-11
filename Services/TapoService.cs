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

    internal static async Task<TapoService> CreateServiceAsync(string ips, string email, string password)
    {
        TapoConnect.TapoDeviceClient deviceClient = new([new TapoConnect.Protocol.KlapDeviceClient()]);
        List<TapoConnect.TapoDeviceKey> tapoDeviceKeys = [];
        foreach (var ip in ips.Split(','))
        {
            tapoDeviceKeys.Add(await deviceClient.LoginByIpAsync(ip, email, password));
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
