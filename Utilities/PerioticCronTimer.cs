namespace Simplz.EnergyMonitor.Utilities;

internal sealed class PerioticCronTimer(string cronExpression)
{
    private readonly Cronos.CronExpression _expression = Cronos.CronExpression.Parse(cronExpression, Cronos.CronFormat.IncludeSeconds);

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
