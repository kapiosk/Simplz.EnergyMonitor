using Simplz.EnergyMonitor.Services;
using Simplz.EnergyMonitor.Utilities;

DotNetEnv.Env.Load();

var ips = Environment.GetEnvironmentVariable("TAPO_IPS") ?? throw new InvalidOperationException("Environment variable IP is not set.");
var email = Environment.GetEnvironmentVariable("TAPO_EMAIL") ?? throw new InvalidOperationException("Environment variable EMAIL is not set.");
var password = Environment.GetEnvironmentVariable("TAPO_PASSWORD") ?? throw new InvalidOperationException("Environment variable PASSWORD is not set.");
var energyMonitorCron = Environment.GetEnvironmentVariable("ENERGY_MONITOR_CRON");

TapoService tapoService = await TapoService.CreateServiceAsync(ips, email, password);

if (string.IsNullOrEmpty(energyMonitorCron))
{
    var items = await tapoService.ReadDeviceInfoAsync();
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
        var items = await tapoService.ReadDeviceInfoAsync();
        foreach (var item in items)
        {
            Console.WriteLine($"{item.Nickname} - {item.CurrentPower}w - {item.LocalTime}");
        }
    }
}
