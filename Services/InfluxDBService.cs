using InfluxDB3.Client;
using InfluxDB3.Client.Write;

namespace Simplz.EnergyMonitor.Services;

internal sealed class InfluxDBService : IDisposable
{
    private readonly InfluxDBClient _client;

    private InfluxDBService()
    {
        var authToken = Environment.GetEnvironmentVariable("INFLUXDB_TOKEN")
            ?? throw new InvalidOperationException("Environment variable INFLUXDB_TOKEN is not set.");
        var database = Environment.GetEnvironmentVariable("INFLUXDB_DATABASE")
            ?? throw new InvalidOperationException("Environment variable INFLUXDB_DATABASE is not set.");
        var hostUrl = Environment.GetEnvironmentVariable("INFLUXDB_URL")
            ?? throw new InvalidOperationException("Environment variable INFLUXDB_URL is not set.");
        _client = new InfluxDBClient(hostUrl, authToken, database: database);
    }

    internal static InfluxDBService CreateService()
    {
        return new InfluxDBService();
    }

    internal async Task WriteDataAsync(string measurement, string name, float value, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        await _client.WritePointAsync(
            PointData.Measurement(measurement)
                .SetField(name, value)
                .SetTimestamp(timestamp.ToUniversalTime()
        ), cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
