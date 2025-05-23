namespace IOBox.Workers;

interface IWorker
{
    Task ExecuteAsync(CancellationToken stoppingToken);
}
