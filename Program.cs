﻿using Simplz.EnergyMonitor.Services;
using Simplz.EnergyMonitor.Utilities;

DotNetEnv.Env.Load();

CancellationTokenSource cts = new();
cts.CancelAfter(TimeSpan.FromSeconds(5));

var energyMonitorCron = Environment.GetEnvironmentVariable("ENERGY_MONITOR_CRON"); // ?? "0/5 * * * * *"

TapoService tapoService = await TapoService.CreateServiceAsync();
using InfluxDBService influxDBService = InfluxDBService.CreateService();

if (string.IsNullOrEmpty(energyMonitorCron))
{
    var items = await tapoService.ReadDeviceInfoAsync();
    foreach (var item in items)
    {
        await influxDBService.WriteDataAsync("energy", item.Nickname, item.CurrentPower, item.LocalTime, cts.Token);
    }
}
else
{
    PerioticCronTimer perioticCronTimer = new(energyMonitorCron);
    while (await perioticCronTimer.WaitForNextTickAsync(cts.Token))
    {
        var items = await tapoService.ReadDeviceInfoAsync();
        foreach (var item in items)
        {
            await influxDBService.WriteDataAsync("energy", item.Nickname, item.CurrentPower, item.LocalTime, cts.Token);
        }
    }
}
