namespace Frigate_Helper;


public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    MqttClient mqtt= new();
    readonly EventHandler eventHandler = new();

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;

        mqtt.Event += e =>
        {
            //Handle the event
            eventHandler.Handle(e);
        };

        StatisticHelper.StatisticReady += e =>
        {
            mqtt.Publish(e.Topic,e.ToPayload());
        };

        try{
            mqtt.Connect();
        }
        catch
        {

        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(60000, stoppingToken);

            Console.WriteLine("====Forced Update====");
            eventHandler.GenerateStatistics(true);
            Console.WriteLine("===============");
            
        }
    }
}

