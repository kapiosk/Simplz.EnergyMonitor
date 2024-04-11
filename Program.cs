using Cronos;
using TapoConnect;

DotNetEnv.Env.Load();

var ips = Environment.GetEnvironmentVariable("TAPO_IPS") ?? throw new InvalidOperationException("Environment variable IP is not set.");
var email = Environment.GetEnvironmentVariable("TAPO_EMAIL") ?? throw new InvalidOperationException("Environment variable EMAIL is not set.");
var password = Environment.GetEnvironmentVariable("TAPO_PASSWORD") ?? throw new InvalidOperationException("Environment variable PASSWORD is not set.");
var energyMonitorCron = Environment.GetEnvironmentVariable("ENERGY_MONITOR_CRON");

TapoDeviceClient deviceClient = new([new TapoConnect.Protocol.KlapDeviceClient()]);
List<TapoDeviceKey> tapoDeviceKeys = [];
foreach (var ip in ips.Split(','))
{
    tapoDeviceKeys.Add(await deviceClient.LoginByIpAsync(ip, email, password));
}

if (string.IsNullOrEmpty(energyMonitorCron))
{
    var items = await ProcessItems(deviceClient, tapoDeviceKeys);
    foreach (var item in items)
    {
        Console.WriteLine($"{item.Nickname} - {item.CurrentPower}w - {item.LocalTime}");
    }
}
else
{
    PerioticCronTimer perioticCronTimer = new(energyMonitorCron);
    while (await perioticCronTimer.WaitForNextTickAsync())
    {
        var items = await ProcessItems(deviceClient, tapoDeviceKeys);
        foreach (var item in items)
        {
            Console.WriteLine($"{item.Nickname} - {item.CurrentPower}w - {item.LocalTime}");
        }
    }
}

static async Task<List<TapoDeviceResponse>> ProcessItems(TapoDeviceClient deviceClient, List<TapoDeviceKey> tapoDeviceKeys)
{
    List<TapoDeviceResponse> tapoDeviceResponses = [];
    foreach (var tapoDeviceKey in tapoDeviceKeys)
    {
        var energyUsageResult = await deviceClient.GetEnergyUsageAsync(tapoDeviceKey);
        var deviceGetInfoResult = await deviceClient.GetDeviceInfoAsync(tapoDeviceKey);
        tapoDeviceResponses.Add(new(deviceGetInfoResult.Nickname, energyUsageResult.CurrentPower / 1000, energyUsageResult.LocalTime));
    }
    return tapoDeviceResponses;
}

record TapoDeviceResponse(string Nickname, float CurrentPower, DateTime LocalTime);

class PerioticCronTimer(string cronExpression)
{
    private readonly CronExpression _expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

    public async ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default)
    {
        DateTime? nextUtc = _expression.GetNextOccurrence(DateTime.UtcNow, true);
        if (nextUtc.HasValue)
        {
            await Task.Delay(nextUtc.Value - DateTime.UtcNow, cancellationToken);
            return true;
        }
        return false;
    }
}
