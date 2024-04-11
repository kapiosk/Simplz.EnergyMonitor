using Cronos;
using TapoConnect;

DotNetEnv.Env.Load();

var ips = Environment.GetEnvironmentVariable("TAPO_IPS") ?? throw new InvalidOperationException("Environment variable IP is not set.");
var email = Environment.GetEnvironmentVariable("TAPO_EMAIL") ?? throw new InvalidOperationException("Environment variable EMAIL is not set.");
var password = Environment.GetEnvironmentVariable("TAPO_PASSWORD") ?? throw new InvalidOperationException("Environment variable PASSWORD is not set.");

TapoDeviceClient deviceClient = new();
List<TapoDeviceKey> tapoDeviceKeys = [];
ips.Split(',').ToList().ForEach(async ip => tapoDeviceKeys.Add(await deviceClient.LoginByIpAsync(ips, email, password)));

PerioticCronTimer perioticCronTimer = new("*/5 * * * * *");
while (await perioticCronTimer.WaitForNextTickAsync())
{
    foreach (var tapoDeviceKey in tapoDeviceKeys)
    {
        var energyUsageResult = await deviceClient.GetEnergyUsageAsync(tapoDeviceKey);
        Console.WriteLine(energyUsageResult.CurrentPower);
        var deviceGetInfoResult = await deviceClient.GetDeviceInfoAsync(tapoDeviceKey);
        Console.WriteLine(deviceGetInfoResult.Nickname);
    }
}

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
