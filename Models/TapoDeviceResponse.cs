namespace Simplz.EnergyMonitor.Models;

internal sealed record TapoDeviceResponse(string Nickname, float CurrentPower, DateTime LocalTime);
